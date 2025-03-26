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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            AvatarMainPanel = new ReaLTaiizor.Controls.LostBorderPanel();
            VERSIONTEXT = new Label();
            HelpBtn = new ReaLTaiizor.Controls.LostButton();
            VersionLabel = new Label();
            HideWindowButton = new ReaLTaiizor.Controls.LostButton();
            AppendConsoleButton = new ReaLTaiizor.Controls.LostButton();
            RestartButton = new ReaLTaiizor.Controls.LostButton();
            ClearSavesButton = new ReaLTaiizor.Controls.LostButton();
            DeleteSavesButton = new ReaLTaiizor.Controls.LostButton();
            RestartWithVRCheckBox = new ReaLTaiizor.Controls.HopeCheckBox();
            HideUserIDCheckBox = new ReaLTaiizor.Controls.HopeCheckBox();
            lostBorderPanel2 = new ReaLTaiizor.Controls.LostBorderPanel();
            AvatarIDLabel = new Label();
            OpenAvatarButton = new ReaLTaiizor.Controls.LostButton();
            AvatarIDTextBox = new ReaLTaiizor.Controls.HopeTextBox();
            UserIDLabel = new Label();
            UserIDTextBox = new ReaLTaiizor.Controls.HopeTextBox();
            UnlockButton = new ReaLTaiizor.Controls.LostButton();
            UnlockALLButton = new ReaLTaiizor.Controls.LostButton();
            ResetButton = new ReaLTaiizor.Controls.LostButton();
            UnlockVRCFuryButton = new ReaLTaiizor.Controls.LostButton();
            AutoRestartCheckBox = new ReaLTaiizor.Controls.HopeCheckBox();
            lostBorderPanel1 = new ReaLTaiizor.Controls.LostBorderPanel();
            DiscordBtn = new ReaLTaiizor.Controls.LostButton();
            WebsiteBtn = new ReaLTaiizor.Controls.LostButton();
            GithubBtn = new ReaLTaiizor.Controls.LostButton();
            AvatarMainPanel.SuspendLayout();
            lostBorderPanel2.SuspendLayout();
            lostBorderPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // AvatarMainPanel
            // 
            AvatarMainPanel.BackColor = Color.FromArgb(44, 44, 44);
            AvatarMainPanel.BorderColor = Color.FromArgb(157, 59, 255);
            AvatarMainPanel.Controls.Add(VERSIONTEXT);
            AvatarMainPanel.Controls.Add(HelpBtn);
            AvatarMainPanel.Controls.Add(VersionLabel);
            AvatarMainPanel.Controls.Add(HideWindowButton);
            AvatarMainPanel.Controls.Add(AppendConsoleButton);
            AvatarMainPanel.Controls.Add(RestartButton);
            AvatarMainPanel.Controls.Add(ClearSavesButton);
            AvatarMainPanel.Controls.Add(DeleteSavesButton);
            AvatarMainPanel.Controls.Add(RestartWithVRCheckBox);
            AvatarMainPanel.Controls.Add(HideUserIDCheckBox);
            AvatarMainPanel.Controls.Add(lostBorderPanel2);
            AvatarMainPanel.Controls.Add(AutoRestartCheckBox);
            AvatarMainPanel.Font = new Font("Segoe UI", 12F);
            AvatarMainPanel.ForeColor = Color.White;
            AvatarMainPanel.Location = new Point(12, 12);
            AvatarMainPanel.Name = "AvatarMainPanel";
            AvatarMainPanel.Padding = new Padding(5);
            AvatarMainPanel.ShowText = false;
            AvatarMainPanel.Size = new Size(629, 537);
            AvatarMainPanel.TabIndex = 0;
            AvatarMainPanel.Text = "lostBorderPanel1";
            // 
            // VERSIONTEXT
            // 
            VERSIONTEXT.AutoSize = true;
            VERSIONTEXT.ForeColor = Color.FromArgb(157, 59, 255);
            VERSIONTEXT.Location = new Point(25, 513);
            VERSIONTEXT.Name = "VERSIONTEXT";
            VERSIONTEXT.Size = new Size(49, 21);
            VERSIONTEXT.TabIndex = 14;
            VERSIONTEXT.Text = "NULL";
            // 
            // HelpBtn
            // 
            HelpBtn.BackColor = Color.FromArgb(30, 30, 30);
            HelpBtn.Font = new Font("Segoe UI", 9F);
            HelpBtn.ForeColor = Color.White;
            HelpBtn.HoverColor = Color.FromArgb(157, 59, 255);
            HelpBtn.Image = null;
            HelpBtn.Location = new Point(561, 494);
            HelpBtn.Name = "HelpBtn";
            HelpBtn.Size = new Size(65, 40);
            HelpBtn.TabIndex = 24;
            HelpBtn.Text = "HELP!";
            HelpBtn.Click += HelpBtn_Click;
            // 
            // VersionLabel
            // 
            VersionLabel.AutoSize = true;
            VersionLabel.Location = new Point(3, 513);
            VersionLabel.Name = "VersionLabel";
            VersionLabel.Size = new Size(23, 21);
            VersionLabel.TabIndex = 13;
            VersionLabel.Text = "V:";
            // 
            // HideWindowButton
            // 
            HideWindowButton.BackColor = Color.FromArgb(30, 30, 30);
            HideWindowButton.Font = new Font("Segoe UI", 9F);
            HideWindowButton.ForeColor = Color.White;
            HideWindowButton.HoverColor = Color.Crimson;
            HideWindowButton.Image = null;
            HideWindowButton.Location = new Point(360, 369);
            HideWindowButton.Name = "HideWindowButton";
            HideWindowButton.Size = new Size(120, 40);
            HideWindowButton.TabIndex = 21;
            HideWindowButton.Text = "HIDE";
            HideWindowButton.Click += HideWindowButton_Click;
            // 
            // AppendConsoleButton
            // 
            AppendConsoleButton.BackColor = Color.FromArgb(30, 30, 30);
            AppendConsoleButton.Font = new Font("Segoe UI", 9F);
            AppendConsoleButton.ForeColor = Color.White;
            AppendConsoleButton.HoverColor = Color.FromArgb(157, 59, 255);
            AppendConsoleButton.Image = null;
            AppendConsoleButton.Location = new Point(13, 369);
            AppendConsoleButton.Name = "AppendConsoleButton";
            AppendConsoleButton.Size = new Size(120, 40);
            AppendConsoleButton.TabIndex = 20;
            AppendConsoleButton.Text = "Show Console";
            AppendConsoleButton.Click += AppendConsoleButton_Click;
            // 
            // RestartButton
            // 
            RestartButton.BackColor = Color.FromArgb(30, 30, 30);
            RestartButton.Font = new Font("Segoe UI", 9F);
            RestartButton.ForeColor = Color.White;
            RestartButton.HoverColor = Color.FromArgb(157, 59, 255);
            RestartButton.Image = null;
            RestartButton.Location = new Point(139, 369);
            RestartButton.Name = "RestartButton";
            RestartButton.Size = new Size(215, 40);
            RestartButton.TabIndex = 18;
            RestartButton.Text = "Restart";
            RestartButton.Click += RestartButton_Click;
            // 
            // ClearSavesButton
            // 
            ClearSavesButton.BackColor = Color.FromArgb(30, 30, 30);
            ClearSavesButton.Font = new Font("Segoe UI", 9F);
            ClearSavesButton.ForeColor = Color.White;
            ClearSavesButton.HoverColor = Color.FromArgb(157, 59, 255);
            ClearSavesButton.Image = null;
            ClearSavesButton.Location = new Point(486, 58);
            ClearSavesButton.Name = "ClearSavesButton";
            ClearSavesButton.Size = new Size(135, 40);
            ClearSavesButton.TabIndex = 14;
            ClearSavesButton.Text = "Clear Saves";
            ClearSavesButton.Click += ClearSavesButton_Click;
            // 
            // DeleteSavesButton
            // 
            DeleteSavesButton.BackColor = Color.FromArgb(30, 30, 30);
            DeleteSavesButton.Font = new Font("Segoe UI", 9F);
            DeleteSavesButton.ForeColor = Color.White;
            DeleteSavesButton.HoverColor = Color.FromArgb(157, 59, 255);
            DeleteSavesButton.Image = null;
            DeleteSavesButton.Location = new Point(486, 12);
            DeleteSavesButton.Name = "DeleteSavesButton";
            DeleteSavesButton.Size = new Size(135, 40);
            DeleteSavesButton.TabIndex = 13;
            DeleteSavesButton.Text = "Delete Saves";
            DeleteSavesButton.Click += DeleteSavesButton_Click;
            // 
            // RestartWithVRCheckBox
            // 
            RestartWithVRCheckBox.AutoSize = true;
            RestartWithVRCheckBox.BackColor = Color.FromArgb(44, 44, 44);
            RestartWithVRCheckBox.CheckedColor = Color.FromArgb(157, 59, 255);
            RestartWithVRCheckBox.DisabledColor = Color.FromArgb(196, 198, 202);
            RestartWithVRCheckBox.DisabledStringColor = Color.FromArgb(186, 187, 189);
            RestartWithVRCheckBox.Enable = true;
            RestartWithVRCheckBox.EnabledCheckedColor = Color.FromArgb(157, 59, 255);
            RestartWithVRCheckBox.EnabledStringColor = Color.FromArgb(153, 153, 153);
            RestartWithVRCheckBox.EnabledUncheckedColor = Color.FromArgb(156, 158, 161);
            RestartWithVRCheckBox.Font = new Font("Segoe UI", 12F);
            RestartWithVRCheckBox.ForeColor = Color.White;
            RestartWithVRCheckBox.Location = new Point(486, 104);
            RestartWithVRCheckBox.Name = "RestartWithVRCheckBox";
            RestartWithVRCheckBox.Size = new Size(125, 20);
            RestartWithVRCheckBox.TabIndex = 12;
            RestartWithVRCheckBox.Text = "Restart in VR";
            RestartWithVRCheckBox.UseVisualStyleBackColor = false;
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
            HideUserIDCheckBox.Location = new Point(486, 156);
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
            lostBorderPanel2.Controls.Add(OpenAvatarButton);
            lostBorderPanel2.Controls.Add(AvatarIDTextBox);
            lostBorderPanel2.Controls.Add(UserIDLabel);
            lostBorderPanel2.Controls.Add(UserIDTextBox);
            lostBorderPanel2.Controls.Add(UnlockButton);
            lostBorderPanel2.Controls.Add(UnlockALLButton);
            lostBorderPanel2.Controls.Add(ResetButton);
            lostBorderPanel2.Controls.Add(UnlockVRCFuryButton);
            lostBorderPanel2.Font = new Font("Segoe UI", 12F);
            lostBorderPanel2.ForeColor = Color.White;
            lostBorderPanel2.Location = new Point(13, 12);
            lostBorderPanel2.Name = "lostBorderPanel2";
            lostBorderPanel2.Padding = new Padding(5);
            lostBorderPanel2.ShowText = false;
            lostBorderPanel2.Size = new Size(467, 351);
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
            // OpenAvatarButton
            // 
            OpenAvatarButton.BackColor = Color.FromArgb(20, 20, 20);
            OpenAvatarButton.Font = new Font("Segoe UI", 9F);
            OpenAvatarButton.ForeColor = Color.White;
            OpenAvatarButton.HoverColor = Color.FromArgb(157, 59, 255);
            OpenAvatarButton.Image = null;
            OpenAvatarButton.Location = new Point(12, 288);
            OpenAvatarButton.Name = "OpenAvatarButton";
            OpenAvatarButton.Size = new Size(441, 40);
            OpenAvatarButton.TabIndex = 22;
            OpenAvatarButton.Text = "Open Avatar File";
            OpenAvatarButton.Click += OpenAvatarButton_Click;
            // 
            // AvatarIDTextBox
            // 
            AvatarIDTextBox.BackColor = Color.FromArgb(40, 40, 40);
            AvatarIDTextBox.BaseColor = Color.FromArgb(44, 55, 66);
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
            AvatarIDTextBox.Size = new Size(441, 38);
            AvatarIDTextBox.TabIndex = 6;
            AvatarIDTextBox.TabStop = false;
            AvatarIDTextBox.Text = "avtr_XXXXXXXXXXXXXXXXXXXX";
            AvatarIDTextBox.UseSystemPasswordChar = false;
            AvatarIDTextBox.TextChanged += AvatarIDTextBox_TextChanged;
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
            UserIDTextBox.BackColor = Color.FromArgb(40, 40, 40);
            UserIDTextBox.BaseColor = Color.FromArgb(44, 55, 66);
            UserIDTextBox.BorderColorA = Color.FromArgb(157, 59, 255);
            UserIDTextBox.BorderColorB = Color.FromArgb(157, 59, 255);
            UserIDTextBox.Font = new Font("Segoe UI", 12F);
            UserIDTextBox.ForeColor = Color.White;
            UserIDTextBox.Hint = "";
            UserIDTextBox.Location = new Point(12, 32);
            UserIDTextBox.MaxLength = 32767;
            UserIDTextBox.Multiline = false;
            UserIDTextBox.Name = "UserIDTextBox";
            UserIDTextBox.PasswordChar = '\0';
            UserIDTextBox.ScrollBars = ScrollBars.None;
            UserIDTextBox.SelectedText = "";
            UserIDTextBox.SelectionLength = 0;
            UserIDTextBox.SelectionStart = 0;
            UserIDTextBox.Size = new Size(441, 38);
            UserIDTextBox.TabIndex = 5;
            UserIDTextBox.TabStop = false;
            UserIDTextBox.Text = "usr_XXXXXXXXXXXXXXXXXXXX";
            UserIDTextBox.UseSystemPasswordChar = false;
            UserIDTextBox.TextChanged += UserIDTextBox_TextChanged;
            // 
            // UnlockButton
            // 
            UnlockButton.BackColor = Color.FromArgb(20, 20, 20);
            UnlockButton.Font = new Font("Segoe UI", 9F);
            UnlockButton.ForeColor = Color.White;
            UnlockButton.HoverColor = Color.LimeGreen;
            UnlockButton.Image = null;
            UnlockButton.Location = new Point(12, 150);
            UnlockButton.Name = "UnlockButton";
            UnlockButton.Size = new Size(224, 40);
            UnlockButton.TabIndex = 15;
            UnlockButton.Text = "Unlock Avatar";
            UnlockButton.Click += UnlockButton_Click;
            // 
            // UnlockALLButton
            // 
            UnlockALLButton.BackColor = Color.FromArgb(20, 20, 20);
            UnlockALLButton.Font = new Font("Segoe UI", 9F);
            UnlockALLButton.ForeColor = Color.White;
            UnlockALLButton.HoverColor = Color.Gold;
            UnlockALLButton.Image = null;
            UnlockALLButton.Location = new Point(12, 196);
            UnlockALLButton.Name = "UnlockALLButton";
            UnlockALLButton.Size = new Size(441, 40);
            UnlockALLButton.TabIndex = 19;
            UnlockALLButton.Text = "Unlock ALL";
            UnlockALLButton.Click += UnlockALLButton_Click;
            // 
            // ResetButton
            // 
            ResetButton.BackColor = Color.FromArgb(20, 20, 20);
            ResetButton.Font = new Font("Segoe UI", 9F);
            ResetButton.ForeColor = Color.White;
            ResetButton.HoverColor = Color.Crimson;
            ResetButton.Image = null;
            ResetButton.Location = new Point(12, 242);
            ResetButton.Name = "ResetButton";
            ResetButton.Size = new Size(441, 40);
            ResetButton.TabIndex = 17;
            ResetButton.Text = "Reset Avatar";
            ResetButton.Click += ResetButton_Click;
            // 
            // UnlockVRCFuryButton
            // 
            UnlockVRCFuryButton.BackColor = Color.FromArgb(20, 20, 20);
            UnlockVRCFuryButton.Font = new Font("Segoe UI", 9F);
            UnlockVRCFuryButton.ForeColor = Color.White;
            UnlockVRCFuryButton.HoverColor = Color.LimeGreen;
            UnlockVRCFuryButton.Image = null;
            UnlockVRCFuryButton.Location = new Point(242, 150);
            UnlockVRCFuryButton.Name = "UnlockVRCFuryButton";
            UnlockVRCFuryButton.Size = new Size(211, 40);
            UnlockVRCFuryButton.TabIndex = 16;
            UnlockVRCFuryButton.Text = "Unlock [VRCFURY]";
            UnlockVRCFuryButton.Click += UnlockVRCFuryButton_Click;
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
            AutoRestartCheckBox.Location = new Point(486, 130);
            AutoRestartCheckBox.Name = "AutoRestartCheckBox";
            AutoRestartCheckBox.Size = new Size(121, 20);
            AutoRestartCheckBox.TabIndex = 3;
            AutoRestartCheckBox.Text = "Auto Restart";
            AutoRestartCheckBox.UseVisualStyleBackColor = false;
            AutoRestartCheckBox.CheckedChanged += AutoRestartCheckBox_CheckedChanged;
            // 
            // lostBorderPanel1
            // 
            lostBorderPanel1.BackColor = Color.FromArgb(44, 44, 44);
            lostBorderPanel1.BorderColor = Color.FromArgb(157, 59, 255);
            lostBorderPanel1.Controls.Add(DiscordBtn);
            lostBorderPanel1.Controls.Add(WebsiteBtn);
            lostBorderPanel1.Controls.Add(GithubBtn);
            lostBorderPanel1.Font = new Font("Segoe UI", 12F);
            lostBorderPanel1.ForeColor = Color.White;
            lostBorderPanel1.Location = new Point(647, 12);
            lostBorderPanel1.Name = "lostBorderPanel1";
            lostBorderPanel1.Padding = new Padding(5);
            lostBorderPanel1.ShowText = false;
            lostBorderPanel1.Size = new Size(225, 537);
            lostBorderPanel1.TabIndex = 22;
            lostBorderPanel1.Text = "lostBorderPanel1";
            // 
            // DiscordBtn
            // 
            DiscordBtn.BackColor = Color.FromArgb(30, 30, 30);
            DiscordBtn.Font = new Font("Segoe UI", 9F);
            DiscordBtn.ForeColor = Color.White;
            DiscordBtn.HoverColor = Color.FromArgb(157, 59, 255);
            DiscordBtn.Image = null;
            DiscordBtn.Location = new Point(3, 104);
            DiscordBtn.Name = "DiscordBtn";
            DiscordBtn.Size = new Size(219, 40);
            DiscordBtn.TabIndex = 25;
            DiscordBtn.Text = "Discord";
            DiscordBtn.Click += DiscordBtn_Click;
            // 
            // WebsiteBtn
            // 
            WebsiteBtn.BackColor = Color.FromArgb(30, 30, 30);
            WebsiteBtn.Font = new Font("Segoe UI", 9F);
            WebsiteBtn.ForeColor = Color.White;
            WebsiteBtn.HoverColor = Color.FromArgb(157, 59, 255);
            WebsiteBtn.Image = null;
            WebsiteBtn.Location = new Point(3, 58);
            WebsiteBtn.Name = "WebsiteBtn";
            WebsiteBtn.Size = new Size(219, 40);
            WebsiteBtn.TabIndex = 23;
            WebsiteBtn.Text = "Website";
            WebsiteBtn.Click += WebsiteBtn_Click;
            // 
            // GithubBtn
            // 
            GithubBtn.BackColor = Color.FromArgb(30, 30, 30);
            GithubBtn.Font = new Font("Segoe UI", 9F);
            GithubBtn.ForeColor = Color.White;
            GithubBtn.HoverColor = Color.FromArgb(157, 59, 255);
            GithubBtn.Image = null;
            GithubBtn.Location = new Point(3, 12);
            GithubBtn.Name = "GithubBtn";
            GithubBtn.Size = new Size(219, 40);
            GithubBtn.TabIndex = 22;
            GithubBtn.Text = "Github";
            GithubBtn.Click += GithubBtn_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(35, 35, 35);
            ClientSize = new Size(884, 561);
            Controls.Add(lostBorderPanel1);
            Controls.Add(AvatarMainPanel);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MaximumSize = new Size(900, 600);
            MinimumSize = new Size(900, 600);
            Name = "MainForm";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Avatar Lockpick";
            Load += MainForm_Load;
            AvatarMainPanel.ResumeLayout(false);
            AvatarMainPanel.PerformLayout();
            lostBorderPanel2.ResumeLayout(false);
            lostBorderPanel2.PerformLayout();
            lostBorderPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ReaLTaiizor.Controls.LostBorderPanel AvatarMainPanel;
        private ReaLTaiizor.Controls.HopeCheckBox AutoRestartCheckBox;
        private ReaLTaiizor.Controls.LostBorderPanel lostBorderPanel2;
        private ReaLTaiizor.Controls.HopeTextBox UserIDTextBox;
        private Label AvatarIDLabel;
        private ReaLTaiizor.Controls.HopeTextBox AvatarIDTextBox;
        private Label UserIDLabel;
        private ReaLTaiizor.Controls.HopeCheckBox HideUserIDCheckBox;
        private Label VersionLabel;
        private Label VERSIONTEXT;
        private ReaLTaiizor.Controls.HopeCheckBox RestartWithVRCheckBox;
        private ReaLTaiizor.Controls.LostButton DeleteSavesButton;
        private ReaLTaiizor.Controls.LostButton OpenAvatarButton;
        private ReaLTaiizor.Controls.LostButton HideWindowButton;
        private ReaLTaiizor.Controls.LostButton AppendConsoleButton;
        private ReaLTaiizor.Controls.LostButton UnlockALLButton;
        private ReaLTaiizor.Controls.LostButton RestartButton;
        private ReaLTaiizor.Controls.LostButton ResetButton;
        private ReaLTaiizor.Controls.LostButton UnlockVRCFuryButton;
        private ReaLTaiizor.Controls.LostButton UnlockButton;
        private ReaLTaiizor.Controls.LostButton ClearSavesButton;
        private ReaLTaiizor.Controls.LostBorderPanel lostBorderPanel1;
        private ReaLTaiizor.Controls.LostButton HelpBtn;
        private ReaLTaiizor.Controls.LostButton WebsiteBtn;
        private ReaLTaiizor.Controls.LostButton GithubBtn;
        private ReaLTaiizor.Controls.LostButton DiscordBtn;
    }
}
