using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AutoRes.Ui
{
    public partial class MainForm : Form
    {
        private Timer _processTimer;
        private bool _isGameActive = false;
        private AppSettings _settings;

        public MainForm()
        {
            InitializeComponent();
            this.Icon = File.Exists(Program.IconPath) ? new Icon(Program.IconPath) : SystemIcons.Application;
            this.notifyIcon.Icon = this.Icon;
            _processTimer = new Timer { Interval = 1000 };
            _processTimer.Tick += ProcessTimer_Tick;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                _settings = AppSettings.Load();
                chkAutostart.Checked = _settings.Autostart;
                ApplyDarkMode(_settings.DarkMode);
                if (_settings.NormalResolution != null)
                {
                    lblNormal.Text = $"Normal: {_settings.NormalResolution.Width}x{_settings.NormalResolution.Height}";
                }
                RefreshGameProfiles();
                RefreshProcesses();
                lblStatus.Text = $"Monitoring {_settings.GameProfiles.Count} game(s)";
                _processTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading settings: " + ex.Message);
                lblStatus.Text = "Error: " + ex.Message;
            }
        }

        private void RefreshGameProfiles()
        {
            lbGameProfiles.Items.Clear();
            foreach (var profile in _settings.GameProfiles)
            {
                lbGameProfiles.Items.Add($"{profile.GameName} ({profile.GameResolution.Width}x{profile.GameResolution.Height})");
            }
        }

        private static readonly string[] SystemProcesses = new[] 
        { 
            "svchost", "explorer", "dwm", "lsass", "services", "winlogon", "csrss", "wininit",
            "conhost", "taskhostw", "SearchIndexer", "System", "Registry", "smss", "SearchProtocolHost",
            "SettingSyncHost", "WmiPrvSE", "skydrive", "OneDrive", "TeamViewer", "CCleaner", "avast",
            "avgnt", "Avira", "BitDefender", "McAfee", "Norton", "kaspersky", "nod32",
            "WindowsUpdates", "TiWorker", "ntoskrnl", "Idle", "chrome", "edge", "firefox", "iexplore"
        };

        private bool IsSystemProcess(string processName)
        {
            return SystemProcesses.Any(sp => processName.Equals(sp, StringComparison.OrdinalIgnoreCase));
        }

        private void RefreshProcesses()
        {
            var procList = new System.Collections.Generic.List<string>();
            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    var procName = p.ProcessName;
                    if (!IsSystemProcess(procName))
                    {
                        var pid = p.Id;
                        procList.Add($"{procName} ({pid})");
                    }
                }
                catch { /* ignore processes we can't access */ }
            }
            var procs = procList.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(n => n).ToArray();
            cbProcesses.Items.Clear();
            cbProcesses.Items.AddRange(procs);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshProcesses();
        }

        private void btnSetNormal_Click(object sender, EventArgs e)
        {
            try
            {
                int width = 0, height = 0;
                bool hasTxtInput = !string.IsNullOrEmpty(txtWidth.Text) && !string.IsNullOrEmpty(txtHeight.Text);

                if (hasTxtInput && int.TryParse(txtWidth.Text, out width) && int.TryParse(txtHeight.Text, out height))
                {
                    _settings.NormalResolution = new ResolutionData { Width = width, Height = height };
                    lblNormal.Text = $"Normal: {width}x{height}";
                }
                else
                {
                    var sz = ResolutionHelper.GetCurrentResolution();
                    _settings.NormalResolution = new ResolutionData(sz);
                    lblNormal.Text = $"Normal: {sz.Width}x{sz.Height}";
                }
                _settings.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting normal resolution: " + ex.Message);
            }
        }

        private void btnSaveGame_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(cbProcesses.Text))
                {
                    MessageBox.Show("Please select a process first");
                    return;
                }

                int width = 0, height = 0;
                bool hasTxtInput = !string.IsNullOrEmpty(txtWidth.Text) && !string.IsNullOrEmpty(txtHeight.Text);

                if (!hasTxtInput || !int.TryParse(txtWidth.Text, out width) || !int.TryParse(txtHeight.Text, out height))
                {
                    var sz = ResolutionHelper.GetCurrentResolution();
                    width = sz.Width;
                    height = sz.Height;
                }

                var selectedText = cbProcesses.Text;
                int pidStart = selectedText.LastIndexOf('(');
                string processName = pidStart > 0 ? selectedText.Substring(0, pidStart).Trim() : selectedText;

                var gameRes = new ResolutionData { Width = width, Height = height };
                var gameName = $"{processName} ({width}x{height})";
                var profile = new GameProfile(gameName, processName, gameRes);

                // Remove if already exists
                var existing = _settings.GameProfiles.FirstOrDefault(p => p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                    _settings.GameProfiles.Remove(existing);

                _settings.GameProfiles.Add(profile);
                _settings.Save();
                RefreshGameProfiles();
                MessageBox.Show($"Game profile saved: {gameName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving game profile: " + ex.Message);
            }
        }

        private void btnDeleteGame_Click(object sender, EventArgs e)
        {
            try
            {
                if (lbGameProfiles.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select a game profile to delete");
                    return;
                }

                _settings.GameProfiles.RemoveAt(lbGameProfiles.SelectedIndex);
                _settings.Save();
                RefreshGameProfiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting game profile: " + ex.Message);
            }
        }

        private void btnToggleDark_Click(object sender, EventArgs e)
        {
            _settings.DarkMode = !_settings.DarkMode;
            ApplyDarkMode(_settings.DarkMode);
            _settings.Save();
        }

        private void ApplyDarkMode(bool dark)
        {
            if (dark)
            {
                this.BackColor = Color.FromArgb(45, 45, 48);
                foreach (Control c in this.Controls)
                {
                    c.ForeColor = Color.White;
                    if (c is Button btn)
                    {
                        btn.BackColor = Color.FromArgb(63, 63, 70);
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
                    }
                    else if (c is TextBox)
                    {
                        c.BackColor = Color.FromArgb(45, 45, 48);
                        c.ForeColor = Color.White;
                    }
                    else if (c is ListBox)
                    {
                        c.BackColor = Color.FromArgb(45, 45, 48);
                        c.ForeColor = Color.White;
                    }
                    else if (c is ComboBox)
                    {
                        c.BackColor = Color.FromArgb(45, 45, 48);
                        c.ForeColor = Color.White;
                    }
                    else if (c is Label)
                    {
                        c.BackColor = Color.FromArgb(45, 45, 48);
                        c.ForeColor = Color.White;
                    }
                    else if (c is CheckBox chk)
                    {
                        chk.BackColor = Color.FromArgb(45, 45, 48);
                        chk.ForeColor = Color.White;
                    }
                }
            }
            else
            {
                this.BackColor = SystemColors.Control;
                foreach (Control c in this.Controls)
                {
                    c.ForeColor = SystemColors.ControlText;
                    if (c is Button btn)
                    {
                        btn.BackColor = SystemColors.Control;
                        btn.FlatStyle = FlatStyle.Standard;
                    }
                    else if (c is TextBox)
                    {
                        c.BackColor = SystemColors.Window;
                        c.ForeColor = SystemColors.WindowText;
                    }
                    else if (c is ListBox)
                    {
                        c.BackColor = SystemColors.Window;
                        c.ForeColor = SystemColors.WindowText;
                    }
                    else if (c is ComboBox)
                    {
                        c.BackColor = SystemColors.Window;
                        c.ForeColor = SystemColors.WindowText;
                    }
                    else if (c is Label)
                    {
                        c.BackColor = SystemColors.Control;
                        c.ForeColor = SystemColors.ControlText;
                    }
                    else if (c is CheckBox chk)
                    {
                        chk.BackColor = SystemColors.Control;
                        chk.ForeColor = SystemColors.ControlText;
                    }
                }
            }
        }

        private void chkAutostart_CheckedChanged(object sender, EventArgs e)
        {
            _settings.Autostart = chkAutostart.Checked;
            try
            {
                AutostartHelper.SetAutostart("AutoRes", _settings.Autostart);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set autostart: " + ex.Message);
                chkAutostart.Checked = !chkAutostart.Checked;
            }
            _settings.Save();
        }

        private void cbProcesses_SelectedIndexChanged(object sender, EventArgs e)
        {
            // No longer storing single selected process
        }

        private void ProcessTimer_Tick(object sender, EventArgs e)
        {
            if (_settings.GameProfiles.Count == 0) 
            {
                lblStatus.Text = "No games configured";
                return;
            }

            // Monitor all game profiles
            bool anyGameRunning = false;
            string runningGame = "";

            foreach (var profile in _settings.GameProfiles)
            {
                try
                {
                    var processes = Process.GetProcessesByName(profile.ProcessName);
                    var running = processes.Length > 0;

                    if (running)
                    {
                        anyGameRunning = true;
                        runningGame = profile.GameName;

                        if (!_isGameActive && profile.GameResolution != null)
                        {
                            // A game started, switch to game resolution
                            lblStatus.Text = $"Launching game: {profile.GameName}, changing to {profile.GameResolution.Width}x{profile.GameResolution.Height}...";
                            this.Refresh();
                            System.Threading.Thread.Sleep(100); // Give UI time to update

                            bool success = ResolutionHelper.SetResolution(profile.GameResolution.Width, profile.GameResolution.Height);
                            if (success)
                            {
                                _isGameActive = true;
                                lblStatus.Text = $"Active: {profile.GameName} ({profile.GameResolution.Width}x{profile.GameResolution.Height})";
                            }
                            else
                            {
                                lblStatus.Text = $"Failed to change to {profile.GameResolution.Width}x{profile.GameResolution.Height}. Check if resolution is supported.";
                            }
                            return; // Only change once
                        }
                        else if (_isGameActive)
                        {
                            lblStatus.Text = $"Active: {runningGame}";
                        }
                    }
                }
                catch (Exception ex) 
                { 
                    lblStatus.Text = $"Error checking process: {ex.Message}";
                }
            }

            // If no game is running but was active, restore normal resolution
            if (!anyGameRunning && _isGameActive)
            {
                if (_settings.NormalResolution != null)
                {
                    lblStatus.Text = "Game closed, restoring normal resolution...";
                    this.Refresh();
                    System.Threading.Thread.Sleep(100);

                    ResolutionHelper.SetResolution(_settings.NormalResolution.Width, _settings.NormalResolution.Height);
                    _isGameActive = false;
                    lblStatus.Text = $"Normal: {_settings.NormalResolution.Width}x{_settings.NormalResolution.Height}";
                }
            }
            else if (!anyGameRunning && !_isGameActive)
            {
                lblStatus.Text = $"Monitoring {_settings.GameProfiles.Count} game(s)";
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // restore if active
            if (_isGameActive && _settings.NormalResolution != null)
            {
                ResolutionHelper.SetResolution(_settings.NormalResolution.Width, _settings.NormalResolution.Height);
            }
            base.OnFormClosing(e);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.ShowInTaskbar = false;
            }
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (this.Visible)
                {
                    this.Hide();
                    this.ShowInTaskbar = false;
                }
                else
                {
                    this.Show();
                    this.ShowInTaskbar = true;
                    this.WindowState = FormWindowState.Normal;
                    this.Activate();
                }
            }
        }
    }
}
