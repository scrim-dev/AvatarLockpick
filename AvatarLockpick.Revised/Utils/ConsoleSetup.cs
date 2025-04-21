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
        //This class is so bad omgggg (pls ignore <3)
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

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

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

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
    }
}