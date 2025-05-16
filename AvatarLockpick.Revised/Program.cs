using AvatarLockpick.Revised.Utils;
using Photino.NET;
using System.Drawing;
using System.Reflection;
using System.Text;

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
            Console.Title = "Loading app...";
            VersionChecker.CheckForUpdates();

            AppFolders.Load();

            AppLog.SetupLogFile();

            ConsoleSetup.Init();
            Console.Title = "AvatarLockpick App";
            AppLog.Warn("Startup", "Loading Application...");

            ExtractResources($"{AppFolders.DataLowFolder}\\App");

            // Try to grab the mutex
            using (Mutex mutex = new Mutex(true, AppMutexName, out bool createdNew))
            {
                if (!createdNew)
                {
                    // Another instance is already running
                    MessageBoxUtils.ShowWarning("Another instance of AvatarLockpick is already running.", "Application Already Running");
                    return; // Exit the application
                }

                if (!File.Exists($"{AppFolders.DataLowFolder}\\no_startup_warn.scrim"))
                {
                    MessageBoxUtils.ShowWarning("If you run into any bugs, issues or crashes contact me on discord:\nscrimmane (679060175440707605)" +
                    "\nOr post an 'issue' on github!", "Hey!");
                    try { File.WriteAllText($"{AppFolders.DataLowFolder}\\no_startup_warn.scrim", "cached"); } catch { /*Nothing*/ }
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
                    .Load($"{AppFolders.DataLowFolder}\\App\\index.html"); // Can be used with relative path strings or "new URI()" instance to load a website.

                Thread.Sleep(1200);
                Console.Clear();
                AppLog.Warn("Startup", "Do not close the console as it is needed. If you'd like to hide it open " +
                    $"the hideconsole.txt file in the '{AppFolders.DataLowFolder}' folder and change it from false to true. Then close and reopen the app.");
                AppLog.Success("Startup", "App Loaded!");

                try { AppLog.ClearLogsOnExit = bool.Parse(File.ReadAllText($"{AppFolders.DataLowFolder}\\ClearLogs.txt")); } catch { AppLog.ClearLogsOnExit = false; }

                ConsoleSetup.OnExit += () =>
                {
                    if (AppLog.ClearLogsOnExit)
                    {
                        if (Directory.Exists($"{AppFolders.DataLowFolder}\\Logs"))
                        {
                            try { Directory.Delete($"{AppFolders.DataLowFolder}\\Logs", true); } catch { }
                        }
                    }
                };

                window.SetChromeless(false);
                window.SetDevToolsEnabled(false);
                window.SetIconFile($"{AppFolders.DataLowFolder}\\App\\unlockicon.ico");
                window.WaitForClose(); // Starts the application event loop
            }
        }

        private static void ExtractResources(string outputDirectory)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePrefix = $"{assembly.GetName().Name}.HTML.";

            // Map resource names to output paths
            var resources = new Dictionary<string, string>
        {
            { $"{resourcePrefix}index.html", Path.Combine(outputDirectory, "index.html") },
            { $"{resourcePrefix}AppStyle.css", Path.Combine(outputDirectory, "AppStyle.css") },
            { $"{resourcePrefix}ButtonCalls.js", Path.Combine(outputDirectory, "ButtonCalls.js") },
            { $"{resourcePrefix}unlockicon.ico", Path.Combine(outputDirectory, "unlockicon.ico") }
        };

            foreach (var resource in resources)
            {
                try
                {
                    // Create directory if needed
                    Directory.CreateDirectory(Path.GetDirectoryName(resource.Value));

                    using (Stream stream = assembly.GetManifestResourceStream(resource.Key))
                    using (FileStream fileStream = File.Create(resource.Value))
                    {
                        if (stream == null)
                            throw new FileNotFoundException($"Embedded resource not found: {resource.Key}");

                        stream.CopyTo(fileStream);
                    }
                    Console.WriteLine($"Extracted: {resource.Value}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to extract {resource.Key}: {ex.Message}");
                }
            }
        }
    }
}