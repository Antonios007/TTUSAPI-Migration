using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

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

        private void button1_Click(object sender, EventArgs e)
        {

            //int gwid = ASG.TTUS.GetGatewayIDByName("CME-H");
            //MessageBox.Show(gwid.ToString());
            try
            {
                //ASG.Utility.ReadListFromFile("consolidate.ini", ref AppLogic.Gateways2Consolidate);
                //must be done separately to ensure updates processed before proceeding
                AppLogic.CreateExchangeTrader("CME-H", "TTUSAPI", "ASG", "ANTONIOS", "1234", "USD");

                TTUSAPI.DataObjects.GatewayLogin gwl = null;

                //if (ASG.TTUS.GetGWLoginFromUsername("ANTONIOS3", ASG.TTUS.m_Users, ref gwl))
                //{
                //    AppLogic.AttachExchangeTrader("CME-H", "TTUSAPI", "ASG", "ANTONIOS", gwl);

                List<TTUSAPI.DataObjects.GatewayLoginProductLimitProfile> m_GWRiskLimits = new List<TTUSAPI.DataObjects.GatewayLoginProductLimitProfile>();
                //    //ASG.TTUS.CleanProductLimits(
                //    //    gwl.ProductLimits,
                //    //    "CME-H",
                //    //    AppLogic.Gateways2Consolidate,
                //    //    ref m_GWRiskLimits);

                //    //ASG.TTUS.UploadLimits(m_GWRiskLimits, gwl);
                //}
            }
            catch (Exception ex)
            { Trace.WriteLine(ex.Message); }

        }

        private void button_InsertExchangeTraders_Click(object sender, EventArgs e)
        {
            AppLogic.CreateExchangeTrader("CME-H", "TTUSAPI", "ASG", "ANTONIOS", "1234", "USD");
        }
    }
}
