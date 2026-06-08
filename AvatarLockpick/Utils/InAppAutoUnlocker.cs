using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AvatarLockpick.Utils
{
    /// <summary>
    /// Manages the in-app background auto-unlock feature and the external service process.
    /// </summary>
    internal static class InAppAutoUnlocker
    {
        // ─── In-App runner ──────────────────────────────────────────────────────
        private static CancellationTokenSource? _cts;
        private static Task? _runningTask;
        private static bool _enabled = false;
        private static string _mode = "inapp";
        private static int _intervalSeconds = 30;

        public static bool IsRunning => _runningTask != null && !_runningTask.IsCompleted;

        // ─── Service process ────────────────────────────────────────────────────
        private static Process? _serviceProcess;

        public static bool IsServiceProcessRunning
        {
            get
            {
                try { return _serviceProcess != null && !_serviceProcess.HasExited; }
                catch { return false; }
            }
        }

        private static string ServiceExePath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AvatarLockpick.Service.exe");

        private static string DataFolder
        {
            get
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ALP_data");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        private static string ServiceLogPath => Path.Combine(DataFolder, "ALP_ServiceLog.txt");

        /// <summary>
        /// Applies new settings from the UI. Starts or stops the appropriate runner.
        /// </summary>
        public static void ApplySettings(bool enabled, string mode, int intervalSeconds)
        {
            _enabled = enabled;
            _mode = mode;
            _intervalSeconds = Math.Max(5, intervalSeconds);

            // Always stop the in-app runner first
            Stop();

            if (!enabled)
            {
                // Also stop the service process if switching off
                StopServiceProcess();
                return;
            }

            if (_mode == "inapp")
                Start();
            // Service mode is started/stopped manually via buttons — don't auto-start here
        }

        public static void Start()
        {
            if (IsRunning) return;

            _cts = new CancellationTokenSource();
            _runningTask = Task.Run(() => RunLoop(_cts.Token), _cts.Token);
            AppLog.Log("AutoUnlock", $"In-app auto-unlock started (interval: {_intervalSeconds}s)");
        }

        public static void Stop()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                try { _runningTask?.Wait(2000); } catch { }
                _cts.Dispose();
                _cts = null;
                _runningTask = null;
                AppLog.Log("AutoUnlock", "In-app auto-unlock stopped");
            }
        }

        // ─── Service process management ─────────────────────────────────────────

        public static void StartServiceProcess()
        {
            if (IsServiceProcessRunning)
            {
                AppLog.Warn("AutoUnlock", "Service is already running");
                SendStatus(true);
                return;
            }

            if (!File.Exists(ServiceExePath))
            {
                AppLog.Error("AutoUnlock", $"Service executable not found: {ServiceExePath}");
                SendStatus(false);
                return;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = ServiceExePath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                _serviceProcess = new Process { StartInfo = psi };

                _serviceProcess.OutputDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        AppendServiceLog(e.Data);
                        AppLog.Log("Service", e.Data);
                    }
                };
                _serviceProcess.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        AppendServiceLog("[ERR] " + e.Data);
                        AppLog.Warn("Service", e.Data);
                    }
                };
                _serviceProcess.Exited += (_, _) =>
                {
                    AppLog.Warn("AutoUnlock", "Service process exited");
                    SendStatus(false);
                };
                _serviceProcess.EnableRaisingEvents = true;

                _serviceProcess.Start();
                _serviceProcess.BeginOutputReadLine();
                _serviceProcess.BeginErrorReadLine();

                AppLog.Success("AutoUnlock", $"Service started (PID {_serviceProcess.Id})");
                SendStatus(true);
            }
            catch (Exception ex)
            {
                AppLog.Error("AutoUnlock", $"Failed to start service: {ex.Message}");
                SendStatus(false);
            }
        }

        public static void StopServiceProcess()
        {
            if (!IsServiceProcessRunning)
            {
                AppLog.Log("AutoUnlock", "Service is not running");
                SendStatus(false);
                return;
            }

            try
            {
                _serviceProcess!.Kill(entireProcessTree: true);
                _serviceProcess.WaitForExit(3000);
                AppLog.Log("AutoUnlock", "Service stopped");
            }
            catch (Exception ex)
            {
                AppLog.Warn("AutoUnlock", $"Stop service error: {ex.Message}");
            }
            finally
            {
                _serviceProcess = null;
                SendStatus(false);
            }
        }

        public static void OpenServiceLog()
        {
            if (!File.Exists(ServiceLogPath))
            {
                AppLog.Warn("AutoUnlock", "No service log file found yet");
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = ServiceLogPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                AppLog.Error("AutoUnlock", $"Could not open service log: {ex.Message}");
            }
        }

        private static void SendStatus(bool running)
        {
            string json = JsonConvert.SerializeObject(new
            {
                type = "serviceStatus",
                running
            });
            AppLog.SendToUI(json);
        }

        private static void AppendServiceLog(string line)
        {
            try
            {
                File.AppendAllText(ServiceLogPath,
                    $"[{DateTime.Now:HH:mm:ss}] {line}{Environment.NewLine}");
            }
            catch { }
        }

        private static async Task RunLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    ScanAndUnlock();
                }
                catch (Exception ex)
                {
                    AppLog.Error("AutoUnlock", $"Scan error: {ex.Message}");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        // Tracks the last-write time we processed for each file path so we can skip unchanged files
        private static readonly Dictionary<string, DateTime> _fileLastSeen = new();

        private static void ScanAndUnlock()
        {
            string baseFolder = AvatarUnlocker.GetVRChatAvatarBaseFolder();

            if (!Directory.Exists(baseFolder))
            {
                AppLog.Warn("AutoUnlock", $"LocalAvatarData folder not found: {baseFolder}");
                return;
            }

            if (!AvatarUnlocker.EnsureLockTypesLoaded())
            {
                AppLog.Warn("AutoUnlock", "Lock types not loaded — skipping scan");
                return;
            }

            int totalScanned = 0;
            int totalSkipped = 0;
            int totalUnlocked = 0;

            foreach (string userDir in Directory.GetDirectories(baseFolder))
            {
                foreach (string file in Directory.GetFiles(userDir, "*", SearchOption.TopDirectoryOnly))
                {
                    DateTime lastWrite;
                    try { lastWrite = File.GetLastWriteTimeUtc(file); }
                    catch { continue; }

                    // Skip file if it hasn't changed since we last processed it
                    if (_fileLastSeen.TryGetValue(file, out DateTime prevSeen) && lastWrite <= prevSeen)
                    {
                        totalSkipped++;
                        continue;
                    }

                    totalScanned++;
                    try
                    {
                        if (AvatarUnlocker.PerformDbUnlockOnFile(file))
                        {
                            totalUnlocked++;
                            AppLog.Success("AutoUnlock", $"Unlocked: {Path.GetFileName(file)}");
                        }
                        // Record the write time so we skip this file next cycle unless it changes again
                        _fileLastSeen[file] = lastWrite;
                    }
                    catch (Exception ex)
                    {
                        AppLog.Warn("AutoUnlock", $"Could not process {Path.GetFileName(file)}: {ex.Message}");
                    }
                }
            }

            if (totalScanned > 0 || totalUnlocked > 0)
                AppLog.Success("AutoUnlock", $"Scan complete — {totalUnlocked}/{totalScanned} files unlocked, {totalSkipped} unchanged skipped");
            else if (totalSkipped > 0)
                AppLog.Log("AutoUnlock", $"Scan complete — {totalSkipped} files unchanged, nothing to do");
        }
    }
}
