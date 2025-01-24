using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AvatarLockpick.Utils
{
    internal class VRCManager
    {
        private const string VRCHAT_PROCESS_NAME = "VRChat";
        private const string STEAM_PROTOCOL = "steam://rungameid/438100";

        /// <summary>
        /// Gets the VRChat process if it's running, otherwise returns null
        /// </summary>
        public static Process? GetVRChatProcess()
        {
            return Process.GetProcessesByName(VRCHAT_PROCESS_NAME).FirstOrDefault();
        }

        /// <summary>
        /// Checks if VRChat is currently running
        /// </summary>
        public static bool IsVRChatRunning()
        {
            return GetVRChatProcess() != null;
        }

        /// <summary>
        /// Checks if VRChat process is responding
        /// </summary>
        public static bool IsVRChatResponding()
        {
            var process = GetVRChatProcess();
            return process?.Responding ?? false;
        }

        /// <summary>
        /// Launches VRChat through Steam
        /// </summary>
        public static void LaunchVRChat()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = STEAM_PROTOCOL,
                UseShellExecute = true
            });
        }

        /// <summary>
        /// Gets the current status of VRChat process
        /// </summary>
        public static string GetVRChatStatus()
        {
            var process = GetVRChatProcess();
            if (process == null)
                return "Not Running";
            
            return process.Responding ? "Running" : "Not Responding";
        }

        /// <summary>
        /// Attempts to close VRChat gracefully
        /// </summary>
        /// <returns>True if the process was closed successfully, false if it failed or wasn't running</returns>
        public static bool CloseVRChat()
        {
            var process = GetVRChatProcess();
            if (process != null)
            {
                process.CloseMainWindow();
                return process.WaitForExit(5000); // Wait up to 5 seconds for graceful exit
            }
            return false;
        }

        /// <summary>
        /// Forcefully kills the VRChat process
        /// </summary>
        /// <returns>True if the process was killed successfully, false if it wasn't running</returns>
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
