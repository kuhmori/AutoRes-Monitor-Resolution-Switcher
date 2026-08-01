using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace AutoRes.Ui
{
    [Serializable]
    public class ResolutionData
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public ResolutionData() { }
        public ResolutionData(Size s) { Width = s.Width; Height = s.Height; }

        public Size ToSize() => new Size(Width, Height);
    }

    [Serializable]
    public class GameProfile
    {
        public string GameName { get; set; }
        public string ProcessName { get; set; }
        public ResolutionData GameResolution { get; set; }

        public GameProfile() { }
        public GameProfile(string name, string proc, ResolutionData res)
        {
            GameName = name;
            ProcessName = proc;
            GameResolution = res;
        }
    }

    [Serializable]
    public class AppSettings
    {
        public ResolutionData NormalResolution { get; set; }
        public List<GameProfile> GameProfiles { get; set; }
        public bool Autostart { get; set; }
        public bool DarkMode { get; set; }

        public AppSettings()
        {
            GameProfiles = new List<GameProfile>();
        }

        private static string GetSettingsPath()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoRes");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return Path.Combine(folder, "settings.xml");
        }

        public void Save()
        {
            var path = GetSettingsPath();
            using (var fs = new FileStream(path, FileMode.Create))
            {
                var ser = new XmlSerializer(typeof(AppSettings));
                ser.Serialize(fs, this);
            }
        }

        public static AppSettings Load()
        {
            var path = GetSettingsPath();
            if (!File.Exists(path)) return new AppSettings();
            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    var ser = new XmlSerializer(typeof(AppSettings));
                    return (AppSettings)ser.Deserialize(fs);
                }
            }
            catch
            {
                return new AppSettings();
            }
        }
    }

    public static class AutostartHelper
    {
        public static void SetAutostart(string appName, bool enable)
        {
            var runKey = "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run";
            if (enable)
            {
                var exe = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                Microsoft.Win32.Registry.SetValue(runKey, appName, "\"" + exe + "\"");
            }
            else
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (key == null) return;
                    key.DeleteValue(appName, false);
                }
            }
        }
    }
}
