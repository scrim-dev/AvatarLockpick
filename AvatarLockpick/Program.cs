using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AvatarLockpick
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public const string mutexName = "AvatarLockpickMutex";

        public static string AppVersion { get; set; } = "1.1";

        [STAThread]
        static void Main()
        {
            using var mutex = new Mutex(true, mutexName, out bool createdNew);

            if (!createdNew)
            {
                MessageBox.Show("AvatarLockpick is already running!", "AvatarLockpick", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                var existing = Process.GetCurrentProcess();
                foreach (var process in Process.GetProcessesByName(existing.ProcessName))
                {
                    if (process.Id != existing.Id)
                    {
                        SetForegroundWindow(process.MainWindowHandle);
                        break;
                    }
                }

                return;
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}