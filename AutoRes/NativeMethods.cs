using System;
using System.Runtime.InteropServices;

namespace AutoRes
{
    internal static class NativeMethods
    {
        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xFFFF);
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int CB_SETCUEBANNER = 0x1703;

        /// <summary>Message a second instance sends to tell the running one to show itself.</summary>
        public static readonly int WM_SHOW_AUTORES = RegisterWindowMessage("AutoRes_ShowMainWindow");

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int RegisterWindowMessage(string message);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string appName, string idList);

        public static void BroadcastShowMessage()
        {
            PostMessage(HWND_BROADCAST, WM_SHOW_AUTORES, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>Dark or light window title bar (Windows 10 1809+ / Windows 11, ignored elsewhere).</summary>
        public static void SetDarkTitleBar(IntPtr hwnd, bool dark)
        {
            try
            {
                int value = dark ? 1 : 0;
                if (DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int)) != 0)
                    DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_OLD, ref value, sizeof(int));
            }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
        }

        /// <summary>Applies a visual-style class such as "DarkMode_Explorer" (dark scrollbars, combo arrows).</summary>
        public static void SetControlTheme(IntPtr hwnd, string theme)
        {
            try { SetWindowTheme(hwnd, theme, null); }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
        }

        public static void SetCueBanner(IntPtr comboHandle, string text)
        {
            SendMessage(comboHandle, CB_SETCUEBANNER, IntPtr.Zero, text ?? string.Empty);
        }
    }
}
