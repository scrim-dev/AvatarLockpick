using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

namespace AvatarLockpickLite
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnUnlock_Click(object sender, RoutedEventArgs e)
        {
            ALP_Unlocker.UnlockAvatar(TxtUserId.Text, TxtAvatarId.Text);
        }

        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            var process = Process.GetProcessesByName("VRChat").FirstOrDefault();
            if (process != null)
            {
                process.Kill();
                process.WaitForExit(5000);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "steam://rungameid/438100 --no-vr",
                UseShellExecute = true
            });
        }
    }
}