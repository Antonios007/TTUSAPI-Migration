using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using TTUSAPI;

using System.Threading;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace ASG
{
    using TTUS_Migration;

    static class TTUS
    {
        #region Internal TTUS Data storage
        public static TTUSApi m_TTUSAPI;
        public static string TTUS_User;
        public static string password;

        public static Form1 mainform;
        public static bool m_EnvironmentIsMB = false ;

        //this contains all users in the currently connected environment Key = username
        public static Dictionary<string, TTUSAPI.DataObjects.User> m_Users;

        //this contains all gateway logins in current env Key = MGT separated by spaces
        public static Dictionary<string, TTUSAPI.DataObjects.GatewayLogin> m_GatewayLogins;

        //this contains all gateways in the currently connected env key = int id and is the same for all environments
        public static Dictionary<int, TTUSAPI.DataObjects.Gateway> m_Gateways;

        // key is three character currency code i.e., "USD" for US dollar
        public static Dictionary<string, TTUSAPI.DataObjects.Currency> m_Currencies;

        public static Dictionary<int, TTUSAPI.DataObjects.ProductType> m_ProdTypes;
        public static Dictionary<int, TTUSAPI.DataObjects.MarketProductCatalog> m_MarketProductCatalogs;
        public static Dictionary<int, TTUSAPI.DataObjects.Market> m_Markets;

        #endregion

        public static void Start()
        {
            

            // Create the TTUSAPI object for a console application
            m_TTUSAPI = new TTUSApi(TTUSAPI.TTUSApi.StartupMode.Normal);

            //TT User Setup Server status
            m_TTUSAPI.OnConnectivityStatusUpdate += m_TTUSAPI_OnConnectivityStatusUpdate;
            //Login update
            m_TTUSAPI.OnLoginStatusUpdate += m_TTUSAPI_OnLoginStatusUpdate;
            //API initialization
            m_TTUSAPI.OnInitializeComplete += m_TTUSAPI_OnInitializeComplete;
        }

        public static void SubcribeForCallbacks()
        { 
            m_TTUSAPI.OnUserUpdate += m_TTUSAPI_OnUserUpdate;
            m_TTUSAPI.OnGatewayLoginUpdate += m_TTUSAPI_OnGatewayLoginUpdate;
            m_TTUSAPI.OnGatewayLoginReject += m_TTUSAPI_OnGatewayLoginReject;
            m_TTUSAPI.OnProductUpdate += m_TTUSAPI_OnProductUpdate;
            m_TTUSAPI.OnProductTypesDownload += m_TTUSAPI_OnProductTypesDownload;
            m_TTUSAPI.OnGatewayUpdate += m_TTUSAPI_OnGatewayUpdate;
            m_TTUSAPI.OnMarketsDownload += m_TTUSAPI_OnMarketsDownload;
            m_TTUSAPI.OnCurrencyUpdate += m_TTUSAPI_OnCurrencyUpdate;
            
        }
       
        public static void ShutDown()
        {
            m_TTUSAPI.Logoff();
            m_TTUSAPI.Dispose();
        }

        #region TTUS API Initialization Callbacks
        private static void m_TTUSAPI_OnConnectivityStatusUpdate(object sender, ConnectivityStatusEventArgs e)
        {
            ASG.Utility.DisplayCurrentMethodName();

            Trace.WriteLine(string.Format("ConnectivityStatus: {0}", e.ConnectivityStatus));
            Trace.WriteLine(string.Format("ServerInfo.Version: {0}", e.ServerInfo.Version));
            Trace.WriteLine(string.Format("ServerInfo.IpAddress: {0}", e.ServerInfo.IpAddress));
            Trace.WriteLine(string.Format("ServerInfo.MultiBrokerMode: {0}", e.ServerInfo.MultiBrokerMode));
            if (e.ConnectivityStatus.Equals(ConnectivityStatusType.Ready))
            {
                mainform.toolStripStatusLabel_Connected.BackColor = System.Drawing.Color.LimeGreen ;
                m_EnvironmentIsMB = e.ServerInfo.MultiBrokerMode;
                // Log in to the connected TT User Setup server
                m_TTUSAPI.Login(TTUS_User, password);
            }
        }

        private static void m_TTUSAPI_OnLoginStatusUpdate(object sender, LoginStatusEventArgs e)
        {
            ASG.Utility.DisplayCurrentMethodName();

            Trace.WriteLine(string.Format("LoginResultCode: {0}", e.LoginResultCode));
            if (e.IsSuccess)
            {
                Trace.WriteLine(string.Format("Message: {0}", e.Message));
                Trace.WriteLine(string.Format("SystemAccessMode: {0}", e.SystemAccessMode));
                Trace.WriteLine(string.Format("CompanyID: {0}", e.CompanyID));
                Trace.WriteLine(string.Format("UserGroupID: {0}", e.UserGroupID));

                //Initialize the API to get all of the User, Fix Adapter, Account, and MGT data
                m_TTUSAPI.Initialize();

            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Login Failed!");
                Trace.WriteLine("Login Failed");
                Environment.Exit(0);
            }
        }

        private static void m_TTUSAPI_OnInitializeComplete(object sender, InitializeCompleteEventArgs e)
        {
            ASG.Utility.DisplayCurrentMethodName();


            Trace.WriteLine(string.Format("AccountsDownloadCount: {0}", e.AccountsDownloadCount));
            Trace.WriteLine(string.Format("FixServersDownloadCount: {0}", e.FixServersDownloadCount));
            Trace.WriteLine(string.Format("GatewayLoginsDownloadCount: {0}", e.GatewayLoginsDownloadCount));
            Trace.WriteLine(string.Format("UsersDownloadCount: {0}", e.UsersDownloadCount));

            ResultStatus resultP = m_TTUSAPI.GetProducts();
            Trace.WriteLine(string.Format("GetProducts: {0} [{1}] {2}", resultP.Result,resultP.TransactionID, resultP.ErrorMessage));


        }        
        #endregion

        #region TTUSAPI Data updates

        static void m_TTUSAPI_OnProductTypesDownload(object sender, ProductTypeDownloadEventArgs e)
        {
            ASG.Utility.DisplayCurrentMethodName();
            m_ProdTypes = e.ProductTypes;
        }

        static void m_TTUSAPI_OnMarketsDownload(object sender, MarketDownloadEventArgs e)
        {
            ASG.Utility.DisplayCurrentMethodName();
            // VERIFIED: Market IDs are the same for same markets in SB/MB
            m_Markets = e.Markets;
        }

        // update global data in current application
        static void m_TTUSAPI_OnCurrencyUpdate(object sender, CurrencyUpdateEventArgs e)
        {
            ASG.Utility.DisplayCurrentMethodName();
            handleUpdateCallback(ref m_Currencies, e.Currencies, e.Type);
        }
        
        static void m_TTUSAPI_OnGatewayUpdate(object sender, GatewayUpdateEventArgs e)
        {
            ASG.Utility.DisplayCurrentMethodName();
            handleUpdateCallback(ref m_Gateways, e.Gateways, e.Type);
        }

        static void m_TTUSAPI_OnGatewayLoginUpdate(object sender, GatewayLoginUpdateEventArgs e)
        {
            ASG.Utility.DisplayCurrentMethodName();
            handleUpdateCallback(ref m_GatewayLogins, e.GatewayLogins, e.Type);
        }

        static void m_TTUSAPI_OnGatewayLoginReject(object sender, GatewayLoginRejectEventArgs e)
        {
            Trace.WriteLine(string.Format("ERROR: {0} {1}", e.GatewayLogin.Name, e.RejectMessage));
            mainform.listBox1.Items.Add(e.RejectMessage);
        }

        static void m_TTUSAPI_OnUserUpdate(object sender, UserUpdateEventArgs e)
        {
            ASG.Utility.DisplayCurrentMethodName();
            handleUpdateCallback(ref m_Users, e.Users, e.Type);
        }

        // update market catalogs
        static void m_TTUSAPI_OnProductUpdate(object sender, ProductUpdateEventArgs e)
        {
            ASG.Utility.DisplayCurrentMethodName();
            int i = -1;
            if (e.Type.Equals(UpdateType.Download))
            {
                m_MarketProductCatalogs = e.MarketProductCatalogs;
                Trace.WriteLine("MarketProductCatalogs downloaded");
                foreach (var item in e.MarketProductCatalogs)
                {
                    Trace.WriteLine(string.Format("{2}GatewayProductCatalogs key: {0} Market:{1}", item.Key, item.Value.Market.MarketName, Environment.NewLine));
                    foreach (var item2 in item.Value.GatewayProductCatalogs)
                    {
                        Trace.WriteLine(string.Format("{3}ProductCatalogs key: {0} GatewayID:{1} Name:{2}",
                            item2.Key, item2.Value.Gateway.GatewayID, item2.Value.Gateway.GatewayName, Environment.NewLine));
                        foreach (var item3 in item2.Value.ProductCatalogs)
                        {
                            Trace.WriteLine(string.Format("{2}key: {0} ProductType:{1}",
                                item3.Key, item3.Value.ProductType.ProductTypeName, Environment.NewLine));

                            foreach (var item4 in item3.Value.Products)
                            {
                                if (item4.Value.MarketID != i)
                                {
                                    i = item4.Value.MarketID;
                                    Trace.WriteLine(string.Format("key: {0} Product:{1} MarketID:{2}", item4.Key, item4.Value.Name, item4.Value.MarketID));
                                }
                                else
                                { Trace.Write("."); }
                            }
                            Trace.WriteLine(".");
                        }
                    }
                }
            }
            mainform.toolStripStatusLabelAPI.BackColor = System.Drawing.Color.LimeGreen;
            mainform.button1.Enabled = true;
            mainform.button_InsertExchangeTraders.Enabled = true;
        }
        #endregion

        #region Utility code

        public static int GetGatewayIDByName(string name)
        {
            foreach (int id in m_Gateways.Keys)
            {
                if (m_Gateways[id].GatewayName == name)
                    return id;
            }
            return -1;
        }

        public static bool GetGWLoginFromUsername(string user, 
            Dictionary<string, TTUSAPI.DataObjects.User> user_dict,
            ref TTUSAPI.DataObjects.GatewayLogin gw_login)
        {
            ASG.Utility.DisplayCurrentMethodName();

            TTUSAPI.DataObjects.User u;
            if (user_dict.ContainsKey(user))
            {
                u = user_dict[user];
                Trace.WriteLine(string.Format("User found: {0}", u.UserName));

                TTUSAPI.DataObjects.UserProfile up = new TTUSAPI.DataObjects.UserProfile(u);

                Dictionary<string, TTUSAPI.DataObjects.UserGatewayLogin> ugls = up.UserGatewayLogins;
                Trace.WriteLine(string.Format("{0} gateway logins", ugls.Values.Count));

                string member = string.Empty;
                string group = string.Empty;
                string trader = string.Empty;
                int id = -1;

                foreach (var item in ugls)
                {
                    Trace.WriteLine(string.Format("key: {0} name:{1} Creds:{2}", item.Key, item.Value.GatewayLoginName, item.Value.UserGatewayCredentials.Count));
                    foreach (var item2 in item.Value.UserGatewayCredentials)
                    {
                        Trace.WriteLine(string.Format("key: {0} MGT: {1} {2} {3} gwID:{4}",
                            item2.Key, item2.Value.Member, item2.Value.Group, item2.Value.Trader, item2.Value.GatewayID));
                        if (m_Gateways.ContainsKey(item2.Value.GatewayID))
                        {
                            Trace.WriteLine(string.Format("{0}", m_Gateways[item2.Value.GatewayID].GatewayName));

                            member = item2.Value.Member;
                            group = item2.Value.Group;
                            trader = item2.Value.Trader;
                            id = item2.Value.GatewayID;
                        }
                    }
                }

                try
                {
                    string MGT = string.Format("{0} {1} {2}", member, group, trader);
                    gw_login = m_GatewayLogins[MGT];
                    Trace.WriteLine(string.Format("Gateway Login updated with {0}", MGT));
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    return false;
                }
                if (id != -1)
                { return true; }
                else { return false; }
            }
            else { return false; }
        }

        #endregion

        private static void handleUpdateCallback<T, U>(ref Dictionary<T, U> globalDictionary, Dictionary<T, U> eventDictionary, UpdateType type)
        {
            if (type == UpdateType.Download)
            {
                globalDictionary = eventDictionary;
                Trace.WriteLine(string.Format("Data Downloaded: {0} items",eventDictionary.Count));
            }
            else
            {
                foreach (KeyValuePair<T, U> item in eventDictionary)
                {
                    if (type == UpdateType.Added || type == UpdateType.Changed || type == UpdateType.Relationship)
                    {
                        globalDictionary[item.Key] = item.Value;
                        Trace.WriteLine(string.Format("{0} {1}", item.Key, type.ToString()));
                    }
                    else if (type == UpdateType.Deleted)
                    {
                        globalDictionary.Remove(item.Key);
                        Trace.WriteLine(string.Format("{0} deleted.", item.Key));
                    }
                    else
                    {
                        Trace.WriteLine(String.Format("Unknown update type ({0}) for key ({1})", type.ToString(), item.Key));
                    }
                }

            }
        }

        #region app code
        
        public static int success = 0;
        public static int failed = 0;
        public static void insertProductLimits(TTUSAPI.DataObjects.GatewayLogin gwl, TTUSAPI.DataObjects.GatewayLoginProductLimitProfile plp)
        {

            TTUSAPI.DataObjects.GatewayLoginProfile gwp = new TTUSAPI.DataObjects.GatewayLoginProfile(gwl);

            gwp.AddProductLimit(plp);

            ResultStatus r = m_TTUSAPI.UpdateGatewayLogin(gwp);
            Trace.WriteLine(string.Format("RESULT: {0} [{1}] {2} {3}", r.Result, r.TransactionID, r.ErrorMessage, plp.Product));

            if (r.Result.Equals(TTUSAPI.ResultType.SentToServer))
            { success++; }
            else if (r.Result.Equals(ResultType.FailedValidation))
            { failed++; }

        }



        public static void CleanProductLimits(
            Dictionary<string, TTUSAPI.DataObjects.GatewayLoginProductLimit> pl_dict,
            string new_gateway,
            List<string> gw_list,
            ref List<TTUSAPI.DataObjects.GatewayLoginProductLimitProfile> limits2copy) 
        {
            ASG.Utility.DisplayCurrentMethodName();
            limits2copy.Clear();

            string gwName = string.Empty;
            foreach (var kvp in pl_dict)
            {
                try
                {
                    if (gw_list.Contains(m_Gateways[kvp.Value.GatewayID].GatewayName))
                    {
                        TTUSAPI.DataObjects.GatewayLoginProductLimitProfile plp = new TTUSAPI.DataObjects.GatewayLoginProductLimitProfile(kvp.Value);
                        plp.GatewayID = GetGatewayIDByName(new_gateway);
                        limits2copy.Add(plp);
                        
                        Trace.WriteLine(string.Format("GW1: {0} GW2: {1} newID:{2}",
                            m_Gateways[kvp.Value.GatewayID].GatewayName,
                            new_gateway,
                            plp.GatewayID));
                    }
                    else
                    {
                        Trace.WriteLine(string.Format("Gateway {0} not to be consolidated", m_Gateways[kvp.Value.GatewayID].GatewayName));
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine(string.Format("{0}", e.Message));
                }
            }
            Trace.WriteLine(string.Format("{0} limits to add", limits2copy.Count));
        }

        public static void UploadLimits(
            List<TTUSAPI.DataObjects.GatewayLoginProductLimitProfile> limits,
            TTUSAPI.DataObjects.GatewayLogin gwlogin)
        {
            ASG.Utility.DisplayCurrentMethodName();
            TTUS.success = 0;
            TTUS.failed = 0;
            foreach (TTUSAPI.DataObjects.GatewayLoginProductLimitProfile plp in limits)
            {
                insertProductLimits(gwlogin, plp);
            }
            Trace.WriteLine(string.Format("{0} Limits sent to server", success));
            Trace.WriteLine(string.Format("{0} Limits failed", failed));
            Trace.WriteLine(string.Format("{0} total processed", failed + success));
        }

        #endregion
    }
}
