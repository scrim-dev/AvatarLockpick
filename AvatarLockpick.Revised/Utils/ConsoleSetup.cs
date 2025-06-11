using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AvatarLockpick.Revised.Utils
{
    internal class ConsoleSetup
    {
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

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

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate handler, bool add);

        private delegate bool ConsoleEventDelegate(CtrlType sig);

        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        public static event Action OnExit;

        const int SW_HIDE = 0;
        const int GWL_EXSTYLE = -20;
        const int WS_EX_TOOLWINDOW = 0x00000080;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOZORDER = 0x0004;
        const uint SWP_FRAMECHANGED = 0x0020;
        private const int MF_BYCOMMAND = 0x00000000;
        private const int SC_MAXIMIZE = 0xF030;
        const int WS_EX_APPWINDOW = 0x00040000;
        const int HWND_BOTTOM = 1;

        public static void Init()
        {
            SetConsoleCtrlHandler(Handler, true);
            Console.TreatControlCAsInput = true;
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
            }

            if (!File.Exists($"UI\\hideconsole.txt"))
            {
                try { File.WriteAllText($"UI\\hideconsole.txt", "false"); } catch { }
            }

            try
            {
                if (File.ReadAllText($"UI\\hideconsole.txt") == "true")
                {
                    if (handle != IntPtr.Zero)
                    {
                        // Hide lol
                        ShowWindow(handle, SW_HIDE);

                        SetWindowLong(handle, GWL_EXSTYLE, WS_EX_TOOLWINDOW);

                        SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0,
                            SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
                    }

                    //For newer windows terminal
                    IntPtr hWnd = GetConsoleWindow();
                    if (hWnd != IntPtr.Zero)
                    {
                        ShowWindow(hWnd, SW_HIDE);

                        int style = GetWindowLong(hWnd, GWL_EXSTYLE);
                        SetWindowLong(hWnd, GWL_EXSTYLE,
                            (style | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);

                        SetWindowPos(hWnd, (IntPtr)HWND_BOTTOM,
                            0, 0, 0, 0,
                            SWP_NOSIZE | SWP_NOMOVE | SWP_NOZORDER | SWP_FRAMECHANGED);

                        IntPtr terminalHandle = FindWindow("CASCADIA_HOSTING_WINDOW_CLASS", null);
                        if (terminalHandle != IntPtr.Zero)
                        {
                            int termStyle = GetWindowLong(terminalHandle, GWL_EXSTYLE);
                            SetWindowLong(terminalHandle, GWL_EXSTYLE,
                                (termStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
                            ShowWindow(terminalHandle, SW_HIDE);
                        }
                    }
                }
            }
            catch { }
        }

        private static bool Handler(CtrlType sig)
        {
            AppLog.Warn("Exit", "App closing...");
            OnExit?.Invoke();
            return false;
        }
    }
}