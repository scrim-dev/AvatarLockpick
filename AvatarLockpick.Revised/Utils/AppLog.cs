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
        }
    }
}
