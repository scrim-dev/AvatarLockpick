using Photino.NET;
using System.Drawing;
using System.Text;
using AvatarLockpick.Revised.Utils;

namespace AvatarLockpick.Revised
{
    internal class Program
    {
        public static string AppVersion = "2.1";
        public static HttpUtils HttpC { get; private set; } = new();
        // A unique name for the mutex to ensure only one instance runs.
        private const string AppMutexName = "Global\\AvatarLockpickRevised_Mutex_2A7B9C1D";

        //Application Icon by: Kmg Design
        //GUI made with: https://github.com/tryphotino/photino.NET
        [STAThread]
        static void Main(string[] args)
        {
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

                if (!File.Exists($"GUI\\no_startup_warn.scrim"))
                {
                    MessageBoxUtils.ShowWarning("If you run into any bugs, issues or crashes contact me on discord:\nscrimmane (679060175440707605)" +
                    "\nOr post an 'issue' on github!", "Hey!");
                    try { File.WriteAllText($"GUI\\no_startup_warn.scrim", "cached"); } catch { /*Nothing*/ }
                }

                HttpC.Load();

                // Application logic starts here if this is the first instance
                string windowTitle = "AvatarLockpick";

                // Creating a new PhotinoWindow instance with the fluent API
                var window = new PhotinoWindow()
                    .SetTitle(windowTitle)
                    // Resize to a percentage of the main monitor work area
                    .SetUseOsDefaultSize(false)
                    .SetSize(new Size(900, 600))
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
                    .Load("GUI/index.html"); // Can be used with relative path strings or "new URI()" instance to load a website.
                Thread.Sleep(1200);
                Console.Clear();
                AppLog.Warn("Startup", "Do not close the console as it is needed. If you'd like to hide it open" +
                    " the hideconsole.txt file in the 'GUI' folder and change it from false to true. Then close and reopen the app.");
                AppLog.Success("Startup", "App Loaded!");

                try { AppLog.ClearLogsOnExit = bool.Parse(File.ReadAllText("GUI\\ClearLogs.txt")); } catch { AppLog.ClearLogsOnExit = false; }

                ConsoleSetup.OnExit += () =>
                {
                    if (AppLog.ClearLogsOnExit)
                    {
                        if (Directory.Exists($"GUI\\Logs"))
                        {
                            try { Directory.Delete($"GUI\\Logs", true); } catch { }
                        }
                    }
                };

                window.SetChromeless(false);
                window.SetDevToolsEnabled(false);
                window.SetIconFile($"GUI\\assets\\unlockicon.ico");
                window.WaitForClose(); // Starts the application event loop
            }
        }
    }
}