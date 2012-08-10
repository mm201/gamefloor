using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Gamefloor.Support
{
    internal class ResolutionHelper
    {
        // Retireve screen resolution info, courtesy of
        // http://stackoverflow.com/questions/744541/how-to-list-available-video-modes-using-c
        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
        const int ENUM_CURRENT_SETTINGS = -1;

        const int ENUM_REGISTRY_SETTINGS = -2;

        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
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

        public static List<Size> GetResolutions()
        {
            List<Size> results = new List<Size>();

            DEVMODE vDevMode = new DEVMODE();
            int i = 0;

            while (EnumDisplaySettings(null, i, ref vDevMode))
            {
                results.Add(new Size(vDevMode.dmPelsWidth, vDevMode.dmPelsHeight));
                i++;
            }

            return results;
        }

        // http://www.dotnetspark.com/kb/1948-change-display-settings-programmatically.aspx
        private static DEVMODE? GetCurrentSettings()
        {
            DEVMODE mode = new DEVMODE();
            mode.dmSize = (short)Marshal.SizeOf(mode);

            if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref mode) == true) // Succeeded
            {
                return mode;
            }
            else return null;
        }

        public static int GetCurrentRefreshRate()
        {
            DEVMODE ? mode = GetCurrentSettings();
            if (mode == null) return 0;
            else return ((DEVMODE)mode).dmDisplayFrequency;
        }
    }
}
