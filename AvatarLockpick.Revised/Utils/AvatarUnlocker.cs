﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.System;
using static System.Net.WebRequestMethods;

namespace AvatarLockpick.Revised.Utils
{
    internal class AvatarUnlocker
    {
        //I can honestly do this better but it doesn't matter it works

        /// <summary>
        /// Starts unlock process for each type
        /// [1 = Unlock]
        /// [2 = Unlock All]
        /// [3 = Unlock VRCF]
        /// [4 = Attempt to unlock lock types from db]
        /// </summary>
        public static void Start(int Type, string UserID, string AvatarID)
        {
            Console.WriteLine("Console Loaded!");
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Title = "AvatarLockpick Debug Console";
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            try { WindowUtils.BringConsoleToFrontIfVisible(); } catch { }

            switch (Type)
            {
                case 1:
                    Unlock(UserID, AvatarID);
                    break;
                case 2:
                    UnlockAll(UserID, AvatarID);
                    break;
                case 3:
                    UnlockVRCF(UserID, AvatarID);
                    break;
                case 4:
                    UnlockDB(UserID, AvatarID);
                    break;
                default:
                    Unlock(UserID, AvatarID);
                    break;
            }
        }

        private static string GetVRChatAvatarPath(string userId)
        {
            // Get the path to AppData
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData",
                "LocalLow"
            );

            string vrchatPath = Path.Combine(appDataPath, "VRChat", "VRChat", "LocalAvatarData", userId);
            AppLog.Log("Path", $"Checking VRChat path: {vrchatPath}");
            return vrchatPath;
        }

        private static void Unlock(string UID, string AID)
        {
            AppLog.Log("Unlock", "Starting unlock process...");

            try
            {
                string avatarPath = GetVRChatAvatarPath(UID);
                string fullAvatarPath = Path.Combine(avatarPath, AID);

                AppLog.Log("Path", $"Looking for avatar at path: {fullAvatarPath}");
                AppLog.Log("Path", $"Directory exists: {Directory.Exists(avatarPath)}");

                if (Directory.Exists(avatarPath))
                {
                    AppLog.Log("Path", "Files in directory:");
                    foreach (string file in Directory.GetFiles(avatarPath))
                    {
                        AppLog.Log("Path", $"- {Path.GetFileName(file)}");
                    }
                }

                if (!System.IO.File.Exists(fullAvatarPath))
                {
                    AppLog.Error("File", $"Avatar file not found: {AID}");
                }

                string jsonContent = System.IO.File.ReadAllText(fullAvatarPath);
                AppLog.Success("Path", "Successfully read file content");
                AppLog.Log("JSON", $"Raw JSON content: {jsonContent}");

                bool wasUnlocked = false;

                try
                {
                    // First try to parse as a single object
                    JObject jsonObj = JObject.Parse(jsonContent);
                    AppLog.Success("JSON", "Successfully parsed JSON as object");

                    // Get the animationParameters array
                    var animParams = jsonObj["animationParameters"] as JArray;
                    if (animParams != null)
                    {
                        AppLog.Success("JSON", "Found animationParameters array");
                        foreach (JObject param in animParams)
                        {
                            var nameProperty = param["name"]?.ToString();
                            if (string.IsNullOrEmpty(nameProperty)) continue;

                            // Remove ALL Unicode characters
                            string normalizedName = Regex.Replace(nameProperty, @"[^\u0000-\u007F]+", "", RegexOptions.None);
                            normalizedName = normalizedName.Trim(); // Also trim any remaining whitespace

                            AppLog.Log("JSON", $"Checking parameter: {nameProperty} (normalized: {normalizedName})");

                            if (normalizedName.Equals("locked", StringComparison.OrdinalIgnoreCase))
                            {
                                AppLog.Log("JSON", $"Found locked parameter with name: {nameProperty}");
                                var valueToken = param["value"];

                                if (valueToken != null)
                                {
                                    AppLog.Log("JSON", $"Current value: {valueToken}");
                                    if (valueToken.Type == JTokenType.Integer && valueToken.Value<int>() == 1)
                                    {
                                        param["value"] = new JValue(0);
                                        wasUnlocked = true;
                                        AppLog.Success("JSON", "Changed value to 0");
                                        MessageBoxUtils.ShowInfo("Avatar Unlocked you can now restart your game!", "Nice!");
                                    }
                                }
                            }

                            if (normalizedName.Contains("_SecurityLockSync"))
                            {
                                param["value"] = new JValue(1);
                                wasUnlocked = true;
                                AppLog.Success("Unlocked", "SecurityLockSync and your avatar should be unlocked.");
                            }

                            if (normalizedName.Contains("_SecurityLockMenu"))
                            {
                                param["value"] = new JValue(1);
                                wasUnlocked = true;
                                AppLog.Success("Unlocked", "SecurityLockSync and your avatar should be unlocked.");
                            }
                        }
                    }
                    else
                    {
                        AppLog.Warn("JSON", "No animationParameters array found in JSON");
                    }

                    if (wasUnlocked)
                    {
                        AppLog.Warn("Avatar", "Writing changes back to file...");
                        System.IO.File.WriteAllText(fullAvatarPath, jsonObj.ToString(Newtonsoft.Json.Formatting.None));
                        AppLog.Success("Avatar", "Successfully saved changes");
                        //return true;
                    }
                }
                catch (JsonReaderException)
                {
                    // If it's not a single object, try parsing as array
                    try
                    {
                        JArray jsonArray = JArray.Parse(jsonContent);
                        AppLog.Success("JSON", "Successfully parsed JSON as array");

                        foreach (JObject item in jsonArray.Children<JObject>())
                        {
                            var nameProperty = item["name"]?.ToString();
                            if (string.IsNullOrEmpty(nameProperty)) continue;

                            string normalizedName = Regex.Replace(nameProperty, @"[^\u0000-\u007F]+", "", RegexOptions.None);

                            if (normalizedName.Equals("locked", StringComparison.OrdinalIgnoreCase))
                            {
                                AppLog.Log("JSON", $"Found locked property with name: {nameProperty}");
                                var valueToken = item["value"];

                                if (valueToken != null)
                                {
                                    AppLog.Log("JSON", $"Current value: {valueToken}");

                                    if (valueToken.Type == JTokenType.Integer && valueToken.Value<int>() != 0)
                                    {
                                        item["value"] = new JValue(0);
                                        wasUnlocked = true;
                                        AppLog.Success("JSON", "Changed integer value to 0");
                                    }
                                    else if (valueToken.Type == JTokenType.Boolean && valueToken.Value<bool>() == true)
                                    {
                                        item["value"] = new JValue(false);
                                        wasUnlocked = true;
                                        AppLog.Success("JSON", "Changed boolean value to false");
                                    }
                                    else if (valueToken.Type == JTokenType.String)
                                    {
                                        string? strValue = valueToken.Value<string>();
                                        if (!string.IsNullOrEmpty(strValue) &&
                                            (strValue.Equals("1") || strValue.Equals("true", StringComparison.OrdinalIgnoreCase)))
                                        {
                                            item["value"] = new JValue("0");
                                            wasUnlocked = true;
                                            AppLog.Success("JSON", "Changed string value to '0'");
                                            MessageBoxUtils.ShowInfo("Avatar Unlocked you can now restart your game!", "Nice!");
                                        }
                                    }
                                }
                            }
                        }

                        if (wasUnlocked)
                        {
                            AppLog.Warn("JSON", "Writing changes back to file...");
                            System.IO.File.WriteAllText(fullAvatarPath, jsonArray.ToString(Newtonsoft.Json.Formatting.None));
                            AppLog.Success("JSON", "Successfully saved changes");
                            //return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLog.Error("JSON", $"Error parsing JSON as array: {ex.Message}");
                        throw;
                    }
                }

                if (wasUnlocked)
                {
                    AppLog.Success("JSON", "Avatar unlocked!");
                    MessageBoxUtils.ShowInfo("Avatar unlocked", "Unlocked");
                }
                else
                {
                    AppLog.Warn("JSON", "No locked properties found or all properties were already unlocked");
                    MessageBoxUtils.ShowInfo("Avatar not unlocked, no value found. Maybe try again?", "Awww!");
                }
            }
            catch (Exception ex)
            {
                AppLog.Error("ERROR", $"Error: {ex.Message}");
                AppLog.Error("STACK", $"Stack trace: {ex.StackTrace}");
            }
        }

        //Need to finish.
        private static void UnlockAll(string UID, string AID)
        {
            return;
        }

        private static void UnlockVRCF(string UID, string AID)
        {
            AppLog.Log("UnlockVRCFuryLocks", "Starting unlock process...");

            try
            {
                string avatarPath = GetVRChatAvatarPath(UID);
                string fullAvatarPath = Path.Combine(avatarPath, AID);

                AppLog.Log("Path", $"Looking for avatar at path: {fullAvatarPath}");
                AppLog.Log("Path", $"Directory exists: {Directory.Exists(avatarPath)}");

                if (Directory.Exists(avatarPath))
                {
                    AppLog.Log("Path", "Files in directory:");
                    foreach (string file in Directory.GetFiles(avatarPath))
                    {
                        AppLog.Log("Path", $"- {Path.GetFileName(file)}");
                    }
                }

                if (!System.IO.File.Exists(fullAvatarPath))
                {
                    AppLog.Error("File", $"Avatar file not found: {AID}");
                }

                string jsonContent = System.IO.File.ReadAllText(fullAvatarPath);
                AppLog.Success("Path", "Successfully read file content");
                AppLog.Log("JSON", $"Raw JSON content: {jsonContent}");

                bool wasUnlocked = false;

                try
                {
                    // First try to parse as a single object
                    JObject jsonObj = JObject.Parse(jsonContent);
                    AppLog.Success("JSON", "Successfully parsed JSON as object");

                    // Get the animationParameters array
                    var animParams = jsonObj["animationParameters"] as JArray;
                    if (animParams != null)
                    {
                        AppLog.Success("JSON", "Found animationParameters array");
                        foreach (JObject param in animParams)
                        {
                            var nameProperty = param["name"]?.ToString();
                            if (string.IsNullOrEmpty(nameProperty)) continue;

                            // Remove ALL Unicode characters
                            string normalizedName = Regex.Replace(nameProperty, @"[^\u0000-\u007F]+", "", RegexOptions.None);
                            normalizedName = normalizedName.Trim(); // Also trim any remaining whitespace

                            AppLog.Log("JSON", $"Checking parameter: {nameProperty} (normalized: {normalizedName})");

                            /*if (normalizedName.Contains("_SecurityLockSync"))
                            {
                                AppLog.Log("VF", $"Found VF locked parameter with name: {nameProperty}");
                            }*/

                            if (normalizedName.Equals("locked", StringComparison.OrdinalIgnoreCase))
                            {
                                AppLog.Log("JSON", $"Found locked parameter with name: {nameProperty}");
                                var valueToken = param["value"];

                                if (valueToken != null)
                                {
                                    AppLog.Log("JSON", $"Current value: {valueToken}");
                                    if (valueToken.Type == JTokenType.Integer && valueToken.Value<int>() == 1)
                                    {
                                        param["value"] = new JValue(0);
                                        wasUnlocked = true;
                                        AppLog.Success("JSON", "Changed value to 0");
                                        MessageBoxUtils.ShowInfo("Avatar unlocked! No more VRCFury!", "Nice!");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        AppLog.Warn("JSON", "No animationParameters array found in JSON");
                    }

                    if (wasUnlocked)
                    {
                        AppLog.Warn("Avatar", "Writing changes back to file...");
                        System.IO.File.WriteAllText(fullAvatarPath, jsonObj.ToString(Newtonsoft.Json.Formatting.None));
                        AppLog.Success("Avatar", "Successfully saved changes");
                        //return true;
                    }
                }
                catch (JsonReaderException)
                {
                    // If it's not a single object, try parsing as array
                    try
                    {
                        JArray jsonArray = JArray.Parse(jsonContent);
                        AppLog.Success("JSON", "Successfully parsed JSON as array");

                        foreach (JObject item in jsonArray.Children<JObject>())
                        {
                            var nameProperty = item["name"]?.ToString();
                            if (string.IsNullOrEmpty(nameProperty)) continue;

                            string normalizedName = Regex.Replace(nameProperty, @"[^\u0000-\u007F]+", "", RegexOptions.None);

                            if (normalizedName.Equals("VRCF Lock/Lock", StringComparison.OrdinalIgnoreCase))
                            {
                                AppLog.Log("JSON", $"Found locked property with name: {nameProperty}");
                                var valueToken = item["value"];

                                if (valueToken != null)
                                {
                                    AppLog.Log("JSON", $"Current value: {valueToken}");

                                    if (valueToken.Type == JTokenType.Integer && valueToken.Value<int>() != 0)
                                    {
                                        item["value"] = new JValue(0);
                                        wasUnlocked = true;
                                        AppLog.Success("JSON", "Changed integer value to 0");
                                    }
                                    else if (valueToken.Type == JTokenType.Boolean && valueToken.Value<bool>() == true)
                                    {
                                        item["value"] = new JValue(false);
                                        wasUnlocked = true;
                                        AppLog.Success("JSON", "Changed boolean value to false");
                                    }
                                    else if (valueToken.Type == JTokenType.String)
                                    {
                                        string? strValue = valueToken.Value<string>();
                                        if (!string.IsNullOrEmpty(strValue) &&
                                            (strValue.Equals("1") || strValue.Equals("true", StringComparison.OrdinalIgnoreCase)))
                                        {
                                            item["value"] = new JValue("0");
                                            wasUnlocked = true;
                                            AppLog.Success("JSON", "Changed string value to '0'");
                                        }
                                    }
                                }
                            }
                        }

                        if (wasUnlocked)
                        {
                            AppLog.Warn("JSON", "Writing changes back to file...");
                            System.IO.File.WriteAllText(fullAvatarPath, jsonArray.ToString(Newtonsoft.Json.Formatting.None));
                            AppLog.Success("JSON", "Successfully saved changes");
                            //return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLog.Error("JSON", $"Error parsing JSON as array: {ex.Message}");
                        throw;
                    }
                }

                if (wasUnlocked)
                {
                    AppLog.Success("JSON", "Avatar unlocked!");
                    MessageBoxUtils.ShowInfo("Avatar unlocked", "Unlocked");
                }
                else
                {
                    AppLog.Warn("JSON", "No locked properties found or all properties were already unlocked");
                    MessageBoxUtils.ShowInfo("Avatar not unlocked, no value found. Maybe try again?", "Awww!");
                }
            }
            catch (Exception ex)
            {
                AppLog.Error("ERROR", $"Error: {ex.Message}");
                AppLog.Error("STACK", $"Stack trace: {ex.StackTrace}");
            }
        }

        private static List<string> AllLockTypes { get; } = [];
        private static void LoadLockTypes()
        {
            AppLog.Warn("DB", "Loading lock types from database...");
            try
            {
                AllLockTypes.Clear();
                AllLockTypes.AddRange(Program.HttpC.DownloadStringList(
                    "https://raw.githubusercontent.com/scrim-dev/AvatarLockpick/refs/heads/master/lock_types.txt"
                ));
                AppLog.Success("DB", "Loaded locks");
            }
            catch (Exception ex)
            {
                AppLog.Error("DB", $"{ex.Message}");
            }
        }

        private static void UnlockDB(string UID, string AID)
        {
            AppLog.Warn("UnlockWithDatabaseScan", "Preparing...");
            LoadLockTypes();
            AppLog.Log("Locks", "All possible lock types:");
            foreach (var lockType in AllLockTypes)
            {
                Console.WriteLine(lockType);
            }

            AppLog.Log("UnlockWithDatabaseScan", "Starting unlock process...");

            try
            {
                string avatarPath = GetVRChatAvatarPath(UID);
                string fullAvatarPath = Path.Combine(avatarPath, AID);

                AppLog.Log("Path", $"Looking for avatar at path: {fullAvatarPath}");
                AppLog.Log("Path", $"Directory exists: {Directory.Exists(avatarPath)}");

                if (Directory.Exists(avatarPath))
                {
                    AppLog.Log("Path", "Files in directory:");
                    foreach (string file in Directory.GetFiles(avatarPath))
                    {
                        AppLog.Log("Path", $"- {Path.GetFileName(file)}");
                    }
                }

                if (!System.IO.File.Exists(fullAvatarPath))
                {
                    AppLog.Error("File", $"Avatar file not found: {AID}");
                }

                string jsonContent = System.IO.File.ReadAllText(fullAvatarPath);
                AppLog.Success("Path", "Successfully read file content");
                AppLog.Log("JSON", $"Raw JSON content: {jsonContent}");

                bool wasUnlocked = false;

                try
                {
                    // First try to parse as a single object
                    JObject jsonObj = JObject.Parse(jsonContent);
                    AppLog.Success("JSON", "Successfully parsed JSON as object");

                    // Get the animationParameters array
                    var animParams = jsonObj["animationParameters"] as JArray;
                    if (animParams != null)
                    {
                        AppLog.Success("JSON", "Found animationParameters array");
                        foreach (JObject param in animParams)
                        {
                            var nameProperty = param["name"]?.ToString();
                            if (string.IsNullOrEmpty(nameProperty)) continue;

                            // Remove ALL Unicode characters
                            string normalizedName = Regex.Replace(nameProperty, @"[^\u0000-\u007F]+", "", RegexOptions.None);
                            normalizedName = normalizedName.Trim(); // Also trim any remaining whitespace

                            AppLog.Log("JSON", $"Checking parameter: {nameProperty} (normalized: {normalizedName})");

                            foreach (var lockType in AllLockTypes)
                            {
                                if (normalizedName.Equals(lockType, StringComparison.OrdinalIgnoreCase))
                                {
                                    AppLog.Log("JSON", $"Found locked parameter with name: {lockType}");
                                    var valueToken = param["value"];

                                    if (valueToken != null)
                                    {
                                        AppLog.Log("JSON", $"Current value: {valueToken}");
                                        if (valueToken.Type == JTokenType.Integer && valueToken.Value<int>() == 1)
                                        {
                                            param["value"] = new JValue(0);
                                            wasUnlocked = true;
                                            AppLog.Success("JSON", "Changed value to 0");
                                        }
                                        else
                                        {
                                            param["value"] = new JValue(1);
                                            wasUnlocked = true;
                                            AppLog.Success("JSON", "Changed value to 1");
                                        }
                                        MessageBoxUtils.ShowInfo("Avatar should be unlocked. If not try again or contact me for support.", "Nice!");
                                    }
                                }

                                if (normalizedName.Contains("_SecurityLockSync"))
                                {
                                    param["value"] = new JValue(1);
                                    wasUnlocked = true;
                                    AppLog.Success("Unlocked", "SecurityLockSync and your avatar should be unlocked.");
                                }

                                if (normalizedName.Contains("_SecurityLockMenu"))
                                {
                                    param["value"] = new JValue(1);
                                    wasUnlocked = true;
                                    AppLog.Success("Unlocked", "SecurityLockSync and your avatar should be unlocked.");
                                }
                            }
                        }
                    }
                    else
                    {
                        AppLog.Warn("JSON", "No animationParameters array found in JSON");
                    }

                    if (wasUnlocked)
                    {
                        AppLog.Warn("Avatar", "Writing changes back to file...");
                        System.IO.File.WriteAllText(fullAvatarPath, jsonObj.ToString(Newtonsoft.Json.Formatting.None));
                        AppLog.Success("Avatar", "Successfully saved changes");
                        //return true;
                    }
                }
                catch (JsonReaderException)
                {
                    // If it's not a single object, try parsing as array
                    try
                    {
                        JArray jsonArray = JArray.Parse(jsonContent);
                        AppLog.Success("JSON", "Successfully parsed JSON as array");

                        foreach (JObject item in jsonArray.Children<JObject>())
                        {
                            var nameProperty = item["name"]?.ToString();
                            if (string.IsNullOrEmpty(nameProperty)) continue;

                            string normalizedName = Regex.Replace(nameProperty, @"[^\u0000-\u007F]+", "", RegexOptions.None);
                            foreach (var lockType in AllLockTypes)
                            {
                                if (normalizedName.Equals(lockType, StringComparison.OrdinalIgnoreCase))
                                {
                                    AppLog.Log("JSON", $"Found locked property with name: {nameProperty}");
                                    var valueToken = item["value"];

                                    if (valueToken != null)
                                    {
                                        AppLog.Log("JSON", $"Current value: {valueToken}");

                                        if (valueToken.Type == JTokenType.Integer && valueToken.Value<int>() != 0)
                                        {
                                            item["value"] = new JValue(0);
                                            wasUnlocked = true;
                                            AppLog.Success("JSON", "Changed integer value to 0");
                                        }
                                        else if (valueToken.Type == JTokenType.Boolean && valueToken.Value<bool>() == true)
                                        {
                                            item["value"] = new JValue(false);
                                            wasUnlocked = true;
                                            AppLog.Success("JSON", "Changed boolean value to false");
                                        }
                                        else if (valueToken.Type == JTokenType.String)
                                        {
                                            string? strValue = valueToken.Value<string>();
                                            if (!string.IsNullOrEmpty(strValue) &&
                                                (strValue.Equals("1") || strValue.Equals("true", StringComparison.OrdinalIgnoreCase)))
                                            {
                                                item["value"] = new JValue("0");
                                                wasUnlocked = true;
                                                AppLog.Success("JSON", "Changed string value to '0'");
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (wasUnlocked)
                        {
                            AppLog.Warn("JSON", "Writing changes back to file...");
                            System.IO.File.WriteAllText(fullAvatarPath, jsonArray.ToString(Newtonsoft.Json.Formatting.None));
                            AppLog.Success("JSON", "Successfully saved changes");
                            //return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLog.Error("JSON", $"Error parsing JSON as array: {ex.Message}");
                        throw;
                    }
                }

                if (wasUnlocked)
                {
                    AppLog.Success("JSON", "Avatar unlocked!");
                    MessageBoxUtils.ShowInfo("Avatar unlocked", "Unlocked");
                }
                else
                {
                    AppLog.Warn("JSON", "No locked properties found or all properties were already unlocked");
                    MessageBoxUtils.ShowInfo("Avatar not unlocked, no value found. Maybe try again?", "Awww!");
                }
            }
            catch (Exception ex)
            {
                AppLog.Error("ERROR", $"Error: {ex.Message}");
                AppLog.Error("STACK", $"Stack trace: {ex.StackTrace}");
            }
        }
    }
}