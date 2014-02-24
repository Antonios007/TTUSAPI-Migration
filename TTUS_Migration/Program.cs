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

            if (File.Exists("config\\data.ini"))
            {
                try
                {
                    using (StreamReader sr = new StreamReader("config\\data.ini"))
                    {
                        String line = sr.ReadLine();
                        AppLogic.DataFile = line.Trim();
                    }
                    Trace.WriteLine(string.Format("Data file set to: {0}", AppLogic.DataFile));
                    f.button_ReadConfig.Enabled = true;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }

            if (File.Exists("config\\user.ini"))
            {
                ASG.Utility.ReadDictFromFile("config\\user.ini", ',', ref user);
            }
            else
            {
                Application.Run(new FormLogin());
            }

            ASG.TTUS.TTUS_User = user.Keys.ToList<string>()[0];
            ASG.TTUS.password = user.Values.ToList<string>()[0];

            ASG.TTUS.Start();
            ASG.TTUS.SubcribeForCallbacks();

            ASG.Utility.ErrorReport = f.listBox_Errors;
            ASG.Utility.ReadListFromFile("config\\consolidate.ini", ref AppLogic.Gateways2Consolidate);
            ASG.Utility.ReadListFromFile("config\\gateways.ini", ref AppLogic.TargetGateways);

            // TODO remove for prod code
            f.button_ConsolidateLimits.Enabled = true;

            Application.Run(f);

        }
    }
}
