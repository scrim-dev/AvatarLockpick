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
        public static bool ClearLogsOnExit = true;
        public static string? LogFilePath { get; private set; }
        public static void SetupLogFile()
        {
            var LogFile = $"Log_{DateTime.Now:d}_{DateTime.Now:HH.mm.ss}.txt".Replace("/", "_");

            if (!Directory.Exists($"GUI\\Logs"))
            {
                try { Directory.CreateDirectory($"GUI\\Logs"); } 
                catch(Exception ex)
                {
                    MessageBoxUtils.ShowError(ex.Message);
                }

                try { File.WriteAllText($"GUI\\Logs\\{LogFile}", "LOGS:\n"); } catch { }
                LogFilePath = $"GUI\\Logs\\{LogFile}";
            }
            else
            {
                try { File.WriteAllText($"GUI\\Logs\\{LogFile}", "LOGS:\n"); } catch { }
                LogFilePath = $"GUI\\Logs\\{LogFile}";
            }
        }

        public static void Log(string CurrentTask, string Message)
        {
            //Time
            Console.Write("[".Pastel("#e8e8e8"));
            Console.Write(DateTime.Now.ToString("hh:mm:ss").Pastel("#00ff5e"));
            Console.Write("] ".Pastel("#e8e8e8"));

            //Message
            Console.Write("[".Pastel("#e8e8e8"));
            Console.Write(CurrentTask.Pastel("#0084ff"));
            Console.Write("] ".Pastel("#e8e8e8"));
            Console.Write($"{Message}{Environment.NewLine}".Pastel(ConsoleColor.White));

            WriteToLogFile("i", Message);
        }

        public static void Success(string CurrentTask, string Message)
        {
            //Time
            Console.Write("[".Pastel("#e8e8e8"));
            Console.Write(DateTime.Now.ToString("hh:mm:ss").Pastel("#00ff5e"));
            Console.Write("] ".Pastel("#e8e8e8"));

            //Message
            Console.Write("[".Pastel("#e8e8e8"));
            Console.Write(CurrentTask.Pastel("#0084ff"));
            Console.Write("] ".Pastel("#e8e8e8"));
            Console.Write($"[SUCCESS] {Message}{Environment.NewLine}".Pastel("#00ff40"));

            WriteToLogFile("+", Message);
        }

        public static void Warn(string CurrentTask, string Message)
        {
            //Time
            Console.Write("[".Pastel("#e8e8e8"));
            Console.Write(DateTime.Now.ToString("hh:mm:ss").Pastel("#00ff5e"));
            Console.Write("] ".Pastel("#e8e8e8"));

            //Message
            Console.Write("[".Pastel("#e8e8e8"));
            Console.Write(CurrentTask.Pastel("#0084ff"));
            Console.Write("] ".Pastel("#e8e8e8"));
            Console.Write($"[WARN] {Message}{Environment.NewLine}".Pastel("#ffcc00"));

            WriteToLogFile("!", Message);
        }

        public static void Error(string CurrentTask, string Message)
        {
            //Time
            Console.Write("[".Pastel("#e8e8e8"));
            Console.Write(DateTime.Now.ToString("hh:mm:ss").Pastel("#00ff5e"));
            Console.Write("] ".Pastel("#e8e8e8"));

            //Message
            Console.Write("[".Pastel("#e8e8e8"));
            Console.Write(CurrentTask.Pastel("#0084ff"));
            Console.Write("] ".Pastel("#e8e8e8"));
            Console.Write($"[ERROR] {Message}{Environment.NewLine}".Pastel("#ff002b"));

            WriteToLogFile("X", Message);
        }

        private static void WriteToLogFile(string type, string message)
        {
            var logEntry = $"[{DateTime.Now}] [{type}] {message}";
            if (LogFilePath != null) { File.AppendAllText(LogFilePath, logEntry + Environment.NewLine); }
        }
    }
}
