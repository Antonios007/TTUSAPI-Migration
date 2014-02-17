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

        public static void CreateExchangeTrader()
        {
            TTUSAPI.DataObjects.GatewayCredentialProfile gcp =
                new TTUSAPI.DataObjects.GatewayCredentialProfile(ASG.TTUS_API.m_Gateways[ASG.TTUS_API.GetGatewayIDByname("CME-Q")]);

            gcp.Member = "TTUSAPI";
            gcp.Group = "ASG";
            gcp.Trader = "999";

            TTUSAPI.DataObjects.GatewayLoginProfile glp = new TTUSAPI.DataObjects.GatewayLoginProfile();
            glp.AddGatewayCredential(gcp);
            glp.Member = "TTUSAPI";
            glp.Group = "ASG";
            glp.Trader = "999";
            glp.Password = "1234";
            glp.GatewayLoginRiskSettings.Currency = ASG.TTUS_API.m_Currencies["USD"];
            glp.GatewayLoginRiskSettings.TradingAllowed = true;


            ResultStatus r = ASG.TTUS_API.m_TTUSAPI.AddGatewayLogin(glp);
            Trace.WriteLine(string.Format("RESULT: {0} [{1}] {2}", r.Result, r.TransactionID, r.ErrorMessage));

        }

        public static void AttachExchangeTrader()
        {
            try
            {
                TTUSAPI.DataObjects.GatewayLoginProfile glp =
                    new TTUSAPI.DataObjects.GatewayLoginProfile(ASG.TTUS_API.m_GatewayLogins["TTORDSL ASG 003"]);

                TTUSAPI.DataObjects.GatewayCredentialProfile gcp =
                    new TTUSAPI.DataObjects.GatewayCredentialProfile(
                        ASG.TTUS_API.m_GatewayLogins["TTUSAPI ASG 999"].GatewayCredentials["TTUSAPI ASG 999 690"]);

                Trace.WriteLine(glp.GatewayCredentials.Count);
                glp.AddGatewayCredential(gcp);
                Trace.WriteLine(glp.GatewayCredentials.Count);

                ResultStatus r = ASG.TTUS_API.m_TTUSAPI.UpdateGatewayLogin(glp);
                Trace.WriteLine(string.Format("RESULT: {0} [{1}] {2}", r.Result, r.TransactionID, r.ErrorMessage));

            }
            catch (Exception ex)
            { Trace.WriteLine(ex.Message); }
        }
    }
}
