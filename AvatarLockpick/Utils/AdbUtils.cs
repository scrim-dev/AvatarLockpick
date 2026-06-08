using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AvatarLockpick.Utils
{
    internal static class AdbUtils
    {
        private static string AdbExePath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", "adb.exe");

        // Known VRChat package names for Quest/Android
        private static readonly string[] VRChatPackages =
        {
            "com.vrchat.oculus.quest",        // Meta Quest
            "com.vrchat.mobile.playstore",    // Android Mobile / Google Play
        };

        // ─── ADB Runner ─────────────────────────────────────────────────────────

        /// <summary>
        /// Runs adb with the given arguments and returns stdout + stderr combined.
        /// </summary>
        public static string RunAdb(string arguments, int timeoutMs = 20000)
        {
            if (!File.Exists(AdbExePath))
                throw new FileNotFoundException($"adb.exe not found at: {AdbExePath}");

            var psi = new ProcessStartInfo
            {
                FileName = AdbExePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            using var proc = new Process { StartInfo = psi };
            var sb = new StringBuilder();

            proc.OutputDataReceived += (_, e) => { if (e.Data != null) sb.AppendLine(e.Data); };
            proc.ErrorDataReceived += (_, e) => { if (e.Data != null) sb.AppendLine(e.Data); };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            if (!proc.WaitForExit(timeoutMs))
            {
                try { proc.Kill(); } catch { }
            }

            return sb.ToString();
        }

        // ─── Device Discovery ────────────────────────────────────────────────────

        /// <summary>
        /// Returns a list of currently connected ADB devices.
        /// </summary>
        public static List<AdbDevice> GetConnectedDevices()
        {
            var devices = new List<AdbDevice>();
            try
            {
                // Ensure ADB server is running
                RunAdb("start-server", 5000);
                string output = RunAdb("devices -l");

                foreach (var rawLine in output.Split('\n'))
                {
                    var line = rawLine.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("List of devices")) continue;

                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2) continue;

                    string id = parts[0];
                    string status = parts[1];

                    if (status != "device" && status != "unauthorized" && status != "offline") continue;

                    devices.Add(new AdbDevice
                    {
                        Id = id,
                        Status = status,
                        Model = ExtractField(line, "model:") ?? id,
                        Product = ExtractField(line, "product:") ?? "",
                    });
                }
            }
            catch (Exception ex)
            {
                AppLog.Error("ADB", $"GetConnectedDevices error: {ex.Message}");
            }
            return devices;
        }

        private static string? ExtractField(string line, string key)
        {
            int idx = line.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;
            idx += key.Length;
            int end = line.IndexOf(' ', idx);
            return end < 0 ? line.Substring(idx) : line.Substring(idx, end - idx);
        }

        // ─── VRChat Path Discovery ────────────────────────────────────────────────

        /// <summary>
        /// Locates the VRChat persistent data path on the connected device.
        /// Tries each known package name.
        /// </summary>
        public static string? FindVRChatDataPath(string deviceId)
        {
            foreach (var pkg in VRChatPackages)
            {
                string path = $"/sdcard/Android/data/{pkg}/files";
                string result = RunAdb($"-s {deviceId} shell \"ls \\\"{path}\\\" 2>&1\"");
                bool isError = result.Contains("No such file") ||
                               result.Contains("Permission denied") ||
                               result.Contains("not found");
                if (!isError && result.Trim().Length > 0)
                {
                    AppLog.Log("ADB", $"Found VRChat data path: {path}");
                    return path;
                }
            }
            // Fallback: try to locate via pm path
            foreach (var pkg in VRChatPackages)
            {
                string pmResult = RunAdb($"-s {deviceId} shell pm path {pkg}");
                if (pmResult.Contains("package:"))
                {
                    string path = $"/sdcard/Android/data/{pkg}/files";
                    AppLog.Log("ADB", $"Package found, assuming data path: {path}");
                    return path;
                }
            }
            return null;
        }

        // ─── Avatar File Listing ─────────────────────────────────────────────────

        /// <summary>
        /// Lists all avatar data files found under LocalAvatarData on the device.
        /// </summary>
        public static List<AdbAvatarFile> ListAvatarFiles(string deviceId, string basePath)
        {
            var files = new List<AdbAvatarFile>();
            try
            {
                string avatarDataPath = basePath.TrimEnd('/') + "/LocalAvatarData";

                // List user directories
                string lsUsers = RunAdb($"-s {deviceId} shell ls \"{avatarDataPath}\"");
                foreach (var rawUser in lsUsers.Split('\n'))
                {
                    var uid = rawUser.Trim();
                    if (string.IsNullOrEmpty(uid) || uid.StartsWith("[ERR]") || uid.Contains("No such")) continue;

                    string userPath = $"{avatarDataPath}/{uid}";
                    string lsFiles = RunAdb($"-s {deviceId} shell ls \"{userPath}\"");

                    foreach (var rawFile in lsFiles.Split('\n'))
                    {
                        var fn = rawFile.Trim();
                        if (string.IsNullOrEmpty(fn) || fn.StartsWith("[ERR]") || fn.Contains("No such")) continue;

                        files.Add(new AdbAvatarFile
                        {
                            UserId = uid,
                            FileName = fn,
                            RemotePath = $"{userPath}/{fn}",
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                AppLog.Error("ADB", $"ListAvatarFiles error: {ex.Message}");
            }
            return files;
        }

        // ─── Pull / Unlock / Push ────────────────────────────────────────────────

        /// <summary>
        /// Pulls a single avatar file from the device, runs the DB unlock on it,
        /// and pushes it back if any changes were made. Returns true if unlocked.
        /// </summary>
        public static bool PullUnlockPush(string deviceId, AdbAvatarFile avatarFile, string tempDir)
        {
            string localFile = Path.Combine(tempDir, avatarFile.FileName);

            try
            {
                // Pull
                string pullOut = RunAdb($"-s {deviceId} pull \"{avatarFile.RemotePath}\" \"{localFile}\"");
                if (!File.Exists(localFile))
                {
                    AppLog.Warn("ADB", $"Pull failed for {avatarFile.FileName}: {pullOut.Trim()}");
                    return false;
                }

                // Unlock
                if (!AvatarUnlocker.EnsureLockTypesLoaded())
                {
                    AppLog.Warn("ADB", "Lock types unavailable — skipping");
                    return false;
                }

                bool unlocked = AvatarUnlocker.PerformDbUnlockOnFile(localFile);

                if (unlocked)
                {
                    // Push back
                    string pushOut = RunAdb($"-s {deviceId} push \"{localFile}\" \"{avatarFile.RemotePath}\"");
                    AppLog.Success("ADB", $"Unlocked & pushed: {avatarFile.FileName} (user: {avatarFile.UserId})");
                }

                return unlocked;
            }
            catch (Exception ex)
            {
                AppLog.Error("ADB", $"PullUnlockPush error for {avatarFile.FileName}: {ex.Message}");
                return false;
            }
            finally
            {
                try { if (File.Exists(localFile)) File.Delete(localFile); } catch { }
            }
        }

        // ─── Full Device Scan ────────────────────────────────────────────────────

        /// <summary>
        /// Scans and unlocks all avatar files on the specified device.
        /// Reports progress via AppLog. Returns (scanned, unlocked) counts.
        /// </summary>
        // Known direct LocalAvatarData paths
        private static readonly string[] KnownAvatarDataPaths =
        {
            "/sdcard/Android/data/com.vrchat.oculus.quest/files/LocalAvatarData",     // Meta Quest
            "/sdcard/Android/data/com.vrchat.mobile.playstore/files/LocalAvatarData", // Android Mobile
        };

        public static (int scanned, int unlocked) ScanAndUnlockDevice(string deviceId)
        {
            AppLog.Log("ADB", $"Starting scan on device: {deviceId}");

            var allFiles = new List<AdbAvatarFile>();

            // Try known direct paths first
            foreach (var knownPath in KnownAvatarDataPaths)
            {
                string lsResult = RunAdb($"-s {deviceId} shell \"ls \\\"{knownPath}\\\" 2>&1\"");
                bool accessible = !lsResult.Contains("No such file") &&
                                  !lsResult.Contains("Permission denied") &&
                                  lsResult.Trim().Length > 0;
                if (accessible)
                {
                    AppLog.Log("ADB", $"Found LocalAvatarData at: {knownPath}");
                    allFiles.AddRange(ListAvatarFilesFromPath(deviceId, knownPath));
                }
            }

            // Fallback to package discovery if nothing found via direct paths
            if (allFiles.Count == 0)
            {
                AppLog.Log("ADB", "Direct paths not found, falling back to package discovery...");
                string? basePath = FindVRChatDataPath(deviceId);
                if (basePath == null)
                {
                    AppLog.Error("ADB", $"Could not locate VRChat data on device {deviceId}. Ensure VRChat has been launched at least once.");
                    return (0, 0);
                }
                allFiles = ListAvatarFiles(deviceId, basePath);
            }

            AppLog.Log("ADB", $"Found {allFiles.Count} avatar file(s) to scan");
            if (allFiles.Count == 0) return (0, 0);

            string tempDir = Path.Combine(Path.GetTempPath(), "ALP_ADB_Temp");
            Directory.CreateDirectory(tempDir);

            int scanned = 0, unlocked = 0;
            foreach (var file in allFiles)
            {
                scanned++;
                try { if (PullUnlockPush(deviceId, file, tempDir)) unlocked++; }
                catch (Exception ex) { AppLog.Warn("ADB", $"Skipped {file.FileName}: {ex.Message}"); }
            }

            try { Directory.Delete(tempDir, true); } catch { }
            AppLog.Success("ADB", $"Device scan complete â€” {unlocked}/{scanned} files unlocked");
            return (scanned, unlocked);
        }

        /// <summary>
        /// Lists avatar files directly from an already-resolved LocalAvatarData path.
        /// </summary>
        private static List<AdbAvatarFile> ListAvatarFilesFromPath(string deviceId, string avatarDataPath)
        {
            var files = new List<AdbAvatarFile>();
            try
            {
                string lsUsers = RunAdb($"-s {deviceId} shell ls \"{avatarDataPath}\"");
                foreach (var rawUser in lsUsers.Split('\n'))
                {
                    var uid = rawUser.Trim();
                    if (string.IsNullOrEmpty(uid) || uid.StartsWith("[ERR]") || uid.Contains("No such")) continue;
                    string userPath = $"{avatarDataPath}/{uid}";
                    string lsFiles = RunAdb($"-s {deviceId} shell ls \"{userPath}\"");
                    foreach (var rawFile in lsFiles.Split('\n'))
                    {
                        var fn = rawFile.Trim();
                        if (string.IsNullOrEmpty(fn) || fn.StartsWith("[ERR]") || fn.Contains("No such")) continue;
                        files.Add(new AdbAvatarFile { UserId = uid, FileName = fn, RemotePath = $"{userPath}/{fn}" });
                    }
                }
            }
            catch (Exception ex) { AppLog.Error("ADB", $"ListAvatarFilesFromPath error: {ex.Message}"); }
            return files;
        }
    }
    internal class AdbDevice
    {
        public string Id { get; set; } = "";
        public string Status { get; set; } = "";
        public string Model { get; set; } = "";
        public string Product { get; set; } = "";

        public bool IsReady => Status == "device";
    }

    internal class AdbAvatarFile
    {
        public string UserId { get; set; } = "";
        public string FileName { get; set; } = "";
        public string RemotePath { get; set; } = "";
    }
}
