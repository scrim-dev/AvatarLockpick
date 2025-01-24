namespace AvatarLockpick
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lostBorderPanel1 = new ReaLTaiizor.Controls.LostBorderPanel();
            OpenAviFileBtn = new ReaLTaiizor.Controls.Button();
            HideBtn = new ReaLTaiizor.Controls.Button();
            UnlockAllBtn = new ReaLTaiizor.Controls.Button();
            HideUserIDCheckBox = new ReaLTaiizor.Controls.HopeCheckBox();
            lostBorderPanel2 = new ReaLTaiizor.Controls.LostBorderPanel();
            AvatarIDLabel = new Label();
            AvatarIDTextBox = new ReaLTaiizor.Controls.HopeTextBox();
            UserIDLabel = new Label();
            UserIDTextBox = new ReaLTaiizor.Controls.HopeTextBox();
            AutoRestartCheckBox = new ReaLTaiizor.Controls.HopeCheckBox();
            RestartBtn = new ReaLTaiizor.Controls.Button();
            ResetAvatarBtn = new ReaLTaiizor.Controls.Button();
            UnlockBtn = new ReaLTaiizor.Controls.Button();
            AppendConsoleBtn = new ReaLTaiizor.Controls.Button();
            lostBorderPanel1.SuspendLayout();
            lostBorderPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // lostBorderPanel1
            // 
            lostBorderPanel1.BackColor = Color.FromArgb(44, 44, 44);
            lostBorderPanel1.BorderColor = Color.FromArgb(157, 59, 255);
            lostBorderPanel1.Controls.Add(AppendConsoleBtn);
            lostBorderPanel1.Controls.Add(OpenAviFileBtn);
            lostBorderPanel1.Controls.Add(HideBtn);
            lostBorderPanel1.Controls.Add(UnlockAllBtn);
            lostBorderPanel1.Controls.Add(HideUserIDCheckBox);
            lostBorderPanel1.Controls.Add(lostBorderPanel2);
            lostBorderPanel1.Controls.Add(AutoRestartCheckBox);
            lostBorderPanel1.Controls.Add(RestartBtn);
            lostBorderPanel1.Controls.Add(ResetAvatarBtn);
            lostBorderPanel1.Controls.Add(UnlockBtn);
            lostBorderPanel1.Font = new Font("Segoe UI", 12F);
            lostBorderPanel1.ForeColor = Color.White;
            lostBorderPanel1.Location = new Point(12, 12);
            lostBorderPanel1.Name = "lostBorderPanel1";
            lostBorderPanel1.Padding = new Padding(5);
            lostBorderPanel1.ShowText = false;
            lostBorderPanel1.Size = new Size(860, 537);
            lostBorderPanel1.TabIndex = 0;
            lostBorderPanel1.Text = "lostBorderPanel1";
            // 
            // OpenAviFileBtn
            // 
            OpenAviFileBtn.BackColor = Color.Transparent;
            OpenAviFileBtn.BorderColor = Color.FromArgb(32, 32, 32);
            OpenAviFileBtn.EnteredBorderColor = Color.FromArgb(157, 59, 255);
            OpenAviFileBtn.EnteredColor = Color.FromArgb(34, 34, 34);
            OpenAviFileBtn.Font = new Font("Microsoft Sans Serif", 12F);
            OpenAviFileBtn.Image = null;
            OpenAviFileBtn.ImageAlign = ContentAlignment.MiddleLeft;
            OpenAviFileBtn.InactiveColor = Color.FromArgb(22, 22, 22);
            OpenAviFileBtn.Location = new Point(676, 494);
            OpenAviFileBtn.Name = "OpenAviFileBtn";
            OpenAviFileBtn.PressedBorderColor = Color.White;
            OpenAviFileBtn.PressedColor = Color.MediumSlateBlue;
            OpenAviFileBtn.Size = new Size(171, 34);
            OpenAviFileBtn.TabIndex = 9;
            OpenAviFileBtn.Text = "Open Avatar File";
            OpenAviFileBtn.TextAlignment = StringAlignment.Center;
            OpenAviFileBtn.Click += OpenAviFileBtn_Click;
            // 
            // HideBtn
            // 
            HideBtn.BackColor = Color.Transparent;
            HideBtn.BorderColor = Color.FromArgb(32, 32, 32);
            HideBtn.EnteredBorderColor = Color.FromArgb(157, 59, 255);
            HideBtn.EnteredColor = Color.FromArgb(34, 34, 34);
            HideBtn.Font = new Font("Microsoft Sans Serif", 12F);
            HideBtn.Image = null;
            HideBtn.ImageAlign = ContentAlignment.MiddleLeft;
            HideBtn.InactiveColor = Color.FromArgb(22, 22, 22);
            HideBtn.Location = new Point(676, 454);
            HideBtn.Name = "HideBtn";
            HideBtn.PressedBorderColor = Color.White;
            HideBtn.PressedColor = Color.MediumSlateBlue;
            HideBtn.Size = new Size(171, 34);
            HideBtn.TabIndex = 8;
            HideBtn.Text = "HIDE";
            HideBtn.TextAlignment = StringAlignment.Center;
            HideBtn.Click += HideBtn_Click;
            // 
            // UnlockAllBtn
            // 
            UnlockAllBtn.BackColor = Color.Transparent;
            UnlockAllBtn.BorderColor = Color.FromArgb(32, 32, 32);
            UnlockAllBtn.EnteredBorderColor = Color.FromArgb(157, 59, 255);
            UnlockAllBtn.EnteredColor = Color.FromArgb(34, 34, 34);
            UnlockAllBtn.Font = new Font("Microsoft Sans Serif", 12F);
            UnlockAllBtn.Image = null;
            UnlockAllBtn.ImageAlign = ContentAlignment.MiddleLeft;
            UnlockAllBtn.InactiveColor = Color.FromArgb(22, 22, 22);
            UnlockAllBtn.Location = new Point(13, 494);
            UnlockAllBtn.Name = "UnlockAllBtn";
            UnlockAllBtn.PressedBorderColor = Color.White;
            UnlockAllBtn.PressedColor = Color.MediumSlateBlue;
            UnlockAllBtn.Size = new Size(332, 40);
            UnlockAllBtn.TabIndex = 7;
            UnlockAllBtn.Text = "Unlock All (Buggy)";
            UnlockAllBtn.TextAlignment = StringAlignment.Center;
            UnlockAllBtn.Click += UnlockAllBtn_Click;
            // 
            // HideUserIDCheckBox
            // 
            HideUserIDCheckBox.AutoSize = true;
            HideUserIDCheckBox.BackColor = Color.FromArgb(44, 44, 44);
            HideUserIDCheckBox.CheckedColor = Color.FromArgb(157, 59, 255);
            HideUserIDCheckBox.DisabledColor = Color.FromArgb(196, 198, 202);
            HideUserIDCheckBox.DisabledStringColor = Color.FromArgb(186, 187, 189);
            HideUserIDCheckBox.Enable = true;
            HideUserIDCheckBox.EnabledCheckedColor = Color.FromArgb(157, 59, 255);
            HideUserIDCheckBox.EnabledStringColor = Color.FromArgb(153, 153, 153);
            HideUserIDCheckBox.EnabledUncheckedColor = Color.FromArgb(156, 158, 161);
            HideUserIDCheckBox.Font = new Font("Segoe UI", 12F);
            HideUserIDCheckBox.ForeColor = Color.White;
            HideUserIDCheckBox.Location = new Point(725, 428);
            HideUserIDCheckBox.Name = "HideUserIDCheckBox";
            HideUserIDCheckBox.Size = new Size(122, 20);
            HideUserIDCheckBox.TabIndex = 6;
            HideUserIDCheckBox.Text = "Hide User ID";
            HideUserIDCheckBox.UseVisualStyleBackColor = false;
            HideUserIDCheckBox.CheckedChanged += HideUserIDCheckBox_CheckedChanged;
            // 
            // lostBorderPanel2
            // 
            lostBorderPanel2.BackColor = Color.FromArgb(34, 34, 34);
            lostBorderPanel2.BorderColor = Color.FromArgb(157, 59, 255);
            lostBorderPanel2.Controls.Add(AvatarIDLabel);
            lostBorderPanel2.Controls.Add(AvatarIDTextBox);
            lostBorderPanel2.Controls.Add(UserIDLabel);
            lostBorderPanel2.Controls.Add(UserIDTextBox);
            lostBorderPanel2.Font = new Font("Segoe UI", 12F);
            lostBorderPanel2.ForeColor = Color.White;
            lostBorderPanel2.Location = new Point(13, 12);
            lostBorderPanel2.Name = "lostBorderPanel2";
            lostBorderPanel2.Padding = new Padding(5);
            lostBorderPanel2.ShowText = false;
            lostBorderPanel2.Size = new Size(834, 384);
            lostBorderPanel2.TabIndex = 5;
            lostBorderPanel2.Text = "lostBorderPanel2";
            // 
            // AvatarIDLabel
            // 
            AvatarIDLabel.AutoSize = true;
            AvatarIDLabel.Location = new Point(12, 82);
            AvatarIDLabel.Name = "AvatarIDLabel";
            AvatarIDLabel.Size = new Size(74, 21);
            AvatarIDLabel.TabIndex = 7;
            AvatarIDLabel.Text = "Avatar ID";
            // 
            // AvatarIDTextBox
            // 
            AvatarIDTextBox.BackColor = Color.Black;
            AvatarIDTextBox.BaseColor = Color.FromArgb(157, 59, 255);
            AvatarIDTextBox.BorderColorA = Color.FromArgb(157, 59, 255);
            AvatarIDTextBox.BorderColorB = Color.FromArgb(157, 59, 255);
            AvatarIDTextBox.Font = new Font("Segoe UI", 12F);
            AvatarIDTextBox.ForeColor = Color.White;
            AvatarIDTextBox.Hint = "";
            AvatarIDTextBox.Location = new Point(12, 106);
            AvatarIDTextBox.MaxLength = 32767;
            AvatarIDTextBox.Multiline = false;
            AvatarIDTextBox.Name = "AvatarIDTextBox";
            AvatarIDTextBox.PasswordChar = '\0';
            AvatarIDTextBox.ScrollBars = ScrollBars.None;
            AvatarIDTextBox.SelectedText = "";
            AvatarIDTextBox.SelectionLength = 0;
            AvatarIDTextBox.SelectionStart = 0;
            AvatarIDTextBox.Size = new Size(426, 38);
            AvatarIDTextBox.TabIndex = 6;
            AvatarIDTextBox.TabStop = false;
            AvatarIDTextBox.Text = "avtr_XXXXXXXXXXXXXXXXXXXX";
            AvatarIDTextBox.UseSystemPasswordChar = false;
            // 
            // UserIDLabel
            // 
            UserIDLabel.AutoSize = true;
            UserIDLabel.Location = new Point(12, 9);
            UserIDLabel.Name = "UserIDLabel";
            UserIDLabel.Size = new Size(61, 21);
            UserIDLabel.TabIndex = 5;
            UserIDLabel.Text = "User ID";
            // 
            // UserIDTextBox
            // 
            UserIDTextBox.BackColor = Color.Black;
            UserIDTextBox.BaseColor = Color.FromArgb(157, 59, 255);
            UserIDTextBox.BorderColorA = Color.FromArgb(157, 59, 255);
            UserIDTextBox.BorderColorB = Color.FromArgb(157, 59, 255);
            UserIDTextBox.Font = new Font("Segoe UI", 12F);
            UserIDTextBox.ForeColor = Color.White;
            UserIDTextBox.Hint = "";
            UserIDTextBox.Location = new Point(12, 33);
            UserIDTextBox.MaxLength = 32767;
            UserIDTextBox.Multiline = false;
            UserIDTextBox.Name = "UserIDTextBox";
            UserIDTextBox.PasswordChar = '\0';
            UserIDTextBox.ScrollBars = ScrollBars.None;
            UserIDTextBox.SelectedText = "";
            UserIDTextBox.SelectionLength = 0;
            UserIDTextBox.SelectionStart = 0;
            UserIDTextBox.Size = new Size(426, 38);
            UserIDTextBox.TabIndex = 4;
            UserIDTextBox.TabStop = false;
            UserIDTextBox.Text = "usr_XXXXXXXXXXXXXXXXXXXX";
            UserIDTextBox.UseSystemPasswordChar = false;
            // 
            // AutoRestartCheckBox
            // 
            AutoRestartCheckBox.AutoSize = true;
            AutoRestartCheckBox.BackColor = Color.FromArgb(44, 44, 44);
            AutoRestartCheckBox.CheckedColor = Color.FromArgb(157, 59, 255);
            AutoRestartCheckBox.DisabledColor = Color.FromArgb(196, 198, 202);
            AutoRestartCheckBox.DisabledStringColor = Color.FromArgb(186, 187, 189);
            AutoRestartCheckBox.Enable = true;
            AutoRestartCheckBox.EnabledCheckedColor = Color.FromArgb(157, 59, 255);
            AutoRestartCheckBox.EnabledStringColor = Color.FromArgb(153, 153, 153);
            AutoRestartCheckBox.EnabledUncheckedColor = Color.FromArgb(156, 158, 161);
            AutoRestartCheckBox.Font = new Font("Segoe UI", 12F);
            AutoRestartCheckBox.ForeColor = Color.White;
            AutoRestartCheckBox.Location = new Point(726, 402);
            AutoRestartCheckBox.Name = "AutoRestartCheckBox";
            AutoRestartCheckBox.Size = new Size(121, 20);
            AutoRestartCheckBox.TabIndex = 3;
            AutoRestartCheckBox.Text = "Auto Restart";
            AutoRestartCheckBox.UseVisualStyleBackColor = false;
            AutoRestartCheckBox.CheckedChanged += AutoRestartCheckBox_CheckedChanged;
            // 
            // RestartBtn
            // 
            RestartBtn.BackColor = Color.Transparent;
            RestartBtn.BorderColor = Color.FromArgb(32, 32, 32);
            RestartBtn.EnteredBorderColor = Color.FromArgb(157, 59, 255);
            RestartBtn.EnteredColor = Color.FromArgb(34, 34, 34);
            RestartBtn.Font = new Font("Microsoft Sans Serif", 12F);
            RestartBtn.Image = null;
            RestartBtn.ImageAlign = ContentAlignment.MiddleLeft;
            RestartBtn.InactiveColor = Color.FromArgb(22, 22, 22);
            RestartBtn.Location = new Point(13, 448);
            RestartBtn.Name = "RestartBtn";
            RestartBtn.PressedBorderColor = Color.White;
            RestartBtn.PressedColor = Color.MediumSlateBlue;
            RestartBtn.Size = new Size(332, 40);
            RestartBtn.TabIndex = 2;
            RestartBtn.Text = "Restart";
            RestartBtn.TextAlignment = StringAlignment.Center;
            RestartBtn.Click += RestartBtn_Click;
            // 
            // ResetAvatarBtn
            // 
            ResetAvatarBtn.BackColor = Color.Transparent;
            ResetAvatarBtn.BorderColor = Color.FromArgb(32, 32, 32);
            ResetAvatarBtn.EnteredBorderColor = Color.FromArgb(157, 59, 255);
            ResetAvatarBtn.EnteredColor = Color.FromArgb(34, 34, 34);
            ResetAvatarBtn.Font = new Font("Microsoft Sans Serif", 12F);
            ResetAvatarBtn.Image = null;
            ResetAvatarBtn.ImageAlign = ContentAlignment.MiddleLeft;
            ResetAvatarBtn.InactiveColor = Color.FromArgb(22, 22, 22);
            ResetAvatarBtn.Location = new Point(182, 402);
            ResetAvatarBtn.Name = "ResetAvatarBtn";
            ResetAvatarBtn.PressedBorderColor = Color.White;
            ResetAvatarBtn.PressedColor = Color.MediumSlateBlue;
            ResetAvatarBtn.Size = new Size(163, 40);
            ResetAvatarBtn.TabIndex = 1;
            ResetAvatarBtn.Text = "Reset";
            ResetAvatarBtn.TextAlignment = StringAlignment.Center;
            ResetAvatarBtn.Click += ResetAvatarBtn_Click;
            // 
            // UnlockBtn
            // 
            UnlockBtn.BackColor = Color.Transparent;
            UnlockBtn.BorderColor = Color.FromArgb(32, 32, 32);
            UnlockBtn.EnteredBorderColor = Color.FromArgb(157, 59, 255);
            UnlockBtn.EnteredColor = Color.FromArgb(34, 34, 34);
            UnlockBtn.Font = new Font("Microsoft Sans Serif", 12F);
            UnlockBtn.Image = null;
            UnlockBtn.ImageAlign = ContentAlignment.MiddleLeft;
            UnlockBtn.InactiveColor = Color.FromArgb(22, 22, 22);
            UnlockBtn.Location = new Point(13, 402);
            UnlockBtn.Name = "UnlockBtn";
            UnlockBtn.PressedBorderColor = Color.White;
            UnlockBtn.PressedColor = Color.MediumSlateBlue;
            UnlockBtn.Size = new Size(163, 40);
            UnlockBtn.TabIndex = 0;
            UnlockBtn.Text = "Unlock";
            UnlockBtn.TextAlignment = StringAlignment.Center;
            UnlockBtn.Click += UnlockBtn_Click;
            // 
            // AppendConsoleBtn
            // 
            AppendConsoleBtn.BackColor = Color.Transparent;
            AppendConsoleBtn.BorderColor = Color.FromArgb(32, 32, 32);
            AppendConsoleBtn.EnteredBorderColor = Color.FromArgb(157, 59, 255);
            AppendConsoleBtn.EnteredColor = Color.FromArgb(34, 34, 34);
            AppendConsoleBtn.Font = new Font("Microsoft Sans Serif", 12F);
            AppendConsoleBtn.Image = null;
            AppendConsoleBtn.ImageAlign = ContentAlignment.MiddleLeft;
            AppendConsoleBtn.InactiveColor = Color.FromArgb(22, 22, 22);
            AppendConsoleBtn.Location = new Point(591, 454);
            AppendConsoleBtn.Name = "AppendConsoleBtn";
            AppendConsoleBtn.PressedBorderColor = Color.White;
            AppendConsoleBtn.PressedColor = Color.MediumSlateBlue;
            AppendConsoleBtn.Size = new Size(79, 74);
            AppendConsoleBtn.TabIndex = 10;
            AppendConsoleBtn.Text = "Append Console";
            AppendConsoleBtn.TextAlignment = StringAlignment.Center;
            AppendConsoleBtn.Click += AppendConsoleBtn_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(35, 35, 35);
            ClientSize = new Size(884, 561);
            Controls.Add(lostBorderPanel1);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MaximumSize = new Size(900, 600);
            MinimumSize = new Size(900, 600);
            Name = "MainForm";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Avatar Lockpick";
            Load += MainForm_Load;
            lostBorderPanel1.ResumeLayout(false);
            lostBorderPanel1.PerformLayout();
            lostBorderPanel2.ResumeLayout(false);
            lostBorderPanel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private ReaLTaiizor.Controls.LostBorderPanel lostBorderPanel1;
        private ReaLTaiizor.Controls.Button RestartBtn;
        private ReaLTaiizor.Controls.Button ResetAvatarBtn;
        private ReaLTaiizor.Controls.Button UnlockBtn;
        private ReaLTaiizor.Controls.HopeCheckBox AutoRestartCheckBox;
        private ReaLTaiizor.Controls.LostBorderPanel lostBorderPanel2;
        private ReaLTaiizor.Controls.HopeTextBox UserIDTextBox;
        private Label AvatarIDLabel;
        private ReaLTaiizor.Controls.HopeTextBox AvatarIDTextBox;
        private Label UserIDLabel;
        private ReaLTaiizor.Controls.HopeCheckBox HideUserIDCheckBox;
        private ReaLTaiizor.Controls.Button UnlockAllBtn;
        private ReaLTaiizor.Controls.Button HideBtn;
        private ReaLTaiizor.Controls.Button OpenAviFileBtn;
        private ReaLTaiizor.Controls.Button AppendConsoleBtn;
    }
}
