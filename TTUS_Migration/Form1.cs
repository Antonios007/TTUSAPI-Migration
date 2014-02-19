using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
            if (ASG.TTUS.GetGWLoginFromUsername("ANTONIOS3", ASG.TTUS.m_Users, ref AppLogic.m_UpdateGWLogin))
            {
                // TODO verify that this logic will work
                AppLogic.m_GatewayLoginProductLimits = AppLogic.m_UpdateGWLogin.ProductLimits;
                ASG.TTUS.CleanProductLimits(
                    AppLogic.m_GatewayLoginProductLimits, 
                    "CME-H", 
                    ref AppLogic.m_GWRiskLimits, 
                    ref AppLogic.Gateways2Consolidate);

                ASG.TTUS.UploadLimits(AppLogic.m_GWRiskLimits, AppLogic.m_UpdateGWLogin);


            }

        }
    }
}
