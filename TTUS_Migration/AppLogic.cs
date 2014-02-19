using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using TTUSAPI;

namespace TTUS_Migration
{
    static class AppLogic
    {
        public static string DataDir = "Data";
        public static int NumberofLimits = -1;

        public static bool CreateExchangeTrader(string gateway, string member, string group, string trader, string password, string currency_key)
        {
            if (ASG.TTUS.m_Gateways.ContainsKey(ASG.TTUS.GetGatewayIDByName(gateway)))
            {
                if (ASG.TTUS.m_Currencies.ContainsKey(currency_key))
                {
                    TTUSAPI.DataObjects.GatewayCredentialProfile gcp =
                        new TTUSAPI.DataObjects.GatewayCredentialProfile(ASG.TTUS.m_Gateways[ASG.TTUS.GetGatewayIDByName(gateway)]);

                    gcp.Member = member;
                    gcp.Group = group;
                    gcp.Trader = trader;

                    TTUSAPI.DataObjects.GatewayLoginProfile glp = new TTUSAPI.DataObjects.GatewayLoginProfile();
                    glp.AddGatewayCredential(gcp);

                    glp.Member = member;
                    glp.Group = group;
                    glp.Trader = trader;
                    glp.Password = password;
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
            try
            {
                TTUSAPI.DataObjects.GatewayLoginProfile glp = new TTUSAPI.DataObjects.GatewayLoginProfile(ttord);

                int gwid = ASG.TTUS.GetGatewayIDByName(gateway);
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

        public static void CopyProductLimits(string username, TTUSAPI.DataObjects.Gateway gw1, TTUSAPI.DataObjects.Gateway gw2)
        { 

            
        }
    }
}
