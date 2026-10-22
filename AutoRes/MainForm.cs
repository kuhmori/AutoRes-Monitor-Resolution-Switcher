using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AutoRes
{
    public sealed class MainForm : Form
    {
        private const int PollIntervalMs = 1500;

        // Processes that are never games - hidden from the program picker.
        private static readonly HashSet<string> IgnoredProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "svchost", "explorer", "dwm", "lsass", "services", "winlogon", "csrss", "wininit", "conhost",
            "taskhostw", "SearchIndexer", "SearchHost", "SearchProtocolHost", "SearchFilterHost", "System",
            "Registry", "smss", "Idle", "fontdrvhost", "sihost", "ctfmon", "RuntimeBroker", "dllhost",
            "SettingSyncHost", "WmiPrvSE", "spoolsv", "audiodg", "TiWorker", "TrustedInstaller", "MsMpEng",
            "NisSrv", "SecurityHealthService", "SecurityHealthSystray", "StartMenuExperienceHost",
            "ShellExperienceHost", "TextInputHost", "ApplicationFrameHost", "SystemSettings", "LockApp",
            "smartscreen", "backgroundTaskHost", "WUDFHost", "Memory Compression", "AutoRes",
            "chrome", "msedge", "firefox", "iexplore", "msedgewebview2"
        };

        private readonly AppSettings _settings;
        private readonly float _scale;
        private Theme _theme = Theme.Light;

        private readonly Timer _pollTimer;
        private readonly Timer _flashTimer;
        private NotifyIcon _tray;
        private ContextMenuStrip _trayMenu;
        private ToolStripMenuItem _trayStatusItem;

        private bool _allowVisible;
        private bool _exiting;
        private bool _loadingValues;

        // Game state
        private GameProfile _activeProfile;
        private Size _activeResolution;
        private Size _resolutionBeforeGame;
        private string _failedKey;
        private StatusKind _stateKind = StatusKind.Idle;
        private string _stateText = "";

        // UI
        private Label _title;
        private StatusPill _status;
        private Card _desktopCard, _gameCard, _profilesCard, _settingsCard;
        private ThemedComboBox _cbNormal, _cbProcess, _cbGameRes;
        private FlatButton _btnUseCurrent, _btnSaveNormal, _btnRefresh, _btnBrowse, _btnSaveProfile, _btnRemove;
        private Label _lblProgram, _lblResolution;
        private ProfileList _profileList;
        private ToggleSwitch _tgAutostart, _tgStartMinimized, _tgCloseToTray, _tgDarkMode;
        private ToolTip _toolTip;

        public MainForm()
        {
            _settings = AppSettings.Load();
            _allowVisible = !_settings.StartMinimized;

            using (var g = Graphics.FromHwnd(IntPtr.Zero))
                _scale = g.DpiX / 96f;

            AutoScaleMode = AutoScaleMode.None;
            Font = new Font("Segoe UI", 9.75f);
            Text = "AutoRes";
            Icon = LoadAppIcon(new Size(32, 32));
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(S(480), S(640));

            BuildUi();
            BuildTray();
            LoadValues();
            ApplyTheme();

            if (_settings.Autostart)
            {
                // Keep the registry entry pointing at this exe, in case it was moved.
                try { AutostartHelper.SetAutostart(true); } catch { /* not critical */ }
            }

            _flashTimer = new Timer { Interval = 3500 };
            _flashTimer.Tick += (s, e) => { _flashTimer.Stop(); RenderStatus(); };

            _pollTimer = new Timer { Interval = PollIntervalMs };
            _pollTimer.Tick += (s, e) => CheckGames();
            _pollTimer.Start();
            CheckGames();
        }

        #region Layout

        private int S(int value) => (int)Math.Round(value * _scale);
        private Rectangle R(int x, int y, int w, int h) => new Rectangle(S(x), S(y), S(w), S(h));

        private void BuildUi()
        {
            SuspendLayout();
            _toolTip = new ToolTip();

            _title = new Label
            {
                Text = "AutoRes",
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 15f),
                Location = new Point(S(18), S(14)),
            };

            _status = new StatusPill { Bounds = R(150, 20, 310, 30), Font = new Font("Segoe UI", 9f) };

            // --- Desktop resolution -------------------------------------------------------------
            _desktopCard = new Card { Title = "Desktop resolution", Bounds = R(20, 64, 440, 86) };
            _cbNormal = new ThemedComboBox { CueText = "e.g. 1920 × 1080" };
            Place(_cbNormal, 16, 42, 200);
            _btnUseCurrent = new FlatButton { Text = "Use current", Bounds = R(224, 42, 96, 30) };
            _btnSaveNormal = new FlatButton { Text = "Save", Kind = ButtonKind.Primary, Bounds = R(328, 42, 96, 30) };
            _desktopCard.Controls.AddRange(new Control[] { _cbNormal, _btnUseCurrent, _btnSaveNormal });
            _toolTip.SetToolTip(_btnUseCurrent, "Fill in the resolution you're using right now");
            _toolTip.SetToolTip(_desktopCard, "AutoRes switches back to this resolution when a game closes");

            // --- Add / update a game ------------------------------------------------------------
            _gameCard = new Card { Title = "Add or update a game", Bounds = R(20, 162, 440, 124) };
            _lblProgram = MakeFieldLabel("Program", 42);
            _cbProcess = new ThemedComboBox { CueText = "Process name, e.g. overwatch" };
            Place(_cbProcess, 92, 42, 210);
            _btnRefresh = new FlatButton { Text = "↻", Font = new Font("Segoe UI Symbol", 11f), Bounds = R(308, 42, 34, 30) };
            _btnBrowse = new FlatButton { Text = "Browse…", Bounds = R(348, 42, 76, 30) };
            _lblResolution = MakeFieldLabel("Resolution", 80);
            _cbGameRes = new ThemedComboBox { CueText = "e.g. 1728 × 1080" };
            Place(_cbGameRes, 92, 80, 210);
            _btnSaveProfile = new FlatButton { Text = "Save game", Kind = ButtonKind.Primary, Bounds = R(308, 80, 116, 30) };
            _gameCard.Controls.AddRange(new Control[]
                { _lblProgram, _cbProcess, _btnRefresh, _btnBrowse, _lblResolution, _cbGameRes, _btnSaveProfile });
            _toolTip.SetToolTip(_btnRefresh, "Reload the list of running programs");
            _toolTip.SetToolTip(_btnBrowse, "Pick the game's .exe file");

            // --- Profiles -----------------------------------------------------------------------
            _profilesCard = new Card { Title = "Game profiles", Bounds = R(20, 298, 440, 214) };
            _btnRemove = new FlatButton { Text = "Remove", Kind = ButtonKind.Danger, Bounds = R(334, 8, 90, 28), Enabled = false };
            _profileList = new ProfileList
            {
                Bounds = R(16, 46, 408, 152),
                ItemHeight = S(38),
                EmptyText = "No games yet — add one above.",
            };
            _profilesCard.FramedControl = _profileList;
            _profilesCard.Controls.AddRange(new Control[] { _btnRemove, _profileList });

            // --- Settings -----------------------------------------------------------------------
            _settingsCard = new Card { Title = "Settings", Bounds = R(20, 524, 440, 100) };
            _tgAutostart = new ToggleSwitch { Text = "Start with Windows", Bounds = R(16, 40, 192, 26) };
            _tgStartMinimized = new ToggleSwitch { Text = "Start minimized", Bounds = R(232, 40, 192, 26) };
            _tgCloseToTray = new ToggleSwitch { Text = "Close to tray", Bounds = R(16, 68, 192, 26) };
            _tgDarkMode = new ToggleSwitch { Text = "Dark mode", Bounds = R(232, 68, 192, 26) };
            _settingsCard.Controls.AddRange(new Control[] { _tgAutostart, _tgStartMinimized, _tgCloseToTray, _tgDarkMode });
            _toolTip.SetToolTip(_tgStartMinimized, "Start hidden in the tray");
            _toolTip.SetToolTip(_tgCloseToTray, "The X button hides AutoRes to the tray instead of quitting.\nRight-click the tray icon to exit.");

            Controls.AddRange(new Control[] { _title, _status, _desktopCard, _gameCard, _profilesCard, _settingsCard });

            // Events
            _btnUseCurrent.Click += (s, e) => _cbNormal.Text = ResolutionHelper.Format(ResolutionHelper.GetCurrentResolution());
            _btnSaveNormal.Click += (s, e) => SaveNormalResolution();
            _btnRefresh.Click += (s, e) => { RefreshProcessList(); Flash(StatusKind.Info, "Program list refreshed"); };
            _btnBrowse.Click += (s, e) => BrowseForGame();
            _btnSaveProfile.Click += (s, e) => SaveProfile();
            _btnRemove.Click += (s, e) => RemoveSelectedProfile();
            _profileList.SelectedIndexChanged += (s, e) => OnProfileSelected();
            _profileList.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) RemoveSelectedProfile(); };
            _cbProcess.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; SaveProfile(); } };
            _cbGameRes.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; SaveProfile(); } };
            _cbNormal.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; SaveNormalResolution(); } };

            _tgAutostart.CheckedChanged += (s, e) => OnAutostartChanged();
            _tgStartMinimized.CheckedChanged += (s, e) => UpdateSetting(() => _settings.StartMinimized = _tgStartMinimized.Checked);
            _tgCloseToTray.CheckedChanged += (s, e) => UpdateSetting(() => _settings.CloseToTray = _tgCloseToTray.Checked);
            _tgDarkMode.CheckedChanged += (s, e) =>
            {
                UpdateSetting(() => _settings.DarkMode = _tgDarkMode.Checked);
                if (!_loadingValues) ApplyTheme();
            };

            ResumeLayout(false);
            PerformLayout();
        }

        private Label MakeFieldLabel(string text, int y)
        {
            return new Label { Text = text, AutoSize = false, Bounds = R(16, y, 72, 30), TextAlign = ContentAlignment.MiddleLeft };
        }

        /// <summary>Positions a combo box so it's vertically centred on a 30px button row.</summary>
        private void Place(ComboBox combo, int x, int rowY, int width)
        {
            combo.Font = Font;
            combo.Left = S(x);
            combo.Width = S(width);
            combo.Top = S(rowY) + (S(30) - combo.Height) / 2;
        }

        private void BuildTray()
        {
            _trayMenu = new ContextMenuStrip { ShowImageMargin = false, Font = new Font("Segoe UI", 9.75f) };

            var openItem = new ToolStripMenuItem("Open AutoRes", null, (s, e) => ShowFromTray());
            openItem.Font = new Font(_trayMenu.Font, FontStyle.Bold);
            _trayStatusItem = new ToolStripMenuItem("Starting…") { Enabled = false };
            var exitItem = new ToolStripMenuItem("Exit", null, (s, e) => ExitApplication());

            _trayMenu.Items.AddRange(new ToolStripItem[]
            {
                openItem, new ToolStripSeparator(), _trayStatusItem, new ToolStripSeparator(), exitItem
            });
            foreach (ToolStripItem item in _trayMenu.Items)
                if (item is ToolStripMenuItem) item.Padding = new Padding(S(4), S(3), S(4), S(3));

            _tray = new NotifyIcon
            {
                Icon = LoadAppIcon(SystemInformation.SmallIconSize),
                Text = "AutoRes",
                ContextMenuStrip = _trayMenu,
                Visible = true,
            };
            _tray.MouseClick += (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                if (Visible && WindowState != FormWindowState.Minimized) HideToTray();
                else ShowFromTray();
            };
            _tray.BalloonTipClicked += (s, e) => ShowFromTray();
        }

        private static Icon LoadAppIcon(Size size)
        {
            try
            {
                using (var stream = typeof(MainForm).Assembly.GetManifestResourceStream("AutoRes.app.ico"))
                {
                    if (stream != null) return new Icon(stream, size);
                }
            }
            catch { /* fall through */ }
            return SystemIcons.Application;
        }

        #endregion

        #region Theme

        private void ApplyTheme()
        {
            _theme = _settings.DarkMode ? Theme.Dark : Theme.Light;
            SuspendLayout();

            BackColor = _theme.Background;
            ForeColor = _theme.Text;
            _title.BackColor = _theme.Background;
            _title.ForeColor = _theme.Text;
            _status.ApplyTheme(_theme);

            foreach (var card in new[] { _desktopCard, _gameCard, _profilesCard, _settingsCard })
            {
                card.ApplyTheme(_theme);
                foreach (Control c in card.Controls) ApplyTheme(c);
            }

            _trayMenu.Renderer = new MenuRenderer(_theme);

            ResumeLayout();
            if (IsHandleCreated) UpdateTitleBar();
            Invalidate(true);
        }

        private void ApplyTheme(Control c)
        {
            if (c is FlatButton button) button.ApplyTheme(_theme);
            else if (c is ToggleSwitch toggle) toggle.ApplyTheme(_theme);
            else if (c is ThemedComboBox combo) combo.ApplyTheme(_theme);
            else if (c is ProfileList list) list.ApplyTheme(_theme);
            else if (c is Label label)
            {
                label.BackColor = _theme.Surface;
                label.ForeColor = _theme.MutedText;
            }
        }

        private void UpdateTitleBar()
        {
            NativeMethods.SetDarkTitleBar(Handle, _theme.IsDark);
            if (Visible)
            {
                // Windows only repaints the caption after a non-client change; nudge it.
                Width += 1;
                Width -= 1;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            NativeMethods.SetDarkTitleBar(Handle, _theme.IsDark);
        }

        #endregion

        #region Values & actions

        private void LoadValues()
        {
            _loadingValues = true;
            try
            {
                var modes = ResolutionHelper.GetSupportedResolutions().Select(s => (object)ResolutionHelper.Format(s)).ToArray();
                _cbNormal.Items.AddRange(modes);
                _cbGameRes.Items.AddRange(modes);

                var current = ResolutionHelper.GetCurrentResolution();
                _cbNormal.Text = _settings.NormalResolution != null
                    ? _settings.NormalResolution.ToString()
                    : ResolutionHelper.Format(current);
                _cbGameRes.Text = ResolutionHelper.Format(current);
                UpdateNormalNote();

                _tgAutostart.Checked = _settings.Autostart;
                _tgStartMinimized.Checked = _settings.StartMinimized;
                _tgCloseToTray.Checked = _settings.CloseToTray;
                _tgDarkMode.Checked = _settings.DarkMode;

                RefreshProcessList();
                RefreshProfileList();
            }
            finally
            {
                _loadingValues = false;
            }
        }

        private void UpdateNormalNote()
        {
            _desktopCard.Note = _settings.NormalResolution != null
                ? "Saved: " + _settings.NormalResolution
                : "Not saved yet";
        }

        private void UpdateSetting(Action change)
        {
            if (_loadingValues) return;
            change();
            TrySave();
        }

        private void TrySave()
        {
            try { _settings.Save(); }
            catch (Exception ex) { Flash(StatusKind.Warning, "Couldn't save settings: " + ex.Message); }
        }

        private void OnAutostartChanged()
        {
            if (_loadingValues) return;
            try
            {
                AutostartHelper.SetAutostart(_tgAutostart.Checked);
                _settings.Autostart = _tgAutostart.Checked;
                TrySave();
            }
            catch (Exception ex)
            {
                _loadingValues = true;
                _tgAutostart.Checked = !_tgAutostart.Checked;
                _loadingValues = false;
                Flash(StatusKind.Warning, "Couldn't change autostart: " + ex.Message);
            }
        }

        private void SaveNormalResolution()
        {
            Size size;
            if (!ResolutionHelper.TryParse(_cbNormal.Text, out size))
            {
                Flash(StatusKind.Warning, "Enter a resolution like 1920 × 1080");
                _cbNormal.Focus();
                return;
            }
            _settings.NormalResolution = new ResolutionData(size);
            _cbNormal.Text = ResolutionHelper.Format(size);
            TrySave();
            UpdateNormalNote();
            Flash(StatusKind.Info, "Desktop resolution saved");
        }

        private static string CleanProcessName(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            var name = text.Trim().Trim('"');
            name = Path.GetFileName(name);
            if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - 4);
            return name.Trim();
        }

        private void SaveProfile()
        {
            var processName = CleanProcessName(_cbProcess.Text);
            if (processName.Length == 0)
            {
                Flash(StatusKind.Warning, "Pick or type the game's process name first");
                _cbProcess.Focus();
                return;
            }

            Size size;
            if (!ResolutionHelper.TryParse(_cbGameRes.Text, out size))
            {
                Flash(StatusKind.Warning, "Enter a resolution like 1728 × 1080");
                _cbGameRes.Focus();
                return;
            }

            var existing = _settings.GameProfiles.FirstOrDefault(p =>
                string.Equals(p.ProcessName, processName, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.GameResolution = new ResolutionData(size);
            }
            else
            {
                existing = new GameProfile(processName, new ResolutionData(size));
                _settings.GameProfiles.Add(existing);
            }

            TrySave();
            RefreshProfileList(existing);
            _cbProcess.Text = processName;
            _cbGameRes.Text = ResolutionHelper.Format(size);
            Flash(StatusKind.Info, $"Saved {processName} at {ResolutionHelper.Format(size)}");
            CheckGames();
        }

        private void RemoveSelectedProfile()
        {
            var profile = _profileList.SelectedItem as GameProfile;
            if (profile == null) return;
            _settings.GameProfiles.Remove(profile);
            TrySave();
            RefreshProfileList();
            Flash(StatusKind.Info, $"Removed {profile.ProcessName}");
            CheckGames();
        }

        private void OnProfileSelected()
        {
            var profile = _profileList.SelectedItem as GameProfile;
            _btnRemove.Enabled = profile != null;
            if (profile == null) return;
            _cbProcess.Text = profile.ProcessName;
            _cbGameRes.Text = profile.GameResolution.ToString();
        }

        private void BrowseForGame()
        {
            using (var dialog = new OpenFileDialog
            {
                Title = "Select the game's .exe",
                Filter = "Programs (*.exe)|*.exe",
                CheckFileExists = true,
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _cbProcess.Text = CleanProcessName(dialog.FileName);
                    _cbGameRes.Focus();
                }
            }
        }

        private void RefreshProcessList()
        {
            var text = _cbProcess.Text;
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    if (p.SessionId != 0 && !IgnoredProcesses.Contains(p.ProcessName))
                        names.Add(p.ProcessName);
                }
                catch { /* process exited or access denied */ }
                finally { p.Dispose(); }
            }

            _cbProcess.BeginUpdate();
            _cbProcess.Items.Clear();
            _cbProcess.Items.AddRange(names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).Cast<object>().ToArray());
            _cbProcess.EndUpdate();
            _cbProcess.Text = text;
        }

        private void RefreshProfileList(GameProfile select = null)
        {
            _profileList.BeginUpdate();
            _profileList.Items.Clear();
            foreach (var profile in _settings.GameProfiles) _profileList.Items.Add(profile);
            _profileList.EndUpdate();

            if (select != null) _profileList.SelectedItem = select;
            _btnRemove.Enabled = _profileList.SelectedItem != null;
            _profileList.Invalidate();
        }

        #endregion

        #region Game monitoring

        private static HashSet<string> GetRunningProcessNames()
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in Process.GetProcesses())
            {
                try { names.Add(p.ProcessName); }
                catch { /* exited */ }
                finally { p.Dispose(); }
            }
            return names;
        }

        private void CheckGames()
        {
            var profiles = _settings.GameProfiles;
            GameProfile running = null;
            if (profiles.Count > 0)
            {
                var names = GetRunningProcessNames();
                running = profiles.FirstOrDefault(p => names.Contains(p.ProcessName));
            }

            if (running != null)
            {
                var target = running.GameResolution.ToSize();
                bool alreadyApplied = _activeProfile != null
                    && string.Equals(_activeProfile.ProcessName, running.ProcessName, StringComparison.OrdinalIgnoreCase)
                    && _activeResolution == target;

                if (!alreadyApplied)
                {
                    string key = running.ProcessName + "|" + target;
                    if (key == _failedKey) return; // already failed for this game/resolution - don't retry every tick

                    if (_activeProfile == null)
                        _resolutionBeforeGame = ResolutionHelper.GetCurrentResolution();

                    if (ResolutionHelper.SetResolution(target.Width, target.Height, persist: false))
                    {
                        _activeProfile = running;
                        _activeResolution = target;
                        _failedKey = null;
                    }
                    else
                    {
                        _failedKey = key;
                        SetState(StatusKind.Warning, $"{ResolutionHelper.Format(target)} isn't supported by this display");
                        return;
                    }
                }

                _profileList.RunningProcess = running.ProcessName;
                SetState(StatusKind.Active, $"{running.ProcessName} running · {ResolutionHelper.Format(target)}");
                return;
            }

            _failedKey = null;
            if (_activeProfile != null) RestoreDesktopResolution();
            _profileList.RunningProcess = null;

            if (profiles.Count == 0)
                SetState(StatusKind.Idle, "Add a game to get started");
            else
                SetState(StatusKind.Watching, profiles.Count == 1 ? "Watching 1 game" : $"Watching {profiles.Count} games");
        }

        private void RestoreDesktopResolution()
        {
            var target = _settings.NormalResolution?.ToSize() ?? _resolutionBeforeGame;
            if (target.Width > 0 && target.Height > 0)
                ResolutionHelper.SetResolution(target.Width, target.Height, persist: true);
            _activeProfile = null;
            _activeResolution = Size.Empty;
        }

        private void SetState(StatusKind kind, string text)
        {
            if (kind == _stateKind && text == _stateText) return;
            _stateKind = kind;
            _stateText = text;
            RenderStatus();
        }

        /// <summary>Shows a short-lived message in the status pill, then falls back to the current state.</summary>
        private void Flash(StatusKind kind, string text)
        {
            _status.SetStatus(kind, text);
            _flashTimer.Stop();
            _flashTimer.Start();
        }

        private void RenderStatus()
        {
            if (!_flashTimer.Enabled) _status.SetStatus(_stateKind, _stateText);

            _trayStatusItem.Text = _stateText;
            var tip = "AutoRes – " + _stateText;
            _tray.Text = tip.Length > 63 ? tip.Substring(0, 62) + "…" : tip;
        }

        #endregion

        #region Window & tray behaviour

        protected override void SetVisibleCore(bool value)
        {
            // When starting minimized, create the window handle (so the tray, timers and
            // single-instance message all work) but don't show the window.
            if (!_allowVisible)
            {
                value = false;
                if (!IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        private void ShowFromTray()
        {
            _allowVisible = true;
            Show();
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
            Activate();
            BringToFront();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Don't drop the cursor into the first box (which would also select its text).
            ActiveControl = null;
            ClearComboSelections();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            ClearComboSelections();
        }

        private void ClearComboSelections()
        {
            _cbNormal.ClearSelection();
            _cbProcess.ClearSelection();
            _cbGameRes.ClearSelection();
        }

        private void HideToTray()
        {
            Hide();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized) HideToTray();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_SHOW_AUTORES)
            {
                ShowFromTray();
                return;
            }
            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_exiting && e.CloseReason == CloseReason.UserClosing && _settings.CloseToTray)
            {
                e.Cancel = true;
                HideToTray();
                if (!_settings.TrayHintShown)
                {
                    _settings.TrayHintShown = true;
                    TrySave();
                    _tray.ShowBalloonTip(4000, "AutoRes is still running",
                        "It keeps watching for your games. Right-click the tray icon to exit.", ToolTipIcon.Info);
                }
                return;
            }

            _pollTimer.Stop();
            if (_activeProfile != null) RestoreDesktopResolution();
            _tray.Visible = false;
            base.OnFormClosing(e);
        }

        private void ExitApplication()
        {
            _exiting = true;
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pollTimer?.Dispose();
                _flashTimer?.Dispose();
                _tray?.Dispose();
                _trayMenu?.Dispose();
                _toolTip?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
