using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RemoteBrowserServer
{
    static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Contains("--forced-shutdown"))
            {
                Process process = new Process() { StartInfo = new ProcessStartInfo("taskkill.exe", "/f /im \"" + new FileInfo(Application.ExecutablePath).Name + "\"") { CreateNoWindow = true, UseShellExecute = false } };
                process.Start();
                Environment.Exit(0);
            }
            Application.Run(new Server());
        }
    }
}
