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

        private void button1_Click(object sender, EventArgs e)
        {

            //int gwid = ASG.TTUS.GetGatewayIDByName("CME-H");
            //MessageBox.Show(gwid.ToString());
            try
            {
                TTUSAPI.DataObjects.GatewayLogin gwl = null;

                if (ASG.TTUS.GetGWLoginFromUsername("ANTONIOS3", ASG.TTUS.m_Users, ref gwl))
                {
                    AppLogic.AttachExchangeTrader("CME-H", "TTUSAPI", "ASG", "ANTONIOS", gwl);

                   
                    
                    List<TTUSAPI.DataObjects.GatewayLoginProductLimitProfile> m_GWRiskLimits =
                        new List<TTUSAPI.DataObjects.GatewayLoginProductLimitProfile>();

                    ASG.TTUS.CleanProductLimits(
                        gwl.ProductLimits,
                        "CME-H",
                        AppLogic.Gateways2Consolidate,
                        ref m_GWRiskLimits);

                    ASG.TTUS.UploadLimits(m_GWRiskLimits, gwl);
                }
                
            }
            catch (Exception ex)
            { Trace.WriteLine(ex.Message); }

        }



        private void button_ReadConfig_Click(object sender, EventArgs e)
        {
            if (File.Exists(AppLogic.DataFile ))
            {
                AppLogic.InputData = ASG.Utility.ReadCSV(AppLogic.DataFile, true);
                if (AppLogic.InputData.Rows.Count > 0)
                {
                    this.button_InsertExchangeTraders.Enabled = true;
                }
            }
            else
            { MessageBox.Show(string.Format("{0} file not found",AppLogic.DataFile),"ERROR");}
        }

        private void button_InsertExchangeTraders_Click(object sender, EventArgs e)
        {
            AppLogic.CreateAllExchangeTraders();
            this.button_AttachExchangeTraders.Enabled = true;
        }

        private void button_AttachExchangeTraders_Click(object sender, EventArgs e)
        {

        }
    }
}
