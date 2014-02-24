using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace TTUS_Migration
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            ASG.TTUS.ShutDown();
        }

        private void setFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO refactor to utility class
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            //openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        AppLogic.DataFile = openFileDialog1.FileName;
                        this.button_ReadConfig.Enabled = true;
                        using (myStream)
                        {
                            // Insert code to read the stream here.
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }

            using (StreamWriter writer = File.CreateText("config\\data.ini"))
            {
                try
                {
                    writer.WriteLine(AppLogic.DataFile);
                }
                catch (Exception ex)
                { Trace.WriteLine(ex.Message); }
            }
        }

        private void button_ReadConfig_Click(object sender, EventArgs e)
        {
            if (File.Exists(AppLogic.DataFile ))
            {
                AppLogic.InputData = ASG.Utility.ReadCSV(AppLogic.DataFile, true);
                if (AppLogic.InputData.Rows.Count > 0)
                {
                    this.button_InsertExchangeTraders.Enabled = true;
                    this.button_ReadConfig.Enabled = false;
                }
            }
            else
            { MessageBox.Show(string.Format("{0} file not found",AppLogic.DataFile),"ERROR");}
        }

        private void button_InsertExchangeTraders_Click(object sender, EventArgs e)
        {
            AppLogic.CreateAllExchangeTraders();
            this.button_InsertExchangeTraders.Enabled = false;
            this.button_AttachExchangeTraders.Enabled = true;
        }

        private void button_AttachExchangeTraders_Click(object sender, EventArgs e)
        {
            AppLogic.AttachAllExchangeTraders();
            this.button_AttachExchangeTraders.Enabled = false;
            this.button_ConsolidateLimits.Enabled = true;
            // TODO default settings show up as auto-login and not available to user
        }

        private void button_ConsolidateLimits_Click(object sender, EventArgs e)
        {
            AppLogic.ConsolidateGateways();
            this.button_ConsolidateLimits.Enabled = false;
        }


    }
}
