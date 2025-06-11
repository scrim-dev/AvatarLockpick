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
                    Console.WriteLine($"Loading config from: {ConfigPath}");
                    string json = File.ReadAllText(ConfigPath);
                    Console.WriteLine($"Loaded config content: {json}");
                    var config = JsonConvert.DeserializeObject<Config>(json) ?? new Config();
                    Console.WriteLine($"Deserialized config - UserId: {config.UserId}, AvatarId: {config.AvatarId}");
                    return config;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config: {ex.Message}");
            }
            Console.WriteLine("Creating new config");
            return new Config();
        }

        public void Save()
        {
            try
            {
                string dirPath = Path.GetDirectoryName(ConfigPath)!;
                if (!Directory.Exists(dirPath))
                {
                    Console.WriteLine($"Creating config directory: {dirPath}");
                    Directory.CreateDirectory(dirPath);
                }

                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                Console.WriteLine($"Saving config to {ConfigPath}");
                Console.WriteLine($"Config content: {json}");
                File.WriteAllText(ConfigPath, json);
                Console.WriteLine("Config saved successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving config: {ex.Message}");
            }
        }

        public static void ClearConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    Console.WriteLine($"Deleting config file: {ConfigPath}");
                    File.Delete(ConfigPath);
                    Console.WriteLine("Config file deleted successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting config: {ex.Message}");
            }
        }

        public void Clear()
        {
            UserId = null;
            AvatarId = null;
            HideUserId = false;
            AutoRestart = false;
            Save();
        }
    }
} 