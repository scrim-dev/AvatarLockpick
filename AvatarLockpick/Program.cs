using AvatarLockpick.Utils;
using Photino.NET;
using Sentry;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

namespace AvatarLockpick
{
    internal class Program
    {
        public const string AppVersion = "2026.08.09-15"; //Global Version Set
        public static HttpUtils HttpC { get; private set; } = new();
        public static Size AppSize { get; private set; } = new Size(1300, 800);
        public static bool IsDevMode { get; private set; }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(nint hwnd, int attr, ref int attrValue, int attrSize);

        // Fixed mutex name to avoid conflicts with older instances
        private const string AppMutexName = $"AvatarLockpickRevised_{AppVersion}";

        //Application Icon by: Kmg Design
        //GUI made with: https://github.com/tryphotino/photino.NET
        [STAThread]
        static void Main(string[] args)
        {
            EnsureAppWorkingDirectory();
            EnsureStartMenuShortcut();

            IsDevMode = Array.Exists(args, a => a.Equals("--devmode", StringComparison.OrdinalIgnoreCase));
            Console.WriteLine(AppMutexName + " app start/entry");
            if (IsDevMode) Console.WriteLine("[DevMode] Developer mode is active.");

            SentrySdk.Init(o =>
            {
                o.Dsn = "https://27dad4e1312a912b575cbb06ec9f04f9@o4511474449055744.ingest.us.sentry.io/4511474473566208";
                o.Release = $"avatarlockpick@{AppVersion}";
                o.Debug = false;
                o.TracesSampleRate = 0.0;
                o.AutoSessionTracking = true;
                o.IsGlobalModeEnabled = true;
            });

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

                if (!File.Exists($"UI/no_startup_warn.scrim"))
                {
                    MessageBoxUtils.ShowWarning("If you run into any bugs, issues or crashes contact me on discord or post an 'issue' on github!", "Hey!");
                    try { File.WriteAllText($"UI/no_startup_warn.scrim", "cached_startup"); } catch { }
                }

                HttpC.Load();
                PAWUtils.Init();

                // Application logic starts here if this is the first instance
                string windowTitle = "AvatarLockpick";

                // Creating a new PhotinoWindow instance with the fluent API
                var window = new PhotinoWindow()
                    .SetTitle(windowTitle)
                    .SetChromeless(true)
                    .SetUseOsDefaultSize(false)
                    .SetSize(AppSize)
                    .SetUseOsDefaultLocation(false)
                    .Center()
                    .SetResizable(true)
                    .RegisterWindowCreatedHandler((sender, _) =>
                    {
                        if (sender is PhotinoWindow w && OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
                        {
                            int pref = 2; // DWMWCP_ROUND
                            DwmSetWindowAttribute(w.WindowHandle, 33, ref pref, sizeof(int));
                        }
                    })
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
                        GUIcom.Communication(message, window);
                    })
                    .Load($"UI/index.html"); // Can be used with relative path strings or "new URI()" instance to load a website.

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

                // Wire up raw JSON send to UI
                AppLog.OnRawSend += (json) =>
                {
                    try { window.SendWebMessage(json); } catch { }
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

                try { AppLog.ClearLogsOnExit = bool.Parse(File.ReadAllText($"UI/ClearLogs.txt")); } catch { AppLog.ClearLogsOnExit = false; }

                if(!File.Exists($"UI/ALP_History.json"))
                {
                    try { File.WriteAllText($"UI/ALP_History.json", "{}"); } catch { }
                }

                window.SetDevToolsEnabled(false);
                window.SetIconFile($"UI/unlockicon.ico");

                window.WaitForClose(); // Starts the application event loop

                // Cleanup on exit
                if (AppLog.ClearLogsOnExit)
                {
                    if (Directory.Exists($"UI/Logs"))
                    {
                        try { Directory.Delete($"UI/Logs", true); } catch { }
                    }
                }
            }
        }

        private static void EnsureAppWorkingDirectory()
        {
            try
            {
                string? executablePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
                string? appDirectory = Path.GetDirectoryName(executablePath);
                if (!string.IsNullOrWhiteSpace(appDirectory) && Directory.Exists(appDirectory))
                {
                    Directory.SetCurrentDirectory(appDirectory);
                }
            }
            catch { }
        }

        private static void EnsureStartMenuShortcut()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            try
            {
                string? executablePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
                string? appDirectory = Path.GetDirectoryName(executablePath);
                if (string.IsNullOrWhiteSpace(executablePath) || string.IsNullOrWhiteSpace(appDirectory))
                {
                    return;
                }

                string programsPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                if (string.IsNullOrWhiteSpace(programsPath))
                {
                    return;
                }

                string shortcutPath = Path.Combine(programsPath, "AvatarLockpick.lnk");
                string iconPath = Path.Combine(appDirectory, "UI", "unlockicon.ico");
                CreateShortcut(shortcutPath, executablePath, appDirectory, File.Exists(iconPath) ? iconPath : executablePath);
            }
            catch { }
        }

        private static void CreateShortcut(string shortcutPath, string targetPath, string workingDirectory, string iconPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(shortcutPath)!);

            string script = string.Join(Environment.NewLine,
                "$shell = New-Object -ComObject WScript.Shell",
                "$shortcut = $shell.CreateShortcut($args[0])",
                "$shortcut.TargetPath = $args[1]",
                "$shortcut.WorkingDirectory = $args[2]",
                "$shortcut.IconLocation = $args[3]",
                "$shortcut.Save()");

            using Process process = Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                ArgumentList =
                {
                    "-NoProfile",
                    "-ExecutionPolicy",
                    "Bypass",
                    "-Command",
                    script,
                    shortcutPath,
                    targetPath,
                    workingDirectory,
                    iconPath + ",0"
                },
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            })!;

            process.WaitForExit(5000);
        }
    }
}
