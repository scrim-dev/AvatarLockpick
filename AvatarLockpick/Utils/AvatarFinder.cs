using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace AvatarLockpick.Utils
{
    internal class AvatarFinder
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool WriteConsoleW(
            IntPtr hConsoleOutput,
            string lpBuffer,
            uint nNumberOfCharsToWrite,
            out uint lpNumberOfCharsWritten,
            IntPtr lpReserved);

        private const int STD_OUTPUT_HANDLE = -11;
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        private static void AppendToConsole(string message)
        {
            var handle = GetStdHandle(STD_OUTPUT_HANDLE);
            if (handle != INVALID_HANDLE_VALUE)
            {
                WriteConsoleW(handle, $"[{DateTime.Now:HH:mm:ss}] {message}\n", (uint)message.Length + 14, out _, IntPtr.Zero);
            }
            else
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
            }
        }

        //Unlocks all avatars for a user
        public static string GetVRChatAvatarPath(string userId)
        {
            // Get the path to AppData
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData",
                "LocalLow"
            );
            
            string vrchatPath = Path.Combine(appDataPath, "VRChat", "VRChat", "LocalAvatarData", userId);
            AppendToConsole($"Checking VRChat path: {vrchatPath}");
            return vrchatPath;
        }

        //Unlocks a specific avatar for a user
        public static bool UnlockSpecificAvatar(string userId, string avatarFileName)
        {
            try
            {
                string avatarPath = GetVRChatAvatarPath(userId);
                string fullAvatarPath = Path.Combine(avatarPath, avatarFileName);

                AppendToConsole($"Looking for avatar at path: {fullAvatarPath}");
                AppendToConsole($"Directory exists: {Directory.Exists(avatarPath)}");
                
                if (Directory.Exists(avatarPath))
                {
                    AppendToConsole("Files in directory:");
                    foreach (string file in Directory.GetFiles(avatarPath))
                    {
                        AppendToConsole($"- {Path.GetFileName(file)}");
                    }
                }

                if (!File.Exists(fullAvatarPath))
                {
                    throw new FileNotFoundException($"Avatar file not found: {avatarFileName}");
                }

                string jsonContent = File.ReadAllText(fullAvatarPath);
                AppendToConsole("Successfully read file content");
                AppendToConsole($"Raw JSON content: {jsonContent}");

                bool wasUnlocked = false;
                
                try
                {
                    // First try to parse as a single object
                    JObject jsonObj = JObject.Parse(jsonContent);
                    AppendToConsole("Successfully parsed JSON as object");
                    
                    // Get the animationParameters array
                    var animParams = jsonObj["animationParameters"] as JArray;
                    if (animParams != null)
                    {
                        AppendToConsole("Found animationParameters array");
                        foreach (JObject param in animParams)
                        {
                            var nameProperty = param["name"]?.ToString();
                            if (string.IsNullOrEmpty(nameProperty)) continue;

                            // Remove ALL Unicode characters
                            string normalizedName = Regex.Replace(nameProperty, @"[^\u0000-\u007F]+", "", RegexOptions.None);
                            normalizedName = normalizedName.Trim(); // Also trim any remaining whitespace
                            
                            AppendToConsole($"Checking parameter: {nameProperty} (normalized: {normalizedName})");
                            
                            if (normalizedName.Equals("locked", StringComparison.OrdinalIgnoreCase))
                            {
                                AppendToConsole($"Found locked parameter with name: {nameProperty}");
                                var valueToken = param["value"];
                                
                                if (valueToken != null)
                                {
                                    AppendToConsole($"Current value: {valueToken}");
                                    if (valueToken.Type == JTokenType.Integer && valueToken.Value<int>() == 1)
                                    {
                                        param["value"] = new JValue(0);
                                        wasUnlocked = true;
                                        AppendToConsole("Changed value to 0");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        AppendToConsole("No animationParameters array found in JSON");
                    }

                    if (wasUnlocked)
                    {
                        AppendToConsole("Writing changes back to file...");
                        File.WriteAllText(fullAvatarPath, jsonObj.ToString(Newtonsoft.Json.Formatting.None));
                        AppendToConsole("Successfully saved changes");
                        return true;
                    }
                }
                catch (JsonReaderException)
                {
                    // If it's not a single object, try parsing as array
                    try
                    {
                        JArray jsonArray = JArray.Parse(jsonContent);
                        AppendToConsole("Successfully parsed JSON as array");
                        
                        foreach (JObject item in jsonArray.Children<JObject>())
                        {
                            var nameProperty = item["name"]?.ToString();
                            if (string.IsNullOrEmpty(nameProperty)) continue;

                            string normalizedName = Regex.Replace(nameProperty, @"[^\u0000-\u007F]+", "", RegexOptions.None);
                            
                            if (normalizedName.Equals("locked", StringComparison.OrdinalIgnoreCase))
                            {
                                AppendToConsole($"Found locked property with name: {nameProperty}");
                                var valueToken = item["value"];
                                
                                if (valueToken != null)
                                {
                                    AppendToConsole($"Current value: {valueToken}");
                                    
                                    if (valueToken.Type == JTokenType.Integer && valueToken.Value<int>() != 0)
                                    {
                                        item["value"] = new JValue(0);
                                        wasUnlocked = true;
                                        AppendToConsole("Changed integer value to 0");
                                    }
                                    else if (valueToken.Type == JTokenType.Boolean && valueToken.Value<bool>() == true)
                                    {
                                        item["value"] = new JValue(false);
                                        wasUnlocked = true;
                                        AppendToConsole("Changed boolean value to false");
                                    }
                                    else if (valueToken.Type == JTokenType.String)
                                    {
                                        string? strValue = valueToken.Value<string>();
                                        if (!string.IsNullOrEmpty(strValue) &&
                                            (strValue.Equals("1") || strValue.Equals("true", StringComparison.OrdinalIgnoreCase)))
                                        {
                                            item["value"] = new JValue("0");
                                            wasUnlocked = true;
                                            AppendToConsole("Changed string value to '0'");
                                        }
                                    }
                                }
                            }
                        }

                        if (wasUnlocked)
                        {
                            AppendToConsole("Writing changes back to file...");
                            File.WriteAllText(fullAvatarPath, jsonArray.ToString(Newtonsoft.Json.Formatting.None));
                            AppendToConsole("Successfully saved changes");
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendToConsole($"Error parsing JSON as array: {ex.Message}");
                        throw;
                    }
                }

                if (wasUnlocked)
                {
                    AppendToConsole("No locked properties found or all properties were already unlocked");
                    return false;
                }
                else
                {
                    AppendToConsole("No locked properties found or all properties were already unlocked");
                    return false;
                }
            }
            catch (Exception ex)
            {
                AppendToConsole($"Error: {ex.Message}");
                AppendToConsole($"Stack trace: {ex.StackTrace}");
                throw new Exception($"Error unlocking avatar {avatarFileName}: {ex.Message}", ex);
            }
        }

        public static bool DeleteSpecificAvatar(string userId, string avatarFileName)
        {
            try
            {
                string avatarPath = GetVRChatAvatarPath(userId);
                string fullAvatarPath = Path.Combine(avatarPath, avatarFileName);

                if (!File.Exists(fullAvatarPath))
                {
                    throw new FileNotFoundException($"Avatar file not found: {avatarFileName}");
                }

                File.Delete(fullAvatarPath);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting avatar {avatarFileName}: {ex.Message}", ex);
            }
        }

        // Original method kept for backwards compatibility
        public static bool UnlockAvatars(string userId)
        {
            try
            {
                string avatarPath = GetVRChatAvatarPath(userId);
                
                if (!Directory.Exists(avatarPath))
                {
                    throw new DirectoryNotFoundException($"Avatar directory not found for user ID: {userId}");
                }

                string[] avatarFiles = Directory.GetFiles(avatarPath);
                bool anyAvatarUnlocked = false;

                foreach (string avatarFile in avatarFiles)
                {
                    try
                    {
                        anyAvatarUnlocked |= UnlockSpecificAvatar(userId, Path.GetFileName(avatarFile));
                    }
                    catch
                    {
                        // Skip files that can't be processed
                        continue;
                    }
                }

                return anyAvatarUnlocked;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error unlocking avatars: {ex.Message}", ex);
            }
        }

        public static bool OpenAvatarInNotepad(string userId, string avatarFileName)
        {
            try
            {
                string avatarPath = GetVRChatAvatarPath(userId);
                string fullAvatarPath = Path.Combine(avatarPath, avatarFileName);

                if (!File.Exists(fullAvatarPath))
                {
                    throw new FileNotFoundException($"Avatar file not found: {avatarFileName}");
                }

                Process.Start("notepad.exe", fullAvatarPath);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error opening avatar in Notepad {avatarFileName}: {ex.Message}", ex);
            }
        }

        public static bool UnlockVRCFAvatar(string userId, string avatarFileName)
        {
            try
            {
                string avatarPath = GetVRChatAvatarPath(userId);
                string fullAvatarPath = Path.Combine(avatarPath, avatarFileName);

                if (!File.Exists(fullAvatarPath))
                {
                    throw new FileNotFoundException($"Avatar file not found: {avatarFileName}");
                }

                string jsonContent = File.ReadAllText(fullAvatarPath);
                JObject jsonObj = JObject.Parse(jsonContent);
                bool modified = false;

                // Target the specific parameters directly
                var animationParameters = jsonObj["animationParameters"] as JArray;
                if (animationParameters != null)
                {
                    foreach (JObject param in animationParameters.Children<JObject>())
                    {
                        string paramName = param["name"]?.Value<string>() ?? string.Empty;

                        if (paramName.Equals("VRCF Lock/Password Version", StringComparison.OrdinalIgnoreCase))
                        {
                            param["value"] = 1;
                            modified = true;
                            AppendToConsole("Updated Password Version to 1");
                        }
                        else if (paramName.Equals("VRCF Lock/Lock", StringComparison.OrdinalIgnoreCase))
                        {
                            param["value"] = 0;
                            modified = true;
                            AppendToConsole("Updated Lock to 0");
                        }
                    }
                }

                if (modified)
                {
                    File.WriteAllText(fullAvatarPath, jsonObj.ToString(Formatting.None));
                    AppendToConsole("Successfully saved changes to avatar file");
                    return true;
                }

                AppendToConsole("No required lock parameters found in avatar file");
                return false;
            }
            catch (Exception ex)
            {
                AppendToConsole($"Error processing avatar: {ex.Message}");
                throw new Exception($"Failed to unlock avatar {avatarFileName}: {ex.Message}", ex);
            }
        }
    }
}