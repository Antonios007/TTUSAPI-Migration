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

        public static ResultStatus CreateExchangeTrader(string gateway, string member, string group, string trader, string password, string currency_key)
        {
            TTUSAPI.DataObjects.GatewayCredentialProfile gcp =
                new TTUSAPI.DataObjects.GatewayCredentialProfile(ASG.TTUS.m_Gateways[ASG.TTUS.GetGatewayIDByname(gateway)]);

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
            return r;
        }

        public static void AttachExchangeTrader()
        {
            try
            {
                TTUSAPI.DataObjects.GatewayLoginProfile glp =
                    new TTUSAPI.DataObjects.GatewayLoginProfile(ASG.TTUS.m_GatewayLogins["TTORDSL ASG 003"]);

                TTUSAPI.DataObjects.GatewayCredentialProfile gcp =
                    new TTUSAPI.DataObjects.GatewayCredentialProfile(
                        ASG.TTUS.m_GatewayLogins["TTUSAPI ASG 999"].GatewayCredentials["TTUSAPI ASG 999 690"]);

                Trace.WriteLine(glp.GatewayCredentials.Count);
                glp.AddGatewayCredential(gcp);
                Trace.WriteLine(glp.GatewayCredentials.Count);

                ResultStatus r = ASG.TTUS.m_TTUSAPI.UpdateGatewayLogin(glp);
                Trace.WriteLine(string.Format("RESULT: {0} [{1}] {2}", r.Result, r.TransactionID, r.ErrorMessage));

            }
            catch (Exception ex)
            { Trace.WriteLine(ex.Message); }
        }
    }
}
