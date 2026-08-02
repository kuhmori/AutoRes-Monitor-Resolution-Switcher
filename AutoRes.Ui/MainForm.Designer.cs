namespace AutoRes.Ui
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.cbProcesses = new System.Windows.Forms.ComboBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnSetNormal = new System.Windows.Forms.Button();
            this.btnSaveGame = new System.Windows.Forms.Button();
            this.btnToggleDark = new System.Windows.Forms.Button();
            this.chkAutostart = new System.Windows.Forms.CheckBox();
            this.chkStartMinimized = new System.Windows.Forms.CheckBox();
            this.lblNormal = new System.Windows.Forms.Label();
            this.lblGameProfiles = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblWidth = new System.Windows.Forms.Label();
            this.lblHeight = new System.Windows.Forms.Label();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.lbGameProfiles = new System.Windows.Forms.ListBox();
            this.btnDeleteGame = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.SuspendLayout();

            // cbProcesses
            this.cbProcesses.FormattingEnabled = true;
            this.cbProcesses.Location = new System.Drawing.Point(12, 12);
            this.cbProcesses.Name = "cbProcesses";
            this.cbProcesses.Size = new System.Drawing.Size(320, 21);
            this.cbProcesses.TabIndex = 0;
            this.cbProcesses.SelectedIndexChanged += new System.EventHandler(this.cbProcesses_SelectedIndexChanged);

            // btnRefresh
            this.btnRefresh.Location = new System.Drawing.Point(338, 10);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            // btnSetNormal
            this.btnSetNormal.Location = new System.Drawing.Point(12, 50);
            this.btnSetNormal.Name = "btnSetNormal";
            this.btnSetNormal.Size = new System.Drawing.Size(100, 23);
            this.btnSetNormal.TabIndex = 2;
            this.btnSetNormal.Text = "Set Normal";
            this.btnSetNormal.UseVisualStyleBackColor = true;
            this.btnSetNormal.Click += new System.EventHandler(this.btnSetNormal_Click);

            // btnSaveGame
            this.btnSaveGame.Location = new System.Drawing.Point(118, 50);
            this.btnSaveGame.Name = "btnSaveGame";
            this.btnSaveGame.Size = new System.Drawing.Size(100, 23);
            this.btnSaveGame.TabIndex = 3;
            this.btnSaveGame.Text = "Save Game";
            this.btnSaveGame.UseVisualStyleBackColor = true;
            this.btnSaveGame.Click += new System.EventHandler(this.btnSaveGame_Click);

            // btnToggleDark
            this.btnToggleDark.Location = new System.Drawing.Point(224, 50);
            this.btnToggleDark.Name = "btnToggleDark";
            this.btnToggleDark.Size = new System.Drawing.Size(129, 23);
            this.btnToggleDark.TabIndex = 4;
            this.btnToggleDark.Text = "Toggle Dark Mode";
            this.btnToggleDark.UseVisualStyleBackColor = true;
            this.btnToggleDark.Click += new System.EventHandler(this.btnToggleDark_Click);

            // chkAutostart
            this.chkAutostart.AutoSize = true;
            this.chkAutostart.Location = new System.Drawing.Point(12, 90);
            this.chkAutostart.Name = "chkAutostart";
            this.chkAutostart.Size = new System.Drawing.Size(123, 17);
            this.chkAutostart.TabIndex = 5;
            this.chkAutostart.Text = "Autostart with Windows";
            this.chkAutostart.UseVisualStyleBackColor = true;
            this.chkAutostart.CheckedChanged += new System.EventHandler(this.chkAutostart_CheckedChanged);

            // chkStartMinimized
            this.chkStartMinimized.AutoSize = true;
            this.chkStartMinimized.Location = new System.Drawing.Point(180, 90);
            this.chkStartMinimized.Name = "chkStartMinimized";
            this.chkStartMinimized.Size = new System.Drawing.Size(100, 17);
            this.chkStartMinimized.TabIndex = 6;
            this.chkStartMinimized.Text = "Start Minimized";
            this.chkStartMinimized.UseVisualStyleBackColor = true;
            this.chkStartMinimized.CheckedChanged += new System.EventHandler(this.chkStartMinimized_CheckedChanged);

            // lblWidth
            this.lblWidth.AutoSize = true;
            this.lblWidth.Location = new System.Drawing.Point(12, 115);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(38, 13);
            this.lblWidth.TabIndex = 9;
            this.lblWidth.Text = "Width:";

            // txtWidth
            this.txtWidth.Location = new System.Drawing.Point(56, 112);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(60, 20);
            this.txtWidth.TabIndex = 10;
            this.txtWidth.Text = "";

            // lblHeight
            this.lblHeight.AutoSize = true;
            this.lblHeight.Location = new System.Drawing.Point(125, 115);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(41, 13);
            this.lblHeight.TabIndex = 11;
            this.lblHeight.Text = "Height:";

            // txtHeight
            this.txtHeight.Location = new System.Drawing.Point(172, 112);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(60, 20);
            this.txtHeight.TabIndex = 12;
            this.txtHeight.Text = "";

            // lblNormal
            this.lblNormal.AutoSize = true;
            this.lblNormal.Location = new System.Drawing.Point(12, 145);
            this.lblNormal.Name = "lblNormal";
            this.lblNormal.Size = new System.Drawing.Size(44, 13);
            this.lblNormal.TabIndex = 6;
            this.lblNormal.Text = "Normal:";

            // lblGameProfiles
            this.lblGameProfiles.AutoSize = true;
            this.lblGameProfiles.Location = new System.Drawing.Point(12, 165);
            this.lblGameProfiles.Name = "lblGameProfiles";
            this.lblGameProfiles.Size = new System.Drawing.Size(73, 13);
            this.lblGameProfiles.TabIndex = 17;
            this.lblGameProfiles.Text = "Game Profiles:";

            // lbGameProfiles
            this.lbGameProfiles.FormattingEnabled = true;
            this.lbGameProfiles.Location = new System.Drawing.Point(12, 181);
            this.lbGameProfiles.Name = "lbGameProfiles";
            this.lbGameProfiles.Size = new System.Drawing.Size(320, 95);
            this.lbGameProfiles.TabIndex = 18;

            // btnDeleteGame
            this.btnDeleteGame.Location = new System.Drawing.Point(338, 181);
            this.btnDeleteGame.Name = "btnDeleteGame";
            this.btnDeleteGame.Size = new System.Drawing.Size(75, 23);
            this.btnDeleteGame.TabIndex = 19;
            this.btnDeleteGame.Text = "Delete";
            this.btnDeleteGame.UseVisualStyleBackColor = true;
            this.btnDeleteGame.Click += new System.EventHandler(this.btnDeleteGame_Click);

            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 285);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(40, 13);
            this.lblStatus.TabIndex = 8;
            this.lblStatus.Text = "Status:";

            // notifyIcon
            this.notifyIcon.Text = "AutoRes";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseClick);

            // MainForm
            this.ClientSize = new System.Drawing.Size(420, 310);
            this.Controls.Add(this.btnDeleteGame);
            this.Controls.Add(this.lbGameProfiles);
            this.Controls.Add(this.lblGameProfiles);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblNormal);
            this.Controls.Add(this.chkStartMinimized);
            this.Controls.Add(this.chkAutostart);
            this.Controls.Add(this.btnToggleDark);
            this.Controls.Add(this.btnSaveGame);
            this.Controls.Add(this.btnSetNormal);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.cbProcesses);
            this.Controls.Add(this.txtHeight);
            this.Controls.Add(this.lblHeight);
            this.Controls.Add(this.txtWidth);
            this.Controls.Add(this.lblWidth);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AutoRes";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ComboBox cbProcesses;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnSetNormal;
        private System.Windows.Forms.Button btnSaveGame;
        private System.Windows.Forms.Button btnToggleDark;
        private System.Windows.Forms.CheckBox chkAutostart;
        private System.Windows.Forms.CheckBox chkStartMinimized;
        private System.Windows.Forms.Label lblNormal;
        private System.Windows.Forms.Label lblGameProfiles;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblWidth;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.ListBox lbGameProfiles;
        private System.Windows.Forms.Button btnDeleteGame;
        private System.Windows.Forms.NotifyIcon notifyIcon;
    }
}
