using AvatarLockpick.Revised.Utils;
using Photino.NET;
using System.Drawing;
using System.Reflection;
using System.Text;

namespace AvatarLockpick.Revised
{
    internal class Program
    {
        public const string AppVersion = "2.3";
        public static HttpUtils HttpC { get; private set; } = new();
        public static Size AppSize { get; private set; } = new Size(1300, 800);

        // Fixed mutex name to avoid conflicts with older instances
        private const string AppMutexName = $"AvatarLockpickRevised_{AppVersion}";

        //Application Icon by: Kmg Design
        //GUI made with: https://github.com/tryphotino/photino.NET
        [STAThread]
        static void Main(string[] args)
        {
            AppLog.SetupLogFile();

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
                    try { File.WriteAllText($"UI\\no_startup_warn.scrim", "cached_startup"); } catch { }
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

                // Wire up Logging to UI
                AppLog.OnLogReceived += (type, task, msg) =>
                {
                    try
                    {
                        var logData = new
                        {
                            type = "log",
                            logType = type,
                            task = task,
                            message = msg,
                            timestamp = DateTime.Now.ToString("HH:mm:ss")
                        };
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(logData);
                        window.SendWebMessage(json);
                    }
                    catch { }
                };

                // Wire up Progress to UI
                AppLog.OnProgressReceived += (percent, status, title) =>
                {
                    try
                    {
                        var progressData = new
                        {
                            type = "downloadProgress",
                            progress = percent,
                            status = status,
                            title = title
                        };
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(progressData);
                        window.SendWebMessage(json);
                    }
                    catch { }
                };

                // Wire up Download Complete to UI
                AppLog.OnDownloadComplete += () =>
                {
                    try
                    {
                        var completeData = new { type = "downloadComplete" };
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(completeData);
                        window.SendWebMessage(json);
                    }
                    catch { }
                };

                AppLog.Success("Startup", "App Loaded!");

                try { AppLog.ClearLogsOnExit = bool.Parse(File.ReadAllText($"UI\\ClearLogs.txt")); } catch { AppLog.ClearLogsOnExit = false; }

                if(!File.Exists($"UI\\ALP_History.json"))
                {
                    try { File.WriteAllText($"UI\\ALP_History.json", "{}"); } catch { }
                }

                VersionChecker.CheckForUpdates();

                window.SetChromeless(false);
                window.SetDevToolsEnabled(false);
                window.SetIconFile($"UI\\unlockicon.ico");
                window.WaitForClose(); // Starts the application event loop

                // Cleanup on exit
                if (AppLog.ClearLogsOnExit)
                {
                    if (Directory.Exists($"UI\\Logs"))
                    {
                        try { Directory.Delete($"UI\\Logs", true); } catch { }
                    }
                }
            }
        }
    }
}