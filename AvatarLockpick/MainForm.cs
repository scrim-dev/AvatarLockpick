using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Microsoft.VisualBasic;
using AvatarLockpick.Utils;
using System.Windows.Forms;
using Windows.AI.MachineLearning;

namespace AvatarLockpick
{
    public partial class MainForm : Form
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            if (IsWindows10OrGreater(17763))
            {
                var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                return DwmSetWindowAttribute(handle, (int)attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }

            return false;
        }

        private static bool IsWindows10OrGreater(int build = -1)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static bool AutoRestartTog { get; set; } = false;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        private bool hasConsole = false;

        private Config _config;
        private bool _isLoading = false;

        public MainForm()
        {
            InitializeComponent();
            UseImmersiveDarkMode(Handle, true);

            // First load config
            _isLoading = true;
            _config = Config.Load();

            // Then apply loaded config
            UserIDTextBox.Text = _config.UserId ?? "usr_XXXXXXXXXXXXXXXXXXXX";
            AvatarIDTextBox.Text = _config.AvatarId ?? "avtr_XXXXXXXXXXXXXXXXXXXX";
            HideUserIDCheckBox.Checked = _config.HideUserId;
            AutoRestartCheckBox.Checked = _config.AutoRestart;
            AutoRestartTog = _config.AutoRestart;

            // Finally wire up event handlers
            UserIDTextBox.TextChanged += UserIDTextBox_TextChanged;
            AvatarIDTextBox.TextChanged += AvatarIDTextBox_TextChanged;
            _isLoading = false;
        }

        private void SaveConfig()
        {
            if (_isLoading) return;  // Don't save while loading initial values

            Console.WriteLine($"Saving config - UserID: {UserIDTextBox.Text}, AvatarID: {AvatarIDTextBox.Text}");
            _config.UserId = UserIDTextBox.Text;
            _config.AvatarId = AvatarIDTextBox.Text;
            _config.HideUserId = HideUserIDCheckBox.Checked;
            _config.AutoRestart = AutoRestartCheckBox.Checked;
            _config.Save();
        }

        private void UserIDTextBox_TextChanged(object? sender, EventArgs e)
        {
            SaveConfig();
        }

        private void AvatarIDTextBox_TextChanged(object? sender, EventArgs e)
        {
            SaveConfig();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MsgBoxUtils.ShowInfo("Welcome to AvatarLockpick! This tool is designed to help you unlock avatars in VRChat. " +
                "Please make sure you have changed into the avatar you want to unlock before using this!", "Welcome");

            VERSIONTEXT.Text = Program.AppVersion;
        }

        private void UnlockBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(UserIDTextBox.Text) || string.IsNullOrEmpty(AvatarIDTextBox.Text))
            {
                MsgBoxUtils.ShowError("Please enter a User ID and Avatar ID!", "Error");
                return;
            }

            bool success = AvatarFinder.UnlockSpecificAvatar(UserIDTextBox.Text, AvatarIDTextBox.Text);
            if (AutoRestartTog)
            {
                VRCManager.CloseVRChat();
                VRCManager.LaunchVRChat(RestartWithVRCheckBox.Checked);
            }

            if (success)
            {
                MsgBoxUtils.ShowInfo("Avatar unlocked successfully!", "Success");
            }
            else
            {
                MsgBoxUtils.ShowError("Failed to unlock avatar!", "Error");
            }
        }

        private void ResetAvatarBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(UserIDTextBox.Text) || string.IsNullOrEmpty(AvatarIDTextBox.Text))
            {
                MsgBoxUtils.ShowError("Please enter a User ID and Avatar ID!", "Error");
                return;
            }

            bool success = AvatarFinder.DeleteSpecificAvatar(UserIDTextBox.Text, AvatarIDTextBox.Text);
            if (success)
            {
                MsgBoxUtils.ShowInfo("Avatar reset successfully!", "Success");
            }
            else
            {
                MsgBoxUtils.ShowError("Failed to reset avatar!", "Error");
            }
        }

        private void RestartBtn_Click(object sender, EventArgs e)
        {
            VRCManager.CloseVRChat();
            VRCManager.LaunchVRChat(RestartWithVRCheckBox.Checked);
        }

        private void AutoRestartCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            AutoRestartTog = AutoRestartCheckBox.Checked;
            SaveConfig();
        }

        private void HideUserIDCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UserIDTextBox.UseSystemPasswordChar = HideUserIDCheckBox.Checked;
            SaveConfig();
        }

        private void UnlockAllBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(UserIDTextBox.Text))
            {
                MsgBoxUtils.ShowError("Please enter a User ID", "Error");
                return;
            }

            bool success = AvatarFinder.UnlockAvatars(UserIDTextBox.Text);
            if (success)
            {
                MsgBoxUtils.ShowInfo("All avatars unlocked successfully!", "Success");
            }
            else
            {
                MsgBoxUtils.ShowError("Failed to unlock all avatars!", "Error");
            }
        }

        private void HideBtn_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void OpenAviFileBtn_Click(object sender, EventArgs e)
        {
            AvatarFinder.OpenAvatarInNotepad(UserIDTextBox.Text, AvatarIDTextBox.Text);
        }

        private void AppendConsoleBtn_Click(object sender, EventArgs e)
        {
            if (!hasConsole)
            {
                AllocConsole();
                Console.Title = "AvatarLockpick Debug Console";
                Console.WriteLine("Console initialized. Debug output will appear here.");
                hasConsole = true;
            }
            else
            {
                FreeConsole();
                hasConsole = false;
            }
        }

        private void ClearCfgBtn_Click(object sender, EventArgs e)
        {
            _config.Clear();
        }

        private void DeleteCfgBtn_Click(object sender, EventArgs e)
        {
            Config.ClearConfig();
        }

        private void UnlockVRCFBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(UserIDTextBox.Text) || string.IsNullOrEmpty(AvatarIDTextBox.Text))
            {
                MsgBoxUtils.ShowError("Please enter a User ID and Avatar ID!", "Error");
                return;
            }

            bool success = AvatarFinder.UnlockVRCFAvatar(UserIDTextBox.Text, AvatarIDTextBox.Text);
            if (AutoRestartTog)
            {
                VRCManager.CloseVRChat();
                VRCManager.LaunchVRChat(RestartWithVRCheckBox.Checked);
            }

            if (success)
            {
                MsgBoxUtils.ShowInfo("Avatar unlocked successfully!", "Success");
            }
            else
            {
                MsgBoxUtils.ShowError("Failed to unlock avatar!", "Error");
            }
        }
    }
}