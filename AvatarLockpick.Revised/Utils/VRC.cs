using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvatarLockpick.Revised.Utils
{
    internal class VRC
    {
        private const string VRCHAT_PROCESS_NAME = "VRChat";
        private const string STEAM_PROTOCOL = "steam://rungameid/438100";

        public static Process? GetVRChatProcess()
        {
            return Process.GetProcessesByName(VRCHAT_PROCESS_NAME).FirstOrDefault();
        }

        public static bool IsVRChatRunning()
        {
            return GetVRChatProcess() != null;
        }

        public static bool IsVRChatResponding()
        {
            var process = GetVRChatProcess();
            return process?.Responding ?? false;
        }

        public static void LaunchVRChat(bool vr)
        {
            if (vr)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = STEAM_PROTOCOL,
                    UseShellExecute = true
                });
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = STEAM_PROTOCOL + " --no-vr",
                    UseShellExecute = true
                });
            }
        }

        public static string GetVRChatStatus()
        {
            var process = GetVRChatProcess();
            if (process == null)
                return "Not Running";

            return process.Responding ? "Running" : "Not Responding";
        }

        public static bool CloseVRChat()
        {
            var process = GetVRChatProcess();
            process?.Kill();
            return false;
        }

        public static bool KillVRChat()
        {
            var process = GetVRChatProcess();
            if (process != null)
            {
                process.Kill();
                process.WaitForExit(5000);
                return true;
            }
            return false;
        }
    }
}
