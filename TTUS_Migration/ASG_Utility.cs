using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml;

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

        //Read a file containing a single entry per line
        public static void ReadListFromFile(string fn, ref List<string> datastore)
        {
            DisplayCurrentMethodName();

            //Make sure the input file is there
            if (!File.Exists(fn))
            {
                Trace.WriteLine("File " + fn + " does not exist in the specified path.");
            }
            else
            {
                Trace.WriteLine(string.Format("Data File: {0}", fn));
                datastore.Clear();

                TextReader tr = new StreamReader(fn);
                string line;
                //Read the input file and store the info in the dictionary of updateObjects
                while (tr.Peek() >= 0)
                {
                    line = tr.ReadLine();
                    if (line != "" && !line.StartsWith("#"))  // Ignore blank lines and ones that begin with #
                    {
                        try  
                        {
                            Trace.WriteLine(line);
                            datastore.Add(line.Trim());
                        }
                        catch (Exception e)  //Exception reading file, likely because it is not in the expected format
                        {
                            Trace.WriteLine("Exception reading file: " + e.Message);
                            Trace.WriteLine("File " + fn + " not in the correct format.  Correct format is to have one item per line");
                        }
                    }
                }
                Trace.WriteLine(string.Format("{0} lines added to datastore", datastore.Count));
            }
        }

        //Read a file containing two items per line separated by 'separator'
        public static void ReadDictFromFile(string fn, char separator, ref Dictionary<string, string> datastore)
        {
            // ',' = separator for CSV

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
                            string[] dataRow = line.Split(separator);                 
                            datastore.Add(dataRow[0].Trim(), dataRow[1].Trim());
                            Trace.WriteLine(string.Format("{0} : {1}", dataRow[0].Trim(), dataRow[1].Trim()));
                        }
                        catch (Exception e)  //Exception reading file, likely it is not in the expected format
                        {
                            Trace.WriteLine("Exception reading file: " + e.Message);
                            Trace.WriteLine("File " + fn + " not in the correct format.  Correct format is:");
                            Trace.WriteLine("data" + separator + "data");
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

        public static void CaptureDictionaryToDisk<T, U>(Dictionary<T, U> dict, string fn, string d)
        {
            ASG.Utility.DisplayCurrentMethodName();

            if (ASG.Utility.VerifyDirectory(d))
            {
                //http://msdn.microsoft.com/en-us/library/system.runtime.serialization.datacontractserializer.aspx
                DataContractSerializer dcs = new DataContractSerializer(typeof(Dictionary<T, U>));
                FileStream writer = new FileStream(d + "\\" + fn + ".XML", FileMode.Create);
                dcs.WriteObject(writer, dict);
                writer.Close();
            }
        }

        public static Dictionary<T, U> RetreiveDictionaryFromDisk<T, U>(string filename, string d)
        {
            if (ASG.Utility.VerifyDirectory(d))
            {
                FileStream fs = new FileStream(d + "\\" + filename, FileMode.Open);
                Trace.WriteLine(string.Format("Deserializing {0}", fs.Name));
                XmlDictionaryReader reader =
                    XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                DataContractSerializer ser = new DataContractSerializer(typeof(Dictionary<T, U>));

                // Deserialize the data and read it from the instance.
                Dictionary<T, U> deserialized = (Dictionary<T, U>)ser.ReadObject(reader, true);
                reader.Close();
                fs.Close();
                Trace.WriteLine(string.Format("Deserialized {0} entries", deserialized.Count));

                return deserialized;
            }
            else
            {
                Trace.WriteLine("Data Export directory is not found");
                return new Dictionary<T, U>();
            }
        }
    }
}
