using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AvatarLockpick.Revised.Utils
{
    internal class ConsoleSetup
    {
        public static bool HideCons = false;

        private const int MF_BYCOMMAND = 0x00000000;
        private const int SC_MAXIMIZE = 0xF030;

        [DllImport("user32.dll")]
        private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        const int SW_HIDE = 0;
        const int GWL_EXSTYLE = -20;
        const uint WS_EX_TOOLWINDOW = 0x00000080;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOZORDER = 0x0004;
        const uint SWP_FRAMECHANGED = 0x0020;

        public static void Init()
        {
            Console.TreatControlCAsInput = true;
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
            }

            if (!File.Exists($"{Directory.GetCurrentDirectory()}\\GUI\\hideconsole.txt"))
            {
                try { File.WriteAllText($"{Directory.GetCurrentDirectory()}\\GUI\\hideconsole.txt", "false"); } catch { }
            }

            try
            {
                if (File.ReadAllText($"{Directory.GetCurrentDirectory()}\\GUI\\hideconsole.txt") == "true")
                {
                    if (handle != IntPtr.Zero)
                    {
                        // Hide the window
                        ShowWindow(handle, SW_HIDE);

                        // Remove from taskbar by making it a toolwindow
                        SetWindowLong(handle, GWL_EXSTYLE, WS_EX_TOOLWINDOW);

                        // Ensure changes take effect
                        SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0,
                            SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
                    }
                }
            }
            catch { }
        }
    }
}
