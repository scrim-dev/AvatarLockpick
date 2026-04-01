using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
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

namespace AvatarLockpick.Utils
{
    internal class AvatarUnlocker
    {
        //I can honestly do this better but it doesn't matter it works

        // Custom database settings
        private static string CustomDbUrl = "";
        private static string CustomDbPath = "";

        /// <summary>
        /// Sets custom database URL or file path for the Beta Database Unlock feature
        /// </summary>
        public static void SetCustomDatabase(string url, string path)
        {
            CustomDbUrl = url ?? "";
            CustomDbPath = path ?? "";
        }

        /// <summary>
        /// Starts unlock process for each type
        /// [1 = Unlock]
        /// [2 = Unlock All]
        /// [3 = Unlock VRCF]
        /// [4 = Attempt to unlock lock types from db (txt)]
        /// [5 = Attempt to unlock lock types from SQL db (Beta)]
        /// </summary>
        public static void Start(int Type, string UserID, string AvatarID)
        {
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

        // SQL Database lock types
        private static List<string> LockTypes = new List<string>();
        private static Dictionary<string, int> LockUnlockValues = new Dictionary<string, int>();

        private static void LoadLockTypes()
        {
            LockTypes.Clear();
            LockUnlockValues.Clear();

            // Use custom URL if set, otherwise use default URL
            string defaultDbUrl = "https://github.com/scrim-dev/AvatarLockpick-Database/raw/refs/heads/main/db/VRC_LOCKS.db";
            string dbUrl = !string.IsNullOrEmpty(CustomDbUrl) ? CustomDbUrl : defaultDbUrl;
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UI", "VRC_LOCKS.db");
            
            // If custom local path is set, use it directly instead of downloading
            bool useLocalFile = !string.IsNullOrEmpty(CustomDbPath) && System.IO.File.Exists(CustomDbPath);

            try
            {
                if (useLocalFile)
                {
                    // Use custom local database file
                    AppLog.Progress(0, "Loading local database...", "Loading SQL Database");
                    AppLog.Log("SQLDB", $"Using custom local database: {CustomDbPath}");
                    dbPath = CustomDbPath;
                }
                else
                {
                    // Download database from URL
                    AppLog.Progress(0, "Connecting to server...", "Downloading SQL Database");
                    AppLog.Log("SQLDB", $"Downloading database from: {dbUrl}");

                    // Delete old database file if it exists to avoid file locking issues
                    if (System.IO.File.Exists(dbPath))
                    {
                        try
                        {
                            System.IO.File.Delete(dbPath);
                            AppLog.Log("SQLDB", "Deleted old database file");
                        }
                        catch (Exception delEx)
                        {
                            AppLog.Warn("SQLDB", $"Could not delete old database: {delEx.Message}");
                        }
                    }

                    using (var client = new HttpClient())
                    {
                        // Disable caching
                        client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
                        {
                            NoCache = true,
                            NoStore = true
                        };
                        client.DefaultRequestHeaders.Add("Pragma", "no-cache");

                        AppLog.Progress(10, "Fetching database...", "Downloading SQL Database");

                        var response = client.GetAsync(dbUrl).GetAwaiter().GetResult();
                        if (response.IsSuccessStatusCode)
                        {
                            AppLog.Progress(40, "Downloading data...", "Downloading SQL Database");
                            var dbBytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                            
                            AppLog.Progress(70, $"Saving database ({dbBytes.Length} bytes)...", "Downloading SQL Database");
                            System.IO.File.WriteAllBytes(dbPath, dbBytes);
                            
                            AppLog.Progress(80, "Database saved!", "Downloading SQL Database");
                            AppLog.Success("SQLDB", $"Database downloaded successfully ({dbBytes.Length} bytes)");
                        }
                        else
                        {
                            AppLog.DownloadComplete();
                            AppLog.Error("SQLDB", $"Failed to download database: {response.StatusCode}");
                            return;
                        }
                    }
                }

                // Read from SQLite database
                AppLog.Progress(85, "Reading lock types...", "Processing Database");
                AppLog.Log("SQLDB", "Reading lock types from SQL database...");

                // Use full path and proper connection string
                var connectionString = new SqliteConnectionStringBuilder
                {
                    DataSource = dbPath,
                    Mode = SqliteOpenMode.ReadOnly
                }.ToString();

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    // First, list all tables to find the correct table name
                    var listTablesCmd = connection.CreateCommand();
                    listTablesCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
                    
                    string? tableName = null;
                    using (var tableReader = listTablesCmd.ExecuteReader())
                    {
                        while (tableReader.Read())
                        {
                            string tName = tableReader.GetString(0);
                            AppLog.Log("SQLDB", $"Found table: {tName}");
                            if (tName.Contains("LOCK", StringComparison.OrdinalIgnoreCase))
                            {
                                tableName = tName;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(tableName))
                    {
                        AppLog.Error("SQLDB", "No lock table found in database");
                        AppLog.DownloadComplete();
                        return;
                    }

                    AppLog.Log("SQLDB", $"Using table: {tableName}");

                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT Lock_Name, Unlock_Value FROM \"{tableName}\"";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string lockName = reader.GetString(0);
                            int unlockValue = reader.GetInt32(1);

                            if (!string.IsNullOrWhiteSpace(lockName))
                            {
                                LockTypes.Add(lockName.Trim());
                                LockUnlockValues[lockName.Trim()] = unlockValue;
                            }
                        }
                    }
                }

                AppLog.Progress(100, $"Loaded {LockTypes.Count} lock types!", "Complete");
                AppLog.Success("SQLDB", $"Loaded {LockTypes.Count} lock types from SQL database");
                
                // Longer delay to show completion before closing
                System.Threading.Thread.Sleep(1500);
                AppLog.DownloadComplete();
            }
            catch (Exception ex)
            {
                AppLog.DownloadComplete();
                AppLog.Error("SQLDB", $"Error loading SQL database: {ex.Message}");
            }
        }

        private static void UnlockDB(string UID, string AID)
        {
            AppLog.Warn("UnlockDB", "Preparing (SQL Database)...");
            LoadLockTypes();

            if (LockTypes.Count == 0)
            {
                AppLog.Error("SQLDB", "No lock types loaded from SQL database. Aborting.");
                MessageBoxUtils.ShowError("Failed to load lock types from SQL database.");
                return;
            }

            AppLog.Log("SQLDB", "Lock types from SQL database:");
            foreach (var lockType in LockTypes)
            {
                int unlockVal = LockUnlockValues.ContainsKey(lockType) ? LockUnlockValues[lockType] : 0;
                AppLog.Log("SQLDB", $"  - {lockType} (unlock value: {unlockVal})");
            }

            AppLog.Log("UnlockDB", "Starting unlock process...");

            try
            {
                string avatarPath = GetVRChatAvatarPath(UID);
                string fullAvatarPath = Path.Combine(avatarPath, AID);

                AppLog.Log("Path", $"Looking for avatar at path: {fullAvatarPath}");

                if (!System.IO.File.Exists(fullAvatarPath))
                {
                    AppLog.Error("File", $"Avatar file not found: {AID}");
                    MessageBoxUtils.ShowError($"Avatar file not found: {AID}");
                    return;
                }

                string jsonContent = System.IO.File.ReadAllText(fullAvatarPath);
                AppLog.Success("Path", "Successfully read file content");

                bool wasUnlocked = false;

                try
                {
                    JObject jsonObj = JObject.Parse(jsonContent);
                    AppLog.Success("JSON", "Successfully parsed JSON as object");

                    var animParams = jsonObj["animationParameters"] as JArray;
                    if (animParams != null)
                    {
                        AppLog.Success("JSON", "Found animationParameters array");
                        foreach (JObject param in animParams)
                        {
                            var nameProperty = param["name"]?.ToString();
                            if (string.IsNullOrEmpty(nameProperty)) continue;

                            string normalizedName = Regex.Replace(nameProperty, @"[^\u0000-\u007F]+", "", RegexOptions.None);
                            normalizedName = normalizedName.Trim();

                            foreach (var lockType in LockTypes)
                            {
                                if (normalizedName.Equals(lockType, StringComparison.OrdinalIgnoreCase) ||
                                    nameProperty.Equals(lockType, StringComparison.OrdinalIgnoreCase) ||
                                    nameProperty.Trim().Equals(lockType, StringComparison.OrdinalIgnoreCase))
                                {
                                    AppLog.Log("SQLDB", $"Found locked parameter: {nameProperty} (matches: {lockType})");
                                    var valueToken = param["value"];

                                    if (valueToken != null)
                                    {
                                        int unlockValue = LockUnlockValues.ContainsKey(lockType) ? LockUnlockValues[lockType] : 0;
                                        AppLog.Log("SQLDB", $"Current value: {valueToken}, setting to unlock value: {unlockValue}");

                                        param["value"] = new JValue(unlockValue);
                                        wasUnlocked = true;
                                        AppLog.Success("SQLDB", $"Changed value to {unlockValue}");
                                    }
                                }

                                if (normalizedName.Contains("_SecurityLockSync") || nameProperty.Contains("_SecurityLockSync"))
                                {
                                    param["value"] = new JValue(1);
                                    wasUnlocked = true;
                                    AppLog.Success("SQLDB", "Unlocked SecurityLockSync");
                                }

                                if (normalizedName.Contains("_SecurityLockMenu") || nameProperty.Contains("_SecurityLockMenu"))
                                {
                                    param["value"] = new JValue(1);
                                    wasUnlocked = true;
                                    AppLog.Success("SQLDB", "Unlocked SecurityLockMenu");
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
                        AppLog.Warn("SQLDB", "Writing changes back to file...");
                        System.IO.File.WriteAllText(fullAvatarPath, jsonObj.ToString(Newtonsoft.Json.Formatting.None));
                        AppLog.Success("SQLDB", "Successfully saved changes");
                    }
                }
                catch (JsonReaderException)
                {
                    try
                    {
                        JArray jsonArray = JArray.Parse(jsonContent);
                        AppLog.Success("JSON", "Successfully parsed JSON as array");

                        foreach (JObject item in jsonArray.Children<JObject>())
                        {
                            var nameProperty = item["name"]?.ToString();
                            if (string.IsNullOrEmpty(nameProperty)) continue;

                            string normalizedName = Regex.Replace(nameProperty, @"[^\u0000-\u007F]+", "", RegexOptions.None);
                            normalizedName = normalizedName.Trim();

                            foreach (var lockType in LockTypes)
                            {
                                if (normalizedName.Equals(lockType, StringComparison.OrdinalIgnoreCase) ||
                                    nameProperty.Equals(lockType, StringComparison.OrdinalIgnoreCase) ||
                                    nameProperty.Trim().Equals(lockType, StringComparison.OrdinalIgnoreCase))
                                {
                                    AppLog.Log("SQLDB", $"Found locked property: {nameProperty}");
                                    var valueToken = item["value"];

                                    if (valueToken != null)
                                    {
                                        int unlockValue = LockUnlockValues.ContainsKey(lockType) ? LockUnlockValues[lockType] : 0;
                                        item["value"] = new JValue(unlockValue);
                                        wasUnlocked = true;
                                        AppLog.Success("SQLDB", $"Changed value to {unlockValue}");
                                    }
                                }
                            }
                        }

                        if (wasUnlocked)
                        {
                            AppLog.Warn("SQLDB", "Writing changes back to file...");
                            System.IO.File.WriteAllText(fullAvatarPath, jsonArray.ToString(Newtonsoft.Json.Formatting.None));
                            AppLog.Success("SQLDB", "Successfully saved changes");
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
                    AppLog.Success("SQLDB", "Avatar unlocked using SQL database!");
                    MessageBoxUtils.ShowInfo("Avatar unlocked using SQL database", "Unlocked");
                }
                else
                {
                    AppLog.Warn("SQLDB", "No locked properties found or all properties were already unlocked");
                    MessageBoxUtils.ShowInfo("Avatar not unlocked, no matching lock found. Maybe try again?", "Awww!");
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