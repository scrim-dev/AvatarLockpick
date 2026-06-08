using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace AvatarLockpick.Service
{
    /// <summary>
    /// Background Windows Service that periodically scans the VRChat LocalAvatarData folder
    /// and automatically unlocks avatar files using the lock database.
    /// Reads configuration from ALP_ServiceConfig.json next to the service executable.
    /// </summary>
    public class Worker(ILogger<Worker> logger) : BackgroundService
    {
        private static readonly string DefaultDbUrl =
            "https://github.com/scrim-dev/AvatarLockpick-Database/raw/refs/heads/main/db/VRC_LOCKS.db";

        private readonly List<string> _lockTypes = new();
        private readonly Dictionary<string, int> _lockUnlockValues = new();

        private ServiceConfig _config = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Ensure stdout is not buffered so the host app receives lines immediately
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            logger.LogInformation("AvatarLockpick Service starting...");

            _config = LoadConfig();
            await LoadLockDatabase(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                _config = LoadConfig();

                if (_config.Enabled && _config.Mode == "service")
                {
                    logger.LogInformation("Running scan at {time}", DateTimeOffset.Now);
                    ScanAndUnlock();
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Max(5, _config.IntervalSeconds)), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            logger.LogInformation("AvatarLockpick Service stopped.");
        }

        // ─── Config ─────────────────────────────────────────────────────────────

        private static string DataFolder
        {
            get
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ALP_data");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        private string ConfigPath => Path.Combine(DataFolder, "ALP_ServiceConfig.json");

        private ServiceConfig LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    return JsonConvert.DeserializeObject<ServiceConfig>(json) ?? new ServiceConfig();
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning("Could not read config: {msg}", ex.Message);
            }
            return new ServiceConfig();
        }

        // ─── Database ────────────────────────────────────────────────────────────

        private async Task LoadLockDatabase(CancellationToken token)
        {
            string dbPath = Path.Combine(DataFolder, "VRC_LOCKS_SVC.db");

            string dbUrl = string.IsNullOrWhiteSpace(_config.DbUrl) ? DefaultDbUrl : _config.DbUrl;
            string? localDbPath = string.IsNullOrWhiteSpace(_config.DbPath) ? null : _config.DbPath;

            // Prefer local path if set
            if (localDbPath != null && File.Exists(localDbPath))
            {
                dbPath = localDbPath;
                logger.LogInformation("Using local database: {path}", dbPath);
            }
            else
            {
                // Download DB
                try
                {
                    SqliteConnection.ClearAllPools();
                    if (File.Exists(dbPath)) File.Delete(dbPath);

                    logger.LogInformation("Downloading lock database from {url}", dbUrl);
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(60);
                    var bytes = await client.GetByteArrayAsync(dbUrl, token);
                    await File.WriteAllBytesAsync(dbPath, bytes, token);
                    logger.LogInformation("Database downloaded ({size} bytes)", bytes.Length);
                }
                catch (Exception ex)
                {
                    logger.LogError("Failed to download database: {msg}", ex.Message);
                    return;
                }
            }

            // Load lock types
            _lockTypes.Clear();
            _lockUnlockValues.Clear();

            try
            {
                var csb = new SqliteConnectionStringBuilder
                {
                    DataSource = dbPath,
                    Mode = SqliteOpenMode.ReadOnly
                };

                using var conn = new SqliteConnection(csb.ToString());
                conn.Open();

                string? tableName = null;
                var listCmd = conn.CreateCommand();
                listCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
                using (var r = listCmd.ExecuteReader())
                    while (r.Read())
                    {
                        string t = r.GetString(0);
                        if (t.Contains("LOCK", StringComparison.OrdinalIgnoreCase)) tableName = t;
                    }

                if (tableName == null) { logger.LogWarning("No lock table found in DB"); return; }

                var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT Lock_Name, Unlock_Value FROM \"{tableName}\"";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader.GetString(0).Trim();
                    int val = reader.GetInt32(1);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        _lockTypes.Add(name);
                        _lockUnlockValues[name] = val;
                    }
                }

                conn.Close();
                SqliteConnection.ClearAllPools();
                logger.LogInformation("Loaded {count} lock types from database", _lockTypes.Count);
            }
            catch (Exception ex)
            {
                logger.LogError("Error loading database: {msg}", ex.Message);
            }
        }

        // ─── Scan ────────────────────────────────────────────────────────────────

        private void ScanAndUnlock()
        {
            if (_lockTypes.Count == 0)
            {
                logger.LogWarning("Lock types not loaded — skipping scan");
                return;
            }

            string baseFolder = GetAvatarBaseFolder();
            if (!Directory.Exists(baseFolder))
            {
                logger.LogWarning("LocalAvatarData not found: {path}", baseFolder);
                return;
            }

            int scanned = 0, unlocked = 0;

            foreach (string userDir in Directory.GetDirectories(baseFolder))
                foreach (string file in Directory.GetFiles(userDir))
                {
                    scanned++;
                    try { if (UnlockFile(file)) { unlocked++; logger.LogInformation("Unlocked: {f}", Path.GetFileName(file)); } }
                    catch (Exception ex) { logger.LogWarning("Could not process {f}: {e}", Path.GetFileName(file), ex.Message); }
                }

            if (unlocked > 0)
                logger.LogInformation("Scan complete — {u}/{s} files unlocked", unlocked, scanned);
        }

        private bool UnlockFile(string filePath)
        {
            string json;
            try { json = File.ReadAllText(filePath); }
            catch { return false; }

            bool changed = false;

            void ProcessItems(IEnumerable<JObject> items)
            {
                foreach (var item in items)
                {
                    string? name = item["name"]?.ToString();
                    if (string.IsNullOrEmpty(name)) continue;

                    string normalized = Regex.Replace(name, @"[^\u0000-\u007F]+", "").Trim();

                    foreach (var lockType in _lockTypes)
                    {
                        if (normalized.Equals(lockType, StringComparison.OrdinalIgnoreCase) ||
                            name.Trim().Equals(lockType, StringComparison.OrdinalIgnoreCase))
                        {
                            item["value"] = _lockUnlockValues.TryGetValue(lockType, out int v) ? v : 0;
                            changed = true;
                        }
                    }

                    if (normalized.Contains("_SecurityLockSync") || name.Contains("_SecurityLockSync"))
                    { item["value"] = 1; changed = true; }
                    if (normalized.Contains("_SecurityLockMenu") || name.Contains("_SecurityLockMenu"))
                    { item["value"] = 1; changed = true; }
                }
            }

            try
            {
                var obj = JObject.Parse(json);
                if (obj["animationParameters"] is JArray arr) ProcessItems(arr.Children<JObject>());
                if (changed) File.WriteAllText(filePath, obj.ToString(Formatting.None));
            }
            catch (JsonReaderException)
            {
                try
                {
                    var arr = JArray.Parse(json);
                    ProcessItems(arr.Children<JObject>());
                    if (changed) File.WriteAllText(filePath, arr.ToString(Formatting.None));
                }
                catch { return false; }
            }

            return changed;
        }

        private string GetAvatarBaseFolder()
        {
            if (!string.IsNullOrEmpty(_config.WindowsCachePath))
                return Path.Combine(_config.WindowsCachePath, "LocalAvatarData");

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData", "LocalLow", "VRChat", "VRChat", "LocalAvatarData");
        }
    }

    internal class ServiceConfig
    {
        public bool Enabled { get; set; } = false;
        public string Mode { get; set; } = "service";
        public int IntervalSeconds { get; set; } = 30;
        public string DbUrl { get; set; } = "";
        public string DbPath { get; set; } = "";
        public string WindowsCachePath { get; set; } = "";
    }
}
