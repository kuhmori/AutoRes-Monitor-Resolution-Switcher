using System;
using System.Runtime.InteropServices;
using System.Drawing;

namespace AutoRes
{
    internal static class ResolutionHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct DEVMODE
        {
            private const int CCHDEVICENAME = 32;
            private const int CCHFORMNAME = 32;
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
        private const int CDS_TEST = 0x00000002;
        private const int DISP_CHANGE_SUCCESSFUL = 0;

        [DllImport("user32.dll")]
        private static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")] 
        private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        public static Size GetCurrentResolution()
        {
            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
            if (0 != EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
            {
                return new Size(dm.dmPelsWidth, dm.dmPelsHeight);
            }
            return new Size(800, 600);
        }

        public static bool SetResolution(int width, int height)
        {
            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            // Get current display settings
            if (0 != EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
            {
                // Modify the resolution
                dm.dmPelsWidth = width;
                dm.dmPelsHeight = height;
                dm.dmFields = 0x00080000 | 0x00040000; // DM_PELSWIDTH | DM_PELSHEIGHT

                // Try to change display settings with CDS_UPDATEREGISTRY flag
                int result = ChangeDisplaySettings(ref dm, CDS_UPDATEREGISTRY);

                // If UPDATEREGISTRY fails, try without the flag
                if (result != DISP_CHANGE_SUCCESSFUL)
                {
                    result = ChangeDisplaySettings(ref dm, 0);
                }

                return result == DISP_CHANGE_SUCCESSFUL;
            }
            return false;
        }
    }
}
