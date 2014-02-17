using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace TTUS_Migration
{
    static class Program
    {
        public static Dictionary<string, string> user = new Dictionary<string, string>();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ASG.Utility.setupLog();
            Trace.WriteLine(Environment.CommandLine);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 f = new Form1();

            ASG.TTUS.mainform = f;

            if (File.Exists("user.ini"))
            {
                ASG.Utility.ReadDictFromFile("user.ini", ref user);
            }
            else
            {
                Application.Run(new FormLogin());
            }

            ASG.TTUS.TTUS_User = user.Keys.ToList<string>()[0];
            ASG.TTUS.password = user.Values.ToList<string>()[0];

            ASG.TTUS.Start();
            ASG.TTUS.SubcribeForCallbacks();

            Application.Run(f);
        }
    }
}
