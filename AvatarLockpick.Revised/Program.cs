﻿using AvatarLockpick.Revised.Utils;
using Photino.NET;
using System.Drawing;
using System.Reflection;
using System.Text;

namespace AvatarLockpick.Revised
{
    internal class Program
    {
        public const string AppVersion = "2.2";
        public static HttpUtils HttpC { get; private set; } = new();
        public static Size AppSize { get; private set; } = new Size(1300, 800);

        // Fixed mutex name to avoid conflicts with older instances
        private const string AppMutexName = $"AvatarLockpickRevised_{AppVersion}";

        //Application Icon by: Kmg Design
        //GUI made with: https://github.com/tryphotino/photino.NET
        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "Loading app...";
            VersionChecker.CheckForUpdates();

            AppLog.SetupLogFile();

            ConsoleSetup.Init();
            Console.Title = "AvatarLockpick App";
            AppLog.Warn("Startup", "Loading Application...");

            // Try to grab the mutex
            using (Mutex mutex = new Mutex(true, AppMutexName, out bool createdNew))
            {
                if (!createdNew)
                {
                    // Another instance is already running
                    MessageBoxUtils.ShowWarning("Another instance of AvatarLockpick is already running.", "Application Already Running");
                    return; // Exit the application
                }

                if (!File.Exists($"UI\\no_startup_warn.scrim"))
                {
                    MessageBoxUtils.ShowWarning("If you run into any bugs, issues or crashes contact me on discord:\nscrimmane (679060175440707605)" +
                    "\nOr post an 'issue' on github!", "Hey!");
                    try { File.WriteAllText($"UI\\no_startup_warn.scrim", "cached"); } catch { }
                }

                HttpC.Load();

                // Application logic starts here if this is the first instance
                string windowTitle = "AvatarLockpick";

                // Creating a new PhotinoWindow instance with the fluent API
                var window = new PhotinoWindow()
                    .SetTitle(windowTitle)
                    // Resize to a percentage of the main monitor work area
                    .SetUseOsDefaultSize(false)
                    .SetSize(AppSize)
                    // Center window in the middle of the screen
                    .Center()
                    // Users can resize windows by default.
                    .SetResizable(true)
                    .RegisterCustomSchemeHandler("app", (object sender, string scheme, string url, out string contentType) =>
                    {
                        contentType = "text/javascript";
                        return new MemoryStream(Encoding.UTF8.GetBytes(@""));
                    })
                    // Most event handlers can be registered after the
                    // PhotinoWindow was instantiated by calling a registration 
                    // method like the following RegisterWebMessageReceivedHandler.
                    // This could be added in the PhotinoWindowOptions if preferred.
                    .RegisterWebMessageReceivedHandler((sender, message) =>
                    {
                        var window = sender as PhotinoWindow;

                        // The message argument is coming in from sendMessage.
                        // "window.external.sendMessage(message: string)"
                        string response = $"Received message: \"{message}\"";

                        // Send a message back the to JavaScript event handler.
                        // "window.external.receiveMessage(callback: Function)"
                        window.SendWebMessage(response);

                        //Send data to be processed
                        GUIcom.Communication(message);
                    })
                    .Load($"UI\\index.html"); // Can be used with relative path strings or "new URI()" instance to load a website.

                Thread.Sleep(1200);
                Console.Clear();
                AppLog.Warn("Startup", "Do not close the console as it is needed. If you'd like to hide it open " +
                    $"the hideconsole.txt file in the 'UI' folder and change it from false to true. Then close and reopen the app.");
                AppLog.Success("Startup", "App Loaded!");

                try { AppLog.ClearLogsOnExit = bool.Parse(File.ReadAllText($"UI\\ClearLogs.txt")); } catch { AppLog.ClearLogsOnExit = false; }

                ConsoleSetup.OnExit += () =>
                {
                    if (AppLog.ClearLogsOnExit)
                    {
                        if (Directory.Exists($"UI\\Logs"))
                        {
                            try { Directory.Delete($"UI\\Logs", true); } catch { }
                        }
                    }
                };

                if(!File.Exists($"UI\\ALP_History.json"))
                {
                    try { File.WriteAllText($"UI\\ALP_History.json", "{}"); } catch { }
                }

                window.SetChromeless(false);
                window.SetDevToolsEnabled(false);
                window.SetIconFile($"UI\\unlockicon.ico");
                window.WaitForClose(); // Starts the application event loop
            }
        }
    }
}