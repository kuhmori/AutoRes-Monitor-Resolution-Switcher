using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace AutoRes
{
    internal static class ResolutionHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int CDS_UPDATEREGISTRY = 0x00000001;
        private const int DISP_CHANGE_SUCCESSFUL = 0;
        private const int DM_PELSWIDTH = 0x00080000;
        private const int DM_PELSHEIGHT = 0x00100000;

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        private static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        private static DEVMODE NewDevMode()
        {
            return new DEVMODE { dmSize = (short)Marshal.SizeOf(typeof(DEVMODE)) };
        }

        public static Size GetCurrentResolution()
        {
            var dm = NewDevMode();
            return EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm) != 0
                ? new Size(dm.dmPelsWidth, dm.dmPelsHeight)
                : Size.Empty;
        }

        /// <summary>All resolutions the primary display supports, largest first.</summary>
        public static List<Size> GetSupportedResolutions()
        {
            var modes = new HashSet<Size>();
            var dm = NewDevMode();
            for (int i = 0; EnumDisplaySettings(null, i, ref dm) != 0; i++)
            {
                if (dm.dmPelsWidth >= 640 && dm.dmPelsHeight >= 480)
                    modes.Add(new Size(dm.dmPelsWidth, dm.dmPelsHeight));
            }
            return modes.OrderByDescending(s => s.Width).ThenByDescending(s => s.Height).ToList();
        }

        /// <summary>
        /// Changes the primary display resolution. With <paramref name="persist"/> = false the change is
        /// temporary (not written to the registry), so a crash or reboot won't leave you stuck on it.
        /// </summary>
        public static bool SetResolution(int width, int height, bool persist)
        {
            var dm = NewDevMode();
            if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm) == 0) return false;

            dm.dmPelsWidth = width;
            dm.dmPelsHeight = height;
            dm.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT;

            int result = ChangeDisplaySettings(ref dm, persist ? CDS_UPDATEREGISTRY : 0);
            if (result != DISP_CHANGE_SUCCESSFUL && persist)
                result = ChangeDisplaySettings(ref dm, 0);

            return result == DISP_CHANGE_SUCCESSFUL;
        }

        public static string Format(int width, int height) => $"{width} × {height}";
        public static string Format(Size size) => Format(size.Width, size.Height);

        private static readonly Regex ResolutionPattern = new Regex(@"(\d{3,5})\s*[^\d]+\s*(\d{3,5})", RegexOptions.Compiled);

        /// <summary>Accepts "1920x1080", "1920 × 1080", "1920*1080", "1920 1080" …</summary>
        public static bool TryParse(string text, out Size size)
        {
            size = Size.Empty;
            if (string.IsNullOrWhiteSpace(text)) return false;
            var match = ResolutionPattern.Match(text);
            if (!match.Success) return false;
            int w = int.Parse(match.Groups[1].Value);
            int h = int.Parse(match.Groups[2].Value);
            if (w < 320 || h < 200) return false;
            size = new Size(w, h);
            return true;
        }
    }
}
