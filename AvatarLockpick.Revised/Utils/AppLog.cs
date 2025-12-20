using Pastel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvatarLockpick.Revised.Utils
{
    internal class AppLog
    {
        public static bool ClearLogsOnExit = false;
        public static string? LogFilePath { get; private set; }
        
        // Event to notify subscribers (like the UI) of new log messages
        public static event Action<string, string, string>? OnLogReceived;
        
        // Event to notify subscribers of download progress
        public static event Action<int, string, string>? OnProgressReceived;
        
        // Event to notify when download is complete
        public static event Action? OnDownloadComplete;

        public static void Progress(int percent, string status, string title = "Downloading...")
        {
            OnProgressReceived?.Invoke(percent, status, title);
        }

        public static void DownloadComplete()
        {
            OnDownloadComplete?.Invoke();
        }

        public static void SetupLogFile()
        {
            var LogFile = $"Log_{DateTime.Now:d}_{DateTime.Now:HH.mm.ss}.txt".Replace("/", "_");

            if (!Directory.Exists($"UI\\Logs"))
            {
                try { Directory.CreateDirectory($"UI\\Logs"); } 
                catch(Exception ex)
                {
                    MessageBoxUtils.ShowError(ex.Message);
                }

                try { File.WriteAllText($"UI\\Logs\\{LogFile}", "LOGS:\n"); } catch { }
                LogFilePath = $"UI\\Logs\\{LogFile}";
            }
            else
            {
                try { File.WriteAllText($"UI\\Logs\\{LogFile}", "LOGS:\n"); } catch { }
                LogFilePath = $"UI\\Logs\\{LogFile}";
            }

            try { File.WriteAllText($"UI\\ClearLogs.txt", "false"); } catch { }
        }

        public static void Log(string CurrentTask, string Message)
        {
            // Trigger event for UI
            OnLogReceived?.Invoke("info", CurrentTask, Message);

            WriteToLogFile("i", Message);
        }

        public static void Success(string CurrentTask, string Message)
        {
            // Trigger event for UI
            OnLogReceived?.Invoke("success", CurrentTask, Message);

            WriteToLogFile("+", Message);
        }

        public static void Warn(string CurrentTask, string Message)
        {
            // Trigger event for UI
            OnLogReceived?.Invoke("warn", CurrentTask, Message);

            WriteToLogFile("!", Message);
        }

        public static void Error(string CurrentTask, string Message)
        {
            // Trigger event for UI
            OnLogReceived?.Invoke("error", CurrentTask, Message);

            WriteToLogFile("X", Message);
        }

        private static void WriteToLogFile(string type, string message)
        {
            var logEntry = $"[{DateTime.Now}] [{type}] {message}";
            if (LogFilePath != null) { File.AppendAllText(LogFilePath, logEntry + Environment.NewLine); }
        }
    }
}
