using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace AutoRes
{
    // NOTE: class and property names are part of the settings.xml format - keep them stable
    // so existing settings from older versions keep loading.

    [Serializable]
    public class ResolutionData
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public ResolutionData() { }
        public ResolutionData(Size size) { Width = size.Width; Height = size.Height; }
        public ResolutionData(int width, int height) { Width = width; Height = height; }

        public Size ToSize() => new Size(Width, Height);
        public override string ToString() => ResolutionHelper.Format(Width, Height);
    }

    [Serializable]
    public class GameProfile
    {
        public string GameName { get; set; }
        public string ProcessName { get; set; }
        public ResolutionData GameResolution { get; set; }

        public GameProfile() { }
        public GameProfile(string processName, ResolutionData resolution)
        {
            GameName = processName;
            ProcessName = processName;
            GameResolution = resolution;
        }

        public override string ToString() => $"{ProcessName} – {GameResolution}";
    }

    [Serializable]
    public class AppSettings
    {
        public ResolutionData NormalResolution { get; set; }
        public List<GameProfile> GameProfiles { get; set; } = new List<GameProfile>();
        public bool Autostart { get; set; }
        public bool DarkMode { get; set; }
        public bool StartMinimized { get; set; }
        public bool CloseToTray { get; set; } = true;
        public bool TrayHintShown { get; set; }

        private static string SettingsPath
        {
            get
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoRes");
                Directory.CreateDirectory(folder);
                return Path.Combine(folder, "settings.xml");
            }
        }

        public void Save()
        {
            var path = SettingsPath;
            var temp = path + ".tmp";
            using (var fs = new FileStream(temp, FileMode.Create))
            {
                new XmlSerializer(typeof(AppSettings)).Serialize(fs, this);
            }
            // Write to a temp file first so a crash mid-save can't leave a corrupt settings file.
            if (File.Exists(path)) File.Delete(path);
            File.Move(temp, path);
        }

        public static AppSettings Load()
        {
            try
            {
                var path = SettingsPath;
                if (!File.Exists(path)) return new AppSettings();
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var settings = (AppSettings)new XmlSerializer(typeof(AppSettings)).Deserialize(fs);
                    settings.GameProfiles = settings.GameProfiles ?? new List<GameProfile>();
                    settings.GameProfiles.RemoveAll(p => string.IsNullOrWhiteSpace(p?.ProcessName) || p.GameResolution == null);
                    return settings;
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
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "AutoRes";

        public static void SetAutostart(bool enable)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(RunKey))
            {
                if (key == null) return;
                if (enable)
                    key.SetValue(ValueName, "\"" + Application.ExecutablePath + "\"");
                else
                    key.DeleteValue(ValueName, false);
            }
        }
    }
}
