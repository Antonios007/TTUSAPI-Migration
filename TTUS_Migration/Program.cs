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

            ASG.TTUS_API.mainform = f;

            if (File.Exists("user.ini"))
            {
                ASG.Utility.ReadDictFromFile("user.ini", ref user);
            }
            else
            {
                Application.Run(new FormLogin());
            }

            ASG.TTUS_API.TTUS_User = user.Keys.ToList<string>()[0];
            ASG.TTUS_API.password = user.Values.ToList<string>()[0];

            ASG.TTUS_API.Start();
            ASG.TTUS_API.SubcribeForCallbacks();

            Application.Run(f);
        }
    }
}
