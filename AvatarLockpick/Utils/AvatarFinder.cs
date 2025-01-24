using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace AvatarLockpick.Utils
{
    internal class AvatarFinder
    {
        //Unlocks all avatars for a user
        public static string GetVRChatAvatarPath(string userId)
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string vrchatPath = Path.Combine(localAppData, "Low", "VRChat", "VRChat", "LocalAvatarData", userId);
            return vrchatPath;
        }

        //Unlocks a specific avatar for a user
        public static bool UnlockSpecificAvatar(string userId, string avatarFileName)
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
                var jsonObj = JObject.Parse(jsonContent);

                // Find properties that end with "locked" regardless of Unicode characters
                var properties = jsonObj.Descendants()
                    .OfType<JProperty>()
                    .Where(p => p.Name.EndsWith("locked"));

                bool wasUnlocked = false;
                foreach (var prop in properties)
                {
                    if (prop.Value.Type == JTokenType.Integer && prop.Value.Value<int>() == 1)
                    {
                        prop.Value = new JValue(0);
                        wasUnlocked = true;
                    }
                }

                if (wasUnlocked)
                {
                    File.WriteAllText(fullAvatarPath, jsonObj.ToString(Newtonsoft.Json.Formatting.None));
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
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
    }
}