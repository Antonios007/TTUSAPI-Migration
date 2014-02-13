using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace ASG
{
    public static class Utility
    {
        #region Logging Capability
        public static TextWriterTraceListener log;

        public static void setupLog()
        {
            VerifyDirectory("logfiles");
            Trace.AutoFlush = true;
            log = new TextWriterTraceListener(System.IO.File.CreateText("logfiles\\" +
            Process.GetCurrentProcess().ProcessName + DateTime.Now.ToString("yyMMdd-HHmmss") + ".log"));
            Trace.Listeners.Add(log);
        }

        #endregion


        public static bool VerifyDirectory(string dir)
        {
            try
            {
                if (Directory.Exists(dir))
                { return true; }

                DirectoryInfo di = Directory.CreateDirectory(dir);
                if (di.Exists)
                { return true; }
                else
                { return false; }
            }
            catch (Exception ex)
            {
                log.WriteLine(ex.Message);
                return false;
            }

        }

        public static void ReadDictFromFile(string fn, ref Dictionary<string, string> datastore)
        {

            //Make sure the input file is there
            if (!File.Exists(fn))
            {
                log.WriteLine("File " + fn + " does not exist in the specified path.");
            }
            else
            {
                datastore.Clear();
                TextReader tr = new StreamReader(fn);

                string line;
                //Read the input file and store the info in the dictionary of updateObjects
                while (tr.Peek() >= 0)
                {
                    line = tr.ReadLine();
                    if (line != "" && !line.StartsWith("#")) // Ignore blank lines and ones that begin with #
                    {
                        try  //Store file info in the updateObject dictionary
                        {
                            string[] dataRow = line.Split(',');  //Expecting comma delineated input                
                            datastore.Add(dataRow[0].Trim(), dataRow[1].Trim());
                            log.WriteLine(string.Format("{0} : {1}", dataRow[0].Trim(), dataRow[1].Trim()));
                        }
                        catch (Exception e)  //Exception reading file, likely it is not in the expected format
                        {
                            log.WriteLine("Exception reading file: " + e.Message);
                            log.WriteLine("File " + fn + " not in the correct format.  Correct format is:");
                            log.WriteLine("data,data");
                        }
                    }
                }
            }
        }

        public static void DisplayCurrentMethodName()
        {
            System.Diagnostics.StackFrame frame = new System.Diagnostics.StackFrame(1);
            var method = frame.GetMethod();
            Console.ForegroundColor = ConsoleColor.Green;
            Trace.WriteLine(method.Name);
            Console.ResetColor();
        }

    }
}
