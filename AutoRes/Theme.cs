using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AutoRes
{
    internal sealed class Theme
    {
        public bool IsDark;
        public Color Background;
        public Color Surface;
        public Color Border;
        public Color Input;
        public Color Hover;
        public Color Selection;
        public Color Text;
        public Color MutedText;
        public Color Accent;
        public Color AccentHover;
        public Color Success;
        public Color Warning;
        public Color Danger;
        public Color ToggleOff;

        public static readonly Theme Light = new Theme
        {
            IsDark = false,
            Background = Color.FromArgb(242, 243, 246),
            Surface = Color.White,
            Border = Color.FromArgb(221, 224, 230),
            Input = Color.White,
            Hover = Color.FromArgb(236, 238, 242),
            Selection = Color.FromArgb(228, 236, 253),
            Text = Color.FromArgb(28, 30, 36),
            MutedText = Color.FromArgb(105, 112, 125),
            Accent = Color.FromArgb(37, 99, 235),
            AccentHover = Color.FromArgb(29, 78, 216),
            Success = Color.FromArgb(22, 163, 74),
            Warning = Color.FromArgb(217, 119, 6),
            Danger = Color.FromArgb(220, 38, 38),
            ToggleOff = Color.FromArgb(200, 204, 212),
        };

        public static readonly Theme Dark = new Theme
        {
            IsDark = true,
            Background = Color.FromArgb(23, 24, 28),
            Surface = Color.FromArgb(33, 35, 40),
            Border = Color.FromArgb(52, 55, 63),
            Input = Color.FromArgb(27, 29, 33),
            Hover = Color.FromArgb(45, 48, 55),
            Selection = Color.FromArgb(36, 52, 84),
            Text = Color.FromArgb(232, 234, 239),
            MutedText = Color.FromArgb(148, 154, 166),
            Accent = Color.FromArgb(59, 130, 246),
            AccentHover = Color.FromArgb(96, 153, 250),
            Success = Color.FromArgb(34, 197, 94),
            Warning = Color.FromArgb(245, 158, 11),
            Danger = Color.FromArgb(248, 113, 113),
            ToggleOff = Color.FromArgb(72, 76, 86),
        };
    }

    internal static class Draw
    {
        public static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            float d = System.Math.Min(radius * 2, System.Math.Min(r.Width, r.Height));
            var path = new GraphicsPath();
            if (d <= 0)
            {
                path.AddRectangle(r);
                return path;
            }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>Themed look for the tray right-click menu.</summary>
    internal sealed class MenuRenderer : ToolStripProfessionalRenderer
    {
        private readonly Theme _theme;

        public MenuRenderer(Theme theme) : base(new MenuColors(theme))
        {
            _theme = theme;
            RoundedEdges = false;
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? _theme.Text : _theme.MutedText;
            base.OnRenderItemText(e);
        }

        private sealed class MenuColors : ProfessionalColorTable
        {
            private readonly Theme _t;
            public MenuColors(Theme t) { _t = t; UseSystemColors = false; }

            public override Color ToolStripDropDownBackground => _t.Surface;
            public override Color ImageMarginGradientBegin => _t.Surface;
            public override Color ImageMarginGradientMiddle => _t.Surface;
            public override Color ImageMarginGradientEnd => _t.Surface;
            public override Color MenuBorder => _t.Border;
            public override Color MenuItemBorder => _t.Hover;
            public override Color MenuItemSelected => _t.Hover;
            public override Color MenuItemSelectedGradientBegin => _t.Hover;
            public override Color MenuItemSelectedGradientEnd => _t.Hover;
            public override Color MenuItemPressedGradientBegin => _t.Hover;
            public override Color MenuItemPressedGradientEnd => _t.Hover;
            public override Color SeparatorDark => _t.Border;
            public override Color SeparatorLight => _t.Surface;
        }
    }
}
