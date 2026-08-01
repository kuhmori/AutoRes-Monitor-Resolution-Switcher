using System;
using System.IO;
using System.Windows.Forms;

namespace AutoRes.Ui
{
    static class Program
    {
        public static string IconPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "icons8-monitor-32.ico");

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
