using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using TTUSAPI;

namespace TTUS_Migration
{
    static class AppLogic
    {
        public static string DataFile = "test.csv";
        
        //populated through config file.
        public static List<string> Gateways2Consolidate = new List<string>();
        public static List<string> TargetGateways = new List<string>();
        public static DataTable InputData = new DataTable();

        //must ensure updates processed in callback before attaching
        public static bool CreateExchangeTrader(string gateway_name, string member, string group, string trader, string currency_key)
        {
            ASG.Utility.DisplayCurrentMethodName();
            int gateway_id = ASG.TTUS.GetGatewayIDByName(gateway_name);

            if (ASG.TTUS.m_Gateways.ContainsKey(gateway_id))
            {
                if (ASG.TTUS.m_Currencies.ContainsKey(currency_key))
                {
                    TTUSAPI.DataObjects.GatewayCredentialProfile gcp =
                        new TTUSAPI.DataObjects.GatewayCredentialProfile(ASG.TTUS.m_Gateways[gateway_id]);

                    gcp.Member = member;
                    gcp.Group = group;
                    gcp.Trader = trader;

                    TTUSAPI.DataObjects.GatewayLoginProfile glp = new TTUSAPI.DataObjects.GatewayLoginProfile();
                    glp.AddGatewayCredential(gcp);

                    glp.Member = member;
                    glp.Group = group;
                    glp.Trader = trader;
                    //glp.Password = password;
                    // "USD" = US Dollar currency_key
                    glp.GatewayLoginRiskSettings.Currency = ASG.TTUS.m_Currencies[currency_key];
                    glp.GatewayLoginRiskSettings.TradingAllowed = true;

                    ResultStatus r = ASG.TTUS.m_TTUSAPI.AddGatewayLogin(glp);
                    Trace.WriteLine(string.Format("RESULT: {0} [{1}] {2}", r.Result, r.TransactionID, r.ErrorMessage));
                    return true;
                }
                else
                {
                    Trace.WriteLine("Currency not found");
                    return false;
                }
            }
            else 
            {
                Trace.WriteLine("Gateway not found");
                return false; 
            }

        }

        public static void AttachExchangeTrader(string gateway, string member, string group, string trader, TTUSAPI.DataObjects.GatewayLogin ttord)
        {
            ASG.Utility.DisplayCurrentMethodName();

            try
            {
                TTUSAPI.DataObjects.GatewayLoginProfile glp = new TTUSAPI.DataObjects.GatewayLoginProfile(ttord);

                int gwid = ASG.TTUS.GetGatewayIDByName(gateway);
                Trace.WriteLine(string.Format("GatewayID: {0} Gateway Name:{1}", gwid, gateway));

                TTUSAPI.DataObjects.GatewayCredentialProfile gcp = new TTUSAPI.DataObjects.GatewayCredentialProfile(ASG.TTUS.m_Gateways[gwid]);
                gcp.GatewayID = gwid;
                gcp.Member = member;
                gcp.Group = group ;
                gcp.Trader = trader ;
                glp.AddGatewayCredential(gcp);
    
                ResultStatus r = ASG.TTUS.m_TTUSAPI.UpdateGatewayLogin(glp);
                Trace.WriteLine(string.Format("RESULT: {0} [{1}] {2}", r.Result, r.TransactionID, r.ErrorMessage));
            }
            catch (Exception ex)
            { Trace.WriteLine(ex.Message); }
        }

        public static void CreateAllExchangeTraders()
        {
            ASG.Utility.DisplayCurrentMethodName();

            foreach (DataRow dr in InputData.Rows)
            {
                //dr[9]  = CME-H Column
                //dr[10] = CME-J column

                if (dr[9].ToString().Length > 0)
                {
                    CreateExchangeTrader("CME-H", dr[9].ToString(), dr[7].ToString(), dr[8].ToString(), "USD");
                }

                if (dr[10].ToString().Length > 0)
                {
                    CreateExchangeTrader("CME-J", dr[10].ToString(), dr[7].ToString(), dr[8].ToString(), "USD");
                }
            }
        }

        public static void AttachAllExchangeTraders()
        {
            ASG.Utility.DisplayCurrentMethodName();
            foreach (DataRow dr in InputData.Rows)
            {
                //dr[0]  = User 
                //dr[7]  = Exchange Group
                //dr[8]  = Exchange Trader
                //dr[9]  = CME-H Column
                //dr[10] = CME-J column
                TTUSAPI.DataObjects.GatewayLogin gwl = null;
                if (ASG.TTUS.GetGWLoginFromUsername(dr[0].ToString(), ASG.TTUS.m_Users, ref gwl))
                {
                    if (dr[9].ToString().Length > 0)
                    {
                        AttachExchangeTrader("CME-H", dr[9].ToString(), dr[7].ToString(), dr[8].ToString(), gwl);
                    }

                    if (dr[10].ToString().Length > 0)
                    {
                        AttachExchangeTrader("CME-J", dr[10].ToString(), dr[7].ToString(), dr[8].ToString(), gwl);
                    }
                }
            }
        }

        public static void ConsolidateGateways()
        {
            ASG.Utility.DisplayCurrentMethodName();

            string last_user = string.Empty;

            foreach (DataRow dr in InputData.Rows)
            {
                //dr[0]  = User 
                //dr[7]  = Exchange Group
                //dr[8]  = Exchange Trader
                //dr[9]  = CME-H Column
                //dr[10] = CME-J column

                if (!dr[0].ToString().Equals(last_user))
                {
                    TTUSAPI.DataObjects.GatewayLogin gwl = null;
                    if (ASG.TTUS.GetGWLoginFromUsername(dr[0].ToString(), ASG.TTUS.m_Users, ref gwl))
                    {
                        List<TTUSAPI.DataObjects.GatewayLoginProductLimitProfile> m_GWRiskLimits =
                            new List<TTUSAPI.DataObjects.GatewayLoginProductLimitProfile>();
                        Trace.WriteLine("Process CME-H");
                        ASG.TTUS.CleanProductLimits(
                             gwl.ProductLimits,
                             "CME-H",
                             AppLogic.Gateways2Consolidate,
                             ref m_GWRiskLimits);

                        ASG.TTUS.UploadLimits(m_GWRiskLimits, gwl);
                        Trace.WriteLine("Process CME-J");
                        ASG.TTUS.CleanProductLimits(
                             gwl.ProductLimits,
                             "CME-J",
                             AppLogic.Gateways2Consolidate,
                             ref m_GWRiskLimits);

                        ASG.TTUS.UploadLimits(m_GWRiskLimits, gwl);
                    }
                    last_user = dr[0].ToString();
                }
            }
        }
    }
}
