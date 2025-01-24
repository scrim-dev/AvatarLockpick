using System;
using System.IO;
using Newtonsoft.Json;

namespace AvatarLockpick.Utils
{
    public class Config
    {
        public string? UserId { get; set; }
        public string? AvatarId { get; set; }
        public bool HideUserId { get; set; }
        public bool AutoRestart { get; set; }

        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AvatarLockpick",
            "config.json"
        );

        public static Config Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    return JsonConvert.DeserializeObject<Config>(json) ?? new Config();
                }
            }
            catch
            {
                // If there's any error reading the config, return default
            }
            return new Config();
        }

        public void Save()
        {
            try
            {
                string dirPath = Path.GetDirectoryName(ConfigPath)!;
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch
            {
                // Silently fail if we can't save the config
            }
        }
    }
} 