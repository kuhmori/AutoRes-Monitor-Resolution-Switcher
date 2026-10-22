using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AutoRes
{
    internal enum ButtonKind { Secondary, Primary, Danger }

    /// <summary>Flat button that follows the current theme.</summary>
    internal sealed class FlatButton : Button
    {
        public ButtonKind Kind { get; set; }

        public FlatButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 1;
            UseVisualStyleBackColor = false;
            Cursor = Cursors.Hand;
        }

        protected override bool ShowFocusCues => false;

        public void ApplyTheme(Theme t)
        {
            switch (Kind)
            {
                case ButtonKind.Primary:
                    BackColor = t.Accent;
                    ForeColor = Color.White;
                    FlatAppearance.BorderColor = t.Accent;
                    FlatAppearance.MouseOverBackColor = t.AccentHover;
                    FlatAppearance.MouseDownBackColor = t.AccentHover;
                    break;
                case ButtonKind.Danger:
                    BackColor = t.Surface;
                    ForeColor = t.Danger;
                    FlatAppearance.BorderColor = t.Border;
                    FlatAppearance.MouseOverBackColor = t.Hover;
                    FlatAppearance.MouseDownBackColor = t.Border;
                    break;
                default:
                    BackColor = t.Surface;
                    ForeColor = t.Text;
                    FlatAppearance.BorderColor = t.Border;
                    FlatAppearance.MouseOverBackColor = t.Hover;
                    FlatAppearance.MouseDownBackColor = t.Border;
                    break;
            }
        }
    }

    /// <summary>Checkbox drawn as an on/off switch with its label on the left.</summary>
    internal sealed class ToggleSwitch : CheckBox
    {
        private Theme _theme = Theme.Light;

        public ToggleSwitch()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            AutoSize = false;
            Cursor = Cursors.Hand;
        }

        public void ApplyTheme(Theme t)
        {
            _theme = t;
            BackColor = t.Surface;
            ForeColor = t.Text;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);
            float s = g.DpiX / 96f;

            float trackW = 36 * s, trackH = 20 * s;
            var track = new RectangleF(Width - trackW - 2 * s, (Height - trackH) / 2f, trackW, trackH);

            var textRect = new Rectangle(0, 0, (int)(track.X - 8 * s), Height);
            TextRenderer.DrawText(g, Text, Font, textRect, Enabled ? ForeColor : _theme.MutedText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = Draw.RoundedRect(track, trackH / 2f))
            using (var brush = new SolidBrush(Checked ? _theme.Accent : _theme.ToggleOff))
            {
                g.FillPath(brush, path);
            }

            float pad = 3 * s;
            float knob = trackH - 2 * pad;
            float knobX = Checked ? track.Right - pad - knob : track.X + pad;
            using (var brush = new SolidBrush(Color.White))
            {
                g.FillEllipse(brush, knobX, track.Y + pad, knob, knob);
            }

            if (Focused && ShowFocusCues)
            {
                var focus = RectangleF.Inflate(track, 2 * s, 2 * s);
                using (var path = Draw.RoundedRect(focus, focus.Height / 2f))
                using (var pen = new Pen(_theme.Accent, 1.5f * s))
                {
                    g.DrawPath(pen, path);
                }
            }
        }
    }

    /// <summary>Rounded "card" panel with a title, an optional note on the right, and an optional framed child.</summary>
    internal sealed class Card : Panel
    {
        private static readonly Font TitleFont = new Font("Segoe UI Semibold", 10f);
        private Theme _theme = Theme.Light;
        private string _note;

        public string Title { get; set; }
        public Control FramedControl { get; set; }

        public string Note
        {
            get { return _note; }
            set { _note = value; Invalidate(); }
        }

        public Card()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        public void ApplyTheme(Theme t)
        {
            _theme = t;
            BackColor = t.Surface;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? _theme.Background);
            float s = g.DpiX / 96f;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using (var path = Draw.RoundedRect(rect, 10 * s))
            using (var fill = new SolidBrush(_theme.Surface))
            using (var pen = new Pen(_theme.Border))
            {
                g.FillPath(fill, path);
                g.DrawPath(pen, path);
            }
            g.SmoothingMode = SmoothingMode.None;

            var titleTop = (int)(12 * s);
            TextRenderer.DrawText(g, Title, TitleFont, new Point((int)(16 * s), titleTop), _theme.Text, TextFormatFlags.NoPrefix);

            if (!string.IsNullOrEmpty(_note))
            {
                var noteRect = new Rectangle(0, titleTop, Width - (int)(16 * s), TitleFont.Height);
                TextRenderer.DrawText(g, _note, Font, noteRect, _theme.MutedText,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }

            if (FramedControl != null)
            {
                var b = FramedControl.Bounds;
                using (var pen = new Pen(_theme.Border))
                {
                    g.DrawRectangle(pen, b.X - 1, b.Y - 1, b.Width + 1, b.Height + 1);
                }
            }
        }
    }

    internal enum StatusKind { Idle, Watching, Active, Warning, Info }

    /// <summary>Small pill with a coloured dot showing what AutoRes is doing. Right-aligned in its bounds.</summary>
    internal sealed class StatusPill : Control
    {
        private Theme _theme = Theme.Light;
        private StatusKind _kind = StatusKind.Idle;

        public StatusPill()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        public void ApplyTheme(Theme t)
        {
            _theme = t;
            BackColor = t.Background;
            Invalidate();
        }

        public void SetStatus(StatusKind kind, string text)
        {
            if (kind == _kind && text == Text) return;
            _kind = kind;
            Text = text;
            Invalidate();
        }

        private Color DotColor
        {
            get
            {
                switch (_kind)
                {
                    case StatusKind.Active: return _theme.Success;
                    case StatusKind.Warning: return _theme.Warning;
                    case StatusKind.Watching:
                    case StatusKind.Info: return _theme.Accent;
                    default: return _theme.MutedText;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);
            if (string.IsNullOrEmpty(Text)) return;
            float s = g.DpiX / 96f;

            const TextFormatFlags flags = TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
            int padLeft = (int)(26 * s), padRight = (int)(12 * s);
            int textW = TextRenderer.MeasureText(g, Text, Font, Size.Empty, flags).Width;
            int pillW = Math.Min(Width - 1, textW + padLeft + padRight);
            var pill = new RectangleF(Width - pillW - 1, 0.5f, pillW, Height - 1.5f);

            var dot = DotColor;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = Draw.RoundedRect(pill, pill.Height / 2f))
            using (var fill = new SolidBrush(Color.FromArgb(_theme.IsDark ? 38 : 24, dot)))
            using (var pen = new Pen(Color.FromArgb(_theme.IsDark ? 70 : 55, dot)))
            {
                g.FillPath(fill, path);
                g.DrawPath(pen, path);
            }

            float dotSize = 8 * s;
            using (var brush = new SolidBrush(dot))
            {
                g.FillEllipse(brush, pill.X + 11 * s, (Height - dotSize) / 2f, dotSize, dotSize);
            }

            var textRect = new Rectangle((int)pill.X + padLeft, 0, pillW - padLeft - padRight + 2, Height);
            TextRenderer.DrawText(g, Text, Font, textRect, _theme.Text,
                flags | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    /// <summary>Editable combo box with a placeholder and native dark-mode styling.</summary>
    internal sealed class ThemedComboBox : ComboBox
    {
        private bool _dark;
        private string _cue;

        public string CueText
        {
            get { return _cue; }
            set { _cue = value; if (IsHandleCreated) NativeMethods.SetCueBanner(Handle, value); }
        }

        public ThemedComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDown;
            MaxDropDownItems = 12;
        }

        public void ApplyTheme(Theme t)
        {
            _dark = t.IsDark;
            BackColor = t.Input;
            ForeColor = t.Text;
            if (IsHandleCreated) ApplyNativeTheme();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyNativeTheme();
            if (_cue != null) NativeMethods.SetCueBanner(Handle, _cue);
            ClearSelectionIfUnfocused();
        }

        // Windows highlights the whole text whenever it's set from code (e.g. picking a list item),
        // which looks like a stray selection when the box isn't being edited. Clear it.
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            ClearSelectionIfUnfocused();
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            ClearSelectionIfUnfocused();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            ClearSelectionIfUnfocused();
        }

        private void ClearSelectionIfUnfocused()
        {
            if (!IsHandleCreated || Focused) return;
            BeginInvoke((Action)(() =>
            {
                if (!IsDisposed && !Focused && DropDownStyle != ComboBoxStyle.DropDownList) SelectionLength = 0;
            }));
        }

        private void ApplyNativeTheme()
        {
            NativeMethods.SetControlTheme(Handle, _dark ? "DarkMode_CFD" : "CFD");
            Invalidate();
        }
    }

    /// <summary>Owner-drawn list of game profiles: name on the left, resolution on the right.</summary>
    internal sealed class ProfileList : ListBox
    {
        private const int WM_PAINT = 0x000F;
        private Theme _theme = Theme.Light;
        private string _runningProcess;

        public string EmptyText { get; set; } = "No games yet";

        /// <summary>Process name of the game currently running, shown with a green dot.</summary>
        public string RunningProcess
        {
            get { return _runningProcess; }
            set
            {
                if (string.Equals(_runningProcess, value, StringComparison.OrdinalIgnoreCase)) return;
                _runningProcess = value;
                Invalidate();
            }
        }

        public ProfileList()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            BorderStyle = BorderStyle.None;
            IntegralHeight = false;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        public void ApplyTheme(Theme t)
        {
            _theme = t;
            BackColor = t.Input;
            ForeColor = t.Text;
            if (IsHandleCreated) NativeMethods.SetControlTheme(Handle, t.IsDark ? "DarkMode_Explorer" : "Explorer");
            Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            NativeMethods.SetControlTheme(Handle, _theme.IsDark ? "DarkMode_Explorer" : "Explorer");
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count) return;
            var g = e.Graphics;
            var profile = Items[e.Index] as GameProfile;
            bool selected = (e.State & DrawItemState.Selected) != 0;
            float s = g.DpiX / 96f;
            int pad = (int)(12 * s);

            using (var back = new SolidBrush(selected ? _theme.Selection : _theme.Input))
                g.FillRectangle(back, e.Bounds);

            if (e.Index < Items.Count - 1)
            {
                using (var pen = new Pen(_theme.Border))
                    g.DrawLine(pen, e.Bounds.Left + pad, e.Bounds.Bottom - 1, e.Bounds.Right - pad, e.Bounds.Bottom - 1);
            }

            if (profile == null) return;
            bool running = string.Equals(profile.ProcessName, _runningProcess, StringComparison.OrdinalIgnoreCase);

            int x = e.Bounds.Left + pad;
            if (running)
            {
                float dot = 8 * s;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(_theme.Success))
                    g.FillEllipse(brush, x, e.Bounds.Top + (e.Bounds.Height - dot) / 2f, dot, dot);
                g.SmoothingMode = SmoothingMode.None;
                x += (int)(16 * s);
            }

            string res = profile.GameResolution?.ToString() ?? "";
            const TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
            int resW = TextRenderer.MeasureText(g, res, Font, Size.Empty, flags).Width;
            var resRect = new Rectangle(e.Bounds.Right - pad - resW, e.Bounds.Top, resW, e.Bounds.Height);
            var nameRect = new Rectangle(x, e.Bounds.Top, resRect.Left - x - pad, e.Bounds.Height);

            TextRenderer.DrawText(g, profile.ProcessName, Font, nameRect, _theme.Text, flags | TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g, res, Font, resRect, running ? _theme.Success : _theme.MutedText, flags);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_PAINT && Items.Count == 0 && !string.IsNullOrEmpty(EmptyText))
            {
                using (var g = CreateGraphics())
                {
                    TextRenderer.DrawText(g, EmptyText, Font, ClientRectangle, _theme.MutedText,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
                }
            }
        }
    }
}
