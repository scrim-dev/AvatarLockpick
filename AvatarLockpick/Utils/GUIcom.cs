using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AvatarLockpick.Utils
{
    internal class GUIcom
    {
        public static async void Communication(string s, Photino.NET.PhotinoWindow window = null)
        {
            try
            {
                //Json Tokens
                JObject jsonData = JObject.Parse(s);
                JToken actionToken = jsonData["action"];
                //JToken AviInfoActionToken = jsonData["avatarInfo"];
                JToken AviIDToken = jsonData["avatarId"];
                JToken UserIDToken = jsonData["userId"];
                //JToken HelpUrlToken = jsonData["openHelpUrl"];
                JToken TypeToken = jsonData["type"];
                JToken HistoryDataToken = jsonData["payload"];

                if ((string)jsonData["type"] == "avatarInfo")
                {
                    // Deprecated: UI now triggers modal and fetchAvatarInfo.
                    // Kept for fallback if needed.
                }

                if ((string)jsonData["type"] == "openInBrowser")
                {
                    var avatarInfo = new
                    {
                        AvatarId = (string)jsonData["avatarId"],
                        UserId = (string)jsonData["userId"]
                    };
                    URLStuff.OpenUrl($"https://vrchat.com/home/avatar/{avatarInfo.AvatarId}");
                }

                if ((string)jsonData["type"] == "checkPawStatus")
                {
                    if (window != null)
                    {
                        var infoData = new { type = "appInfo", version = Program.AppVersion, devMode = Program.IsDevMode };
                        window.SendWebMessage(Newtonsoft.Json.JsonConvert.SerializeObject(infoData));
                    }
                    Task.Run(async () => {
                        await PAWUtils.CheckApiStatus();
                        if (window != null)
                        {
                            var statusData = new { type = "pawStatus", isOnline = PAWUtils.IsApiOnline };
                            window.SendWebMessage(Newtonsoft.Json.JsonConvert.SerializeObject(statusData));
                        }
                    });
                }

                if ((string)jsonData["type"] == "fetchAvatarInfo")
                {
                    string aviId = AviIDToken?.ToString();
                    Task.Run(async () => {
                        var avatar = await PAWUtils.GetAvatarAsync(aviId);
                        if (window != null)
                        {
                            var resp = new
                            {
                                type = "pawAvatarInfo",
                                avatarId = aviId,
                                found = avatar != null,
                                name = avatar?.Name,
                                authorName = avatar?.AuthorName,
                                description = avatar?.Description,
                                imageUrl = avatar?.ImageUrl ?? avatar?.ThumbnailUrl
                            };
                            window.SendWebMessage(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                        }
                    });
                }

                if ((string)jsonData["type"] == "openUrl")
                {
                    var url = (string)jsonData["url"];
                    if (!string.IsNullOrWhiteSpace(url)) URLStuff.OpenUrl(url);
                }

                if ((string)jsonData["type"] == "checkForUpdates")
                {
                    Task.Run(() => VersionChecker.CheckForUpdates());
                }

                if ((string)jsonData["type"] == "downloadUpdate")
                {
                    URLStuff.OpenUrl("https://github.com/scrim-dev/AvatarLockpick/releases/latest");
                    Task.Run(async () =>
                    {
                        await Task.Delay(1500);
                        window?.Close();
                    });
                }

                if ((string)jsonData["type"] == "openHelpUrl")
                {
                    URLStuff.OpenUrl($"https://github.com/scrim-dev/AvatarLockpick/blob/master/HELP.md");
                }

                if ((string)jsonData["type"] == "restart-novr")
                {
                    Task.Run(() =>
                    {
                        VRC.CloseVRChat();
                        Task.Delay(1000); //Just in case
                        VRC.LaunchVRChat(false);
                    });
                }

                if ((string)jsonData["type"] == "restart-vr")
                {
                    Task.Run(() =>
                    {
                        VRC.CloseVRChat();
                        Task.Delay(1000); //Just in case
                        VRC.LaunchVRChat(true);
                    });
                }

                if((string)jsonData["type"] == "exportHistory")
                {
                    File.WriteAllText($"UI/ALP_History.json", HistoryDataToken.ToString());
                    AppLog.Success("GUIcom", "History data exported successfully to UI/ALP_History.json");
                }

                if ((string)jsonData["type"] == "customDbSettings")
                {
                    string customUrl = (string)jsonData["url"] ?? "";
                    string customPath = (string)jsonData["path"] ?? "";
                    AvatarUnlocker.SetCustomDatabase(customUrl, customPath);
                    AppLog.Log("GUIcom", $"Custom database settings updated - URL: {(string.IsNullOrEmpty(customUrl) ? "(default)" : customUrl)}, Path: {(string.IsNullOrEmpty(customPath) ? "(default)" : customPath)}");
                }

                if ((string)jsonData["type"] == "linuxSettings")
                {
                    string linuxPath = (string)jsonData["path"] ?? "";
                    AvatarUnlocker.SetLinuxCachePath(linuxPath);
                    AppLog.Log("GUIcom", $"Linux custom cache path updated: {(string.IsNullOrEmpty(linuxPath) ? "(default)" : linuxPath)}");
                }

                if ((string)jsonData["type"] == "windowsCacheSettings")
                {
                    string winPath = (string)jsonData["path"] ?? "";
                    AvatarUnlocker.SetWindowsCachePath(winPath);
                    AppLog.Log("GUIcom", $"Windows cache path updated: {(string.IsNullOrEmpty(winPath) ? "(default)" : winPath)}");
                }

                if ((string)jsonData["type"] == "autoUnlockSettings")
                {
                    bool enabled = (bool)(jsonData["enabled"] ?? false);
                    string mode = (string)jsonData["mode"] ?? "inapp";
                    int interval = (int)(jsonData["interval"] ?? 30);
                    InAppAutoUnlocker.ApplySettings(enabled, mode, interval);
                    AppLog.Log("GUIcom", $"Auto Unlock settings applied: enabled={enabled}, mode={mode}, interval={interval}s");
                }

                if ((string)jsonData["type"] == "writeServiceConfig")
                {
                    try
                    {
                        var cfg = new
                        {
                            Enabled = (bool)(jsonData["enabled"] ?? false),
                            Mode = (string)jsonData["mode"] ?? "service",
                            IntervalSeconds = (int)(jsonData["interval"] ?? 30),
                            DbUrl = (string)jsonData["dbUrl"] ?? "",
                            DbPath = (string)jsonData["dbPath"] ?? "",
                            WindowsCachePath = (string)jsonData["windowsCachePath"] ?? ""
                        };
                        string cfgJson = Newtonsoft.Json.JsonConvert.SerializeObject(cfg, Newtonsoft.Json.Formatting.Indented);
                        string dataDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ALP_data");
                        System.IO.Directory.CreateDirectory(dataDir);
                        string cfgPath = System.IO.Path.Combine(dataDir, "ALP_ServiceConfig.json");
                        System.IO.File.WriteAllText(cfgPath, cfgJson);
                        AppLog.Log("GUIcom", "Service config written to ALP_data/ALP_ServiceConfig.json");
                    }
                    catch (Exception cfgEx)
                    {
                        AppLog.Warn("GUIcom", $"Could not write service config: {cfgEx.Message}");
                    }
                }

                if ((string)jsonData["type"] == "startService")
                {
                    System.Threading.Tasks.Task.Run(() => InAppAutoUnlocker.StartServiceProcess());
                }

                if ((string)jsonData["type"] == "stopService")
                {
                    System.Threading.Tasks.Task.Run(() => InAppAutoUnlocker.StopServiceProcess());
                }

                if ((string)jsonData["type"] == "openServiceLog")
                {
                    InAppAutoUnlocker.OpenServiceLog();
                }

                if ((string)jsonData["type"] == "submitFeedback")
                {
                    var feedbackData = jsonData;
                    Task.Run(() => SubmitFeedback(feedbackData));
                }

                // ─── ADB / Quest unlock handlers ────────────────────────────────────
                if ((string)jsonData["type"] == "adbGetDevices")
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            var devices = AdbUtils.GetConnectedDevices();
                            var resp = new
                            {
                                type = "adbDeviceList",
                                devices = devices.Select(d => new
                                {
                                    id = d.Id,
                                    model = d.Model,
                                    product = d.Product,
                                    status = d.Status,
                                    ready = d.IsReady
                                }).ToArray()
                            };
                            AppLog.SendToUI(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                            AppLog.Log("ADB", $"Found {devices.Count} device(s)");
                        }
                        catch (Exception ex)
                        {
                            AppLog.Error("ADB", $"GetDevices failed: {ex.Message}");
                            var err = new { type = "adbDeviceList", devices = Array.Empty<object>(), error = ex.Message };
                            AppLog.SendToUI(Newtonsoft.Json.JsonConvert.SerializeObject(err));
                        }
                    });
                }

                if ((string)jsonData["type"] == "adbScanDevice")
                {
                    string deviceId = (string)jsonData["deviceId"] ?? "";
                    if (string.IsNullOrEmpty(deviceId))
                    {
                        AppLog.Warn("ADB", "adbScanDevice: no deviceId provided");
                    }
                    else
                    {
                        Task.Run(() =>
                        {
                            try
                            {
                                AppLog.Log("ADB", $"Starting full scan on device: {deviceId}");
                                var (scanned, unlocked) = AdbUtils.ScanAndUnlockDevice(deviceId);
                                var resp = new
                                {
                                    type = "adbScanResult",
                                    deviceId,
                                    scanned,
                                    unlocked,
                                    success = true,
                                    message = $"Scan complete — {unlocked}/{scanned} files unlocked"
                                };
                                AppLog.SendToUI(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                            }
                            catch (Exception ex)
                            {
                                AppLog.Error("ADB", $"Scan failed: {ex.Message}");
                                var resp = new { type = "adbScanResult", deviceId, scanned = 0, unlocked = 0, success = false, message = ex.Message };
                                AppLog.SendToUI(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                            }
                        });
                    }
                }

                if ((string)jsonData["type"] == "adbFindPath")
                {
                    string deviceId = (string)jsonData["deviceId"] ?? "";
                    Task.Run(() =>
                    {
                        string? path = null;
                        try { path = AdbUtils.FindVRChatDataPath(deviceId); } catch { }
                        var resp = new { type = "adbPathResult", deviceId, path = path ?? "", found = path != null };
                        AppLog.SendToUI(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                    });
                }

                if ((string)jsonData["type"] == "getCacheSize")
                {
                    Task.Run(() =>
                    {
                        string baseFolder = AvatarUnlocker.GetVRChatAvatarBaseFolder();
                        long sizeBytes = 0;
                        bool exists = System.IO.Directory.Exists(baseFolder);
                        if (exists) sizeBytes = GetDirectorySize(baseFolder);
                        double sizeMB = Math.Round(sizeBytes / 1024.0 / 1024.0, 2);
                        var cacheResp = new { type = "cacheSizeResult", path = baseFolder, exists, sizeBytes, sizeMB };
                        AppLog.SendToUI(Newtonsoft.Json.JsonConvert.SerializeObject(cacheResp));
                    });
                }

                if ((string)jsonData["type"] == "deleteCache")
                {
                    Task.Run(() =>
                    {
                        string baseFolder = AvatarUnlocker.GetVRChatAvatarBaseFolder();
                        bool delSuccess = false;
                        string delMsg = "";
                        try
                        {
                            if (System.IO.Directory.Exists(baseFolder))
                            {
                                System.IO.Directory.Delete(baseFolder, true);
                                delSuccess = true;
                                delMsg = "LocalAvatarData folder deleted successfully.";
                                AppLog.Success("Cache", delMsg);
                            }
                            else
                            {
                                delMsg = "LocalAvatarData folder not found.";
                                AppLog.Warn("Cache", delMsg);
                            }
                        }
                        catch (Exception delEx)
                        {
                            delMsg = $"Failed to delete cache: {delEx.Message}";
                            AppLog.Error("Cache", delMsg);
                        }
                        var delResp = new { type = "cacheDeleteResult", success = delSuccess, message = delMsg };
                        AppLog.SendToUI(Newtonsoft.Json.JsonConvert.SerializeObject(delResp));
                    });
                }
                if ((string)jsonData["type"] == "openExplorer" && window != null)
                {
                    OscExplorer.CurrentAvatarId = (string)jsonData["avatarId"] ?? "";
                    OscExplorer.CurrentUserId   = (string)jsonData["userId"]   ?? "";
                    AppLog.Log("Explorer", $"Opening Explorer for {OscExplorer.CurrentAvatarId}");
                    window.Load("UI/Explorer/index.html");
                }

                if ((string)jsonData["type"] == "explorerReady")
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            var data = OscExplorer.LoadExplorerData(OscExplorer.CurrentUserId, OscExplorer.CurrentAvatarId);
                            data["type"] = "explorerData";
                            AppLog.SendToUI(data.ToString(Newtonsoft.Json.Formatting.None));
                        }
                        catch (Exception ex)
                        {
                            var err = new { type = "explorerData", error = ex.Message };
                            AppLog.SendToUI(Newtonsoft.Json.JsonConvert.SerializeObject(err));
                        }
                    });
                }

                if ((string)jsonData["type"] == "sendOscCommand")
                {
                    string oscAddr  = (string)jsonData["address"]  ?? "";
                    string oscType  = (string)jsonData["oscType"]  ?? "float";
                    string oscValue = (string)jsonData["value"]    ?? "0";
                    Task.Run(() =>
                    {
                        bool ok = OscExplorer.SendOscCommand(oscAddr, oscType, oscValue);
                        var resp = new
                        {
                            type    = "oscCommandResult",
                            address = oscAddr,
                            success = ok,
                            message = ok ? "OSC command sent!" : "Failed to send OSC command."
                        };
                        AppLog.SendToUI(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                    });
                }

                if ((string)jsonData["type"] == "resetOscFile" && window != null)
                {
                    string rAvi = (string)jsonData["avatarId"] ?? "";
                    string rUsr = (string)jsonData["userId"]   ?? "";
                    var (ok1, msg1) = OscExplorer.DeleteOscFile(rUsr, rAvi);
                    var res1 = new { type = "resetFileResult", target = "osc", success = ok1, message = msg1 };
                    window.SendWebMessage(Newtonsoft.Json.JsonConvert.SerializeObject(res1));
                }

                if ((string)jsonData["type"] == "resetAvatarCache" && window != null)
                {
                    string rAvi = (string)jsonData["avatarId"] ?? "";
                    string rUsr = (string)jsonData["userId"]   ?? "";
                    var (ok2, msg2) = OscExplorer.DeleteAvatarCache(rUsr, rAvi);
                    var res2 = new { type = "resetFileResult", target = "avatar", success = ok2, message = msg2 };
                    window.SendWebMessage(Newtonsoft.Json.JsonConvert.SerializeObject(res2));
                }

                if ((string)jsonData["type"] == "closeExplorer" && window != null)
                {
                    AppLog.Log("Explorer", "Closing Explorer, returning to main UI");
                    window.Load("UI/index.html");
                }

                if ((string)jsonData["type"] == "closeApp" && window != null)
                {
                    bool stopSvc = jsonData["stopService"]?.Value<bool>() ?? false;
                    if (stopSvc)
                    {
                        InAppAutoUnlocker.StopServiceProcess();
                    }
                    window.Close();
                    return;
                }

                if ((string)jsonData["type"] == "windowControl" && window != null)
                {
                    string ctrl = (string)jsonData["cmd"] ?? "";
                    switch (ctrl)
                    {
                        case "minimize":  window.SetMinimized(true);  break;
                        case "maximize":  window.SetMaximized(true);  break;
                        case "restore":   window.SetMaximized(false); break;
                        case "close":     window.Close();             break;
                        case "move":
                            int dx = jsonData["dx"]?.Value<int>() ?? 0;
                            int dy = jsonData["dy"]?.Value<int>() ?? 0;
                            window.SetLeft(window.Left + dx);
                            window.SetTop(window.Top + dy);
                            break;
                    }
                    return;
                }

                if (actionToken != null && actionToken.Type == JTokenType.String)
                {
                    string action = actionToken.ToString();
                    switch (action)
                    {
                        case "Unlock":
                            MessageBoxUtils.Show("Press OK to unlock the avatar.", "Unlock Avatar", 0x00000000U);
                            AvatarUnlocker.Start(1, UserIDToken.ToString(), AviIDToken.ToString());
                            AppLog.SendToUI(Newtonsoft.Json.JsonConvert.SerializeObject(new { type = "unlockComplete" }));
                            break;
                        case "Unlock All":
                            MessageBoxUtils.Show("Press OK to unlock all avatars", "Unlock All Avatars", 0x00000000U);
                            AvatarUnlocker.Start(2, UserIDToken.ToString(), AviIDToken.ToString());
                            AppLog.SendToUI(Newtonsoft.Json.JsonConvert.SerializeObject(new { type = "unlockComplete" }));
                            break;
                        case "Unlock (VRCFury)":
                            MessageBoxUtils.Show("Press OK to unlock VRCFury locked avatar", "Unlock VRCFury", 0x00000000U);
                            AvatarUnlocker.Start(3, UserIDToken.ToString(), AviIDToken.ToString());
                            AppLog.SendToUI(Newtonsoft.Json.JsonConvert.SerializeObject(new { type = "unlockComplete" }));
                            break;
                        case "Database Unlock":
                            MessageBoxUtils.Show("Press OK to try unlocking the lock using the SQL database", "Unlock via SQL DB", 0x00000000U);
                            AvatarUnlocker.Start(4, UserIDToken.ToString(), AviIDToken.ToString());
                            AppLog.SendToUI(Newtonsoft.Json.JsonConvert.SerializeObject(new { type = "unlockComplete" }));
                            break;
                        case "openAppDirectory":
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                            {
                                FileName = AppDomain.CurrentDomain.BaseDirectory,
                                UseShellExecute = true
                            });
                            break;
                        default:
                            // Handle unknown action
                            MessageBoxUtils.ShowInfo($"Unknown action: {action}");
                            break;
                    }
                }
            }
            catch (JsonReaderException ex)
            {
                // Handle invalid JSON format
                MessageBoxUtils.ShowError($"Error parsing JSON with Newtonsoft.Json: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other potential errors
                MessageBoxUtils.ShowError($"An unexpected error occurred: {ex.Message}");
            }
        }
        private static long GetDirectorySize(string dir)
        {
            long total = 0;
            try
            {
                foreach (var f in System.IO.Directory.EnumerateFiles(dir, "*", System.IO.SearchOption.AllDirectories))
                    total += new System.IO.FileInfo(f).Length;
            }
            catch { }
            return total;
        }
        private static void SubmitFeedback(JObject data)
        {
            try
            {
                string name     = (string)data["name"]     ?? "Anonymous";
                string email    = (string)data["email"]    ?? "";
                string subject  = (string)data["subject"]  ?? "Feedback";
                string body     = (string)data["body"]     ?? "";
                string category = (string)data["category"] ?? "feedback";

                string contactInfo = string.IsNullOrEmpty(email)
                    ? name
                    : $"{name} <{email}>";

                SentrySdk.CaptureMessage(
                    $"[{category.ToUpper()}] {subject}\n" +
                    $"From: {contactInfo} | v{Program.AppVersion}\n\n" +
                    body,
                    SentryLevel.Info
                );

                var result = new { type = "feedbackResult", success = true, message = "Feedback submitted. Thank you!" };
                AppLog.SendToUI(Newtonsoft.Json.JsonConvert.SerializeObject(result));
                AppLog.Success("Feedback", $"User feedback submitted: [{category}] {subject}");
            }
            catch (Exception ex)
            {
                var result = new { type = "feedbackResult", success = false, message = $"Failed to submit: {ex.Message}" };
                AppLog.SendToUI(Newtonsoft.Json.JsonConvert.SerializeObject(result));
                AppLog.Warn("Feedback", $"Feedback submit failed: {ex.Message}");
            }
        }
    }
}
