using System;
using System.Threading;
using System.Windows.Forms;

namespace AutoRes
{
    internal static class Program
    {
        private const string MutexName = "AutoRes_SingleInstance_193B2E66";

        [STAThread]
        private static void Main()
        {
            bool isFirstInstance;
            using (var mutex = new Mutex(true, MutexName, out isFirstInstance))
            {
                if (!isFirstInstance)
                {
                    // AutoRes is already running (e.g. started with Windows) - bring that window up instead.
                    NativeMethods.BroadcastShowMessage();
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());

                GC.KeepAlive(mutex);
            }
        }
    }
}
