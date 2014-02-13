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

    static class TTUS_API
    {
        #region Internal TTUS Data storage
        public static TTUSApi m_TTUSAPI;
        public static string TTUS_User;
        public static string password;

        public static TTUS_Migration.Form1 mainform;
        public static bool m_EnvironmentIsMB = false ;

        public static string m_Username;
        
        public static TTUSAPI.DataObjects.GatewayLogin m_UpdateGWLogin;

        //this contains all users in the currently connected environment Key = username
        public static Dictionary<string, TTUSAPI.DataObjects.User> m_Users;

        //this contains all gateway logins in current env Key = MGT separated by spaces
        public static Dictionary<string, TTUSAPI.DataObjects.GatewayLogin> m_GatewayLogins;

        //this contains all gateways in the currently connected env key = int id the is the same for all environments
        public static Dictionary<int, TTUSAPI.DataObjects.Gateway> m_Gateways;

        public static Dictionary<int, TTUSAPI.DataObjects.MarketProductCatalog> m_MarketProductCatalogs;
        public static Dictionary<int, TTUSAPI.DataObjects.Market> m_Markets;

        // this data is retreived from single 
        public static Dictionary<string, TTUSAPI.DataObjects.GatewayLoginProductLimit> m_SBGatewayLoginProductLimits;
        public static Dictionary<int, TTUSAPI.DataObjects.Gateway> m_GatewaysSingleBroker;
        public static List<TTUSAPI.DataObjects.GatewayLoginProductLimitProfile> m_GWRiskLimits = new List<TTUSAPI.DataObjects.GatewayLoginProductLimitProfile>();
        public static List<TTUSAPI.DataObjects.GatewayLoginProductLimitProfile> m_NAGWRiskLimits = new List<TTUSAPI.DataObjects.GatewayLoginProductLimitProfile>();

        //populated through config file.
        public static Dictionary<string, string> SB2MBGWMap = new Dictionary<string, string>();

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
            m_TTUSAPI.OnProductUpdate += m_TTUSAPI_OnProductUpdate;
            m_TTUSAPI.OnProductTypesDownload += m_TTUSAPI_OnProductTypesDownload;
            m_TTUSAPI.OnGatewayUpdate += m_TTUSAPI_OnGatewayUpdate;
            m_TTUSAPI.OnMarketsDownload += m_TTUSAPI_OnMarketsDownload;
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
            mainform.toolStripStatusLabelAPI.BackColor = System.Drawing.Color.LimeGreen;
            if (!m_EnvironmentIsMB)
            {
                //mainform.button_SBLimits2Disk.Enabled = true;
            }
            else
            {
               // mainform.button_InsertLimitsToMB.Enabled = true;
            }

            Trace.WriteLine(string.Format("AccountsDownloadCount: {0}", e.AccountsDownloadCount));
            Trace.WriteLine(string.Format("FixServersDownloadCount: {0}", e.FixServersDownloadCount));
            Trace.WriteLine(string.Format("GatewayLoginsDownloadCount: {0}", e.GatewayLoginsDownloadCount));
            Trace.WriteLine(string.Format("UsersDownloadCount: {0}", e.UsersDownloadCount));

            ResultStatus resultP = m_TTUSAPI.GetProducts();
            Trace.WriteLine(string.Format("GetProducts: {0} [{1}] {2}", resultP.Result,resultP.TransactionID, resultP.ErrorMessage));

        }        
        #endregion

        #region TTUSAPI Data updates
        static void m_TTUSAPI_OnUserUpdate(object sender, UserUpdateEventArgs e)
        {
            ASG.Utility.DisplayCurrentMethodName();
            try
            {
                Trace.WriteLine("There were " + e.Users.Count + " Users downloaded:");
                //Populate dictionary with downloaded users
                if (e.Type == UpdateType.Download)
                {
                    m_Users = e.Users;
                }
                //Update dictionary with any user updates
                else if (e.Type == UpdateType.Added || e.Type == UpdateType.Changed || e.Type == UpdateType.Relationship)
                {
                    foreach (KeyValuePair<string, TTUSAPI.DataObjects.User> user in e.Users)
                    {
                        m_Users[user.Key] = user.Value;
                    }
                }
                //Remove user from dictionary
                else if (e.Type == UpdateType.Deleted)
                {
                    foreach (KeyValuePair<string, TTUSAPI.DataObjects.User> user in e.Users)
                    {
                        m_Users.Remove(user.Key);
                    }
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine("Error:" + exception.Message);
            }

            //try
            //{
            //    // THIS is left in as a reference for what was working while testing
            //    m_UpdateUser = m_Users["ANTONIOS3"];
            //    Trace.WriteLine("User: " + m_Users["ANTONIOS3"].UserName );
            //}
            //catch (Exception ex)
            //{
            //    Trace.WriteLine("User not found");
            //    Trace.WriteLine(ex.Message ); 
            //}
        }

        static void m_TTUSAPI_OnGatewayLoginUpdate(object sender, GatewayLoginUpdateEventArgs e)
        {
            ASG.Utility.DisplayCurrentMethodName();
            try
            {
                if (e.Type == UpdateType.Download)
                {
                    Trace.WriteLine("There were " + e.GatewayLogins.Count + " GatewayLogins downloaded:");
                    m_GatewayLogins = e.GatewayLogins;
                }
                //Update dictionary with any user updates
                else if (e.Type == UpdateType.Added || e.Type == UpdateType.Changed || e.Type == UpdateType.Relationship)
                {
                    Trace.WriteLine("There were " + e.GatewayLogins.Count + " GatewayLogins updated:");
                    foreach (KeyValuePair<string, TTUSAPI.DataObjects.GatewayLogin> user in e.GatewayLogins)
                    {
                        m_GatewayLogins[user.Key] = user.Value;
                        
                    }
                }
                //Remove user from dictionary
                else if (e.Type == UpdateType.Deleted)
                {
                    Trace.WriteLine("There were " + e.GatewayLogins.Count + " GatewayLogins deleted:");
                    foreach (KeyValuePair<string, TTUSAPI.DataObjects.GatewayLogin> user in e.GatewayLogins)
                    {
                        m_GatewayLogins.Remove(user.Key);
                    }
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine("Error:" + exception.Message);
            }
        }

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
                            item2.Key, item2.Value.Gateway.GatewayID, item2.Value.Gateway.GatewayName, Environment.NewLine ));
                        foreach (var item3 in item2.Value.ProductCatalogs)
                        {
                            Trace.WriteLine(string.Format("{2}key: {0} ProductType:{1}",
                                item3.Key, item3.Value.ProductType.ProductTypeName, Environment.NewLine));
                            
                            foreach (var item4 in item3.Value.Products)
                            {
                                if (item4.Value.MarketID!=i)
                                {
                                    i = item4.Value.MarketID;
                                    Trace.WriteLine(string.Format("key: {0} Product:{1} MarketID:{2}", item4.Key, item4.Value.Name, item4.Value.MarketID));
                                }
                                else
                                { Trace.Write("."); }
                            }
                        }
                    }
	            }
            }
        }

        static void m_TTUSAPI_OnProductTypesDownload(object sender, ProductTypeDownloadEventArgs e)
        {
            ASG.Utility.DisplayCurrentMethodName();
            //foreach (TTUSAPI.DataObjects.ProductType pt in e.ProductTypes.Values)
            //{
            //    Trace.WriteLine(string.Format("{0} : {1}",pt.ProductTypeID, pt.ProductTypeName));
            //}
        }

        static void m_TTUSAPI_OnGatewayUpdate(object sender, GatewayUpdateEventArgs e)
        {
            ASG.Utility.DisplayCurrentMethodName();

            if (e.Type.Equals(UpdateType.Download))
            {
                m_Gateways = e.Gateways;
                if (!m_EnvironmentIsMB)
                {
                    m_GatewaysSingleBroker = e.Gateways;
                }
            }

            //foreach (var kvp in e.Gateways)
            //{
            //    Trace.WriteLine(string.Format("id:{0} Gateway:{1} {2}", kvp.Value.GatewayID, kvp.Value.GatewayName, kvp.Value.MarketID ));
            //}
        }

        static void m_TTUSAPI_OnMarketsDownload(object sender, MarketDownloadEventArgs e)
        {
            ASG.Utility.DisplayCurrentMethodName();
            // VERIFIED: Market IDs are the same for same markets in SB/MB
            m_Markets = e.Markets;

            //foreach (var kvp in e.Markets)
            //{ 
            //    Trace.WriteLine(string.Format("key: {0} Name: {1} {2} ({3})",kvp.Key,kvp.Value.MarketName,kvp.Value.MarketType ,kvp.Value.MarketID ));
            //}
        }
        #endregion

        #region Data To-From Disk

        public static void MapGateways(string fn)
        {
            Utility.ReadDictFromFile(fn, ref SB2MBGWMap);
        }

        public static void CaptureLimitsToDisk(Dictionary<string, TTUSAPI.DataObjects.GatewayLoginProductLimit> limits, string fn)
        {
            ASG.Utility.DisplayCurrentMethodName();
            AppLogic.NumberofLimits = limits.Count; 

            string d = AppLogic.DataDir ;

            if (Utility.VerifyDirectory(d))
            {
                //http://msdn.microsoft.com/en-us/library/system.runtime.serialization.datacontractserializer.aspx
                DataContractSerializer dcs = new DataContractSerializer( typeof(Dictionary<string, TTUSAPI.DataObjects.GatewayLoginProductLimit>));
                FileStream writer = new FileStream(d + "\\" + fn + ".XML", FileMode.Create);
                dcs.WriteObject(writer, limits);
                writer.Close();
            }
        }
        
        public static void CaptureGatewaysToDisk(Dictionary<int, TTUSAPI.DataObjects.Gateway> gws, string fn)
        {
            ASG.Utility.DisplayCurrentMethodName();
            string d = AppLogic.DataDir + "\\gateways";

            if (Utility.VerifyDirectory(d))
            {
                //http://msdn.microsoft.com/en-us/library/system.runtime.serialization.datacontractserializer.aspx
                DataContractSerializer dcs = new DataContractSerializer(typeof(Dictionary<int, TTUSAPI.DataObjects.Gateway>));
                FileStream writer = new FileStream(d + "\\" + fn + ".XML", FileMode.Create);
                dcs.WriteObject(writer, gws);
                writer.Close();
            }
        }

        public static Dictionary<string, TTUSAPI.DataObjects.GatewayLoginProductLimit> RetreiveLimitsFromDisk(string filename)
        {
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Open);
                Trace.WriteLine(string.Format("Deserializing {0}", fs.Name));
                XmlDictionaryReader reader =
                    XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                DataContractSerializer ser = new DataContractSerializer(typeof(Dictionary<string, TTUSAPI.DataObjects.GatewayLoginProductLimit>));

                // Deserialize the data and read it from the instance.
                Dictionary<string, TTUSAPI.DataObjects.GatewayLoginProductLimit> deserializedGWProdLimits =
                    (Dictionary<string, TTUSAPI.DataObjects.GatewayLoginProductLimit>)ser.ReadObject(reader, true);
                reader.Close();
                fs.Close();
                Trace.WriteLine(string.Format("Deserialized {0} limits", deserializedGWProdLimits.Count));
                //Console.WriteLine(String.Format("{0} {1}, ID: {2}",
                //deserializedPerson.FirstName, deserializedPerson.LastName,
                //deserializedPerson.ID));

                return deserializedGWProdLimits;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return new Dictionary<string, TTUSAPI.DataObjects.GatewayLoginProductLimit>(); 
            }
        }

        public static Dictionary<int, TTUSAPI.DataObjects.Gateway> RetreiveSBGWFromDisk(string filename)
        {
            string d = AppLogic.DataDir + "\\gateways"; 

            if (Utility.VerifyDirectory(d))
            {
                FileStream fs = new FileStream(d + "\\" + filename, FileMode.Open);
                Trace.WriteLine(string.Format("Deserializing {0}", fs.Name));
                XmlDictionaryReader reader =
                    XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                DataContractSerializer ser = new DataContractSerializer(typeof(Dictionary<int, TTUSAPI.DataObjects.Gateway>));

                // Deserialize the data and read it from the instance.
                Dictionary<int, TTUSAPI.DataObjects.Gateway> deserializedGWList =
                    (Dictionary<int, TTUSAPI.DataObjects.Gateway>)ser.ReadObject(reader, true);
                reader.Close();
                fs.Close();
                Trace.WriteLine(string.Format("Deserialized {0} SBGW entries", deserializedGWList.Count));
                //Console.WriteLine(String.Format("{0} {1}, ID: {2}",
                //deserializedPerson.FirstName, deserializedPerson.LastName,
                //deserializedPerson.ID));

                return deserializedGWList;
            }
            else
            {
                Trace.WriteLine("Data Export directory is not found");
                return new Dictionary<int, TTUSAPI.DataObjects.Gateway>();
            }
        }


        #endregion

        #region Utility code
        public static void ListLimits(Dictionary<string, TTUSAPI.DataObjects.GatewayLoginProductLimit> dict)
        {
            foreach (KeyValuePair<string,TTUSAPI.DataObjects.GatewayLoginProductLimit> kvp in dict)
            {
                Trace.WriteLine(string.Format("{0} : {1} - {2}",kvp.Key, kvp.Value.ProductKey, kvp.Value.Product));
            }
        }

        public static int GetGatewayIDbyMarketID(int marketID)
        {
            foreach (int key in m_Gateways.Keys)
            {
                if (m_Gateways[key].MarketID == marketID)
                {
                    if (m_Gateways[key].GatewayName.Contains("-Q"))
                    return m_Gateways[key].GatewayID;
                }
            }

            return -1;
        }
        #endregion

        #region Insert GatewayLoginProductLimit
        static TTUSAPI.DataObjects.GatewayLoginProductLimitProfile test_product()
        {
            // THIS code used only for development testing
            TTUSAPI.DataObjects.GatewayLoginProductLimitProfile plp = new TTUSAPI.DataObjects.GatewayLoginProductLimitProfile();

            plp.AdditionalMarginPercent = 0.00;
            plp.AllowTradeOut = true;
            plp.GatewayID = 690; 
            plp.MaxLongShort = 1000;
            plp.MaxOrderQty = 1000;
            plp.MaxPosition = 1000;
            plp.Product = "6B";
            //plp.ProductKey is read only
            plp.ProductTypeID = 1; 
            plp.SimulationOnly = false;

            return plp;
        }

        public static int success = 0;
        public static int failed = 0;
        public static void insertProductLimits(TTUSAPI.DataObjects.GatewayLogin gwl, TTUSAPI.DataObjects.GatewayLoginProductLimitProfile plp)
        {
           
            TTUSAPI.DataObjects.GatewayLoginProfile gwp = new TTUSAPI.DataObjects.GatewayLoginProfile(gwl);
            
            gwp.AddProductLimit(plp);

            ResultStatus r = m_TTUSAPI.UpdateGatewayLogin(gwp);
            Trace.WriteLine(string.Format("RESULT: {0} [{1}] {2} {3}",r.Result, r.TransactionID, r.ErrorMessage, plp.Product));

            if (r.Result.Equals(TTUSAPI.ResultType.SentToServer))
            { success++; }
            else if (r.Result.Equals(ResultType.FailedValidation))
            { failed++; }
           
        }

        public static bool RetreiveGWLoginFromUser(string user)
        {
            ASG.Utility.DisplayCurrentMethodName();

            TTUSAPI.DataObjects.User u;
            try
            {
                u = m_Users[user];
                Trace.WriteLine(string.Format("User found: {0}", u.UserName));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return false;
            }

            TTUSAPI.DataObjects.UserProfile up = new TTUSAPI.DataObjects.UserProfile(u);

            Dictionary<string, TTUSAPI.DataObjects.UserGatewayLogin> ugls = up.UserGatewayLogins;
            string member = string.Empty;
            string group = string.Empty;
            string trader = string.Empty;
            int id = -1;

            foreach (var item in ugls)
            {
                //Trace.WriteLine(string.Format("", item.Key, item.Value.GatewayLoginName, item.Value.UserGatewayCredentials.Count));
                foreach (var item2 in item.Value.UserGatewayCredentials)
                {
                    Trace.WriteLine(string.Format("key: {0} MGT: {1} {2} {3} gwID:{4}",
                        item2.Key, item2.Value.Member, item2.Value.Group, item2.Value.Trader, item2.Value.GatewayID));
                    if (m_GatewaysSingleBroker.ContainsKey(item2.Value.GatewayID))
                    {
                        Trace.WriteLine(string.Format("{0}", m_GatewaysSingleBroker[item2.Value.GatewayID].GatewayName));

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
                m_UpdateGWLogin = m_GatewayLogins[MGT];
                Trace.WriteLine(string.Format("m_UpdateGWLogin updated with {0}",MGT));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return false;
            }
            return true;
        }
        
        public static void CleanProductLimits()
        {
            ASG.Utility.DisplayCurrentMethodName();
            m_GWRiskLimits.Clear();

            string gwName = string.Empty;
            foreach (var kvp in m_SBGatewayLoginProductLimits)
            {
                try
                {
                    if (SB2MBGWMap.ContainsKey(m_GatewaysSingleBroker[kvp.Value.GatewayID].GatewayName))
                    {
                        TTUSAPI.DataObjects.GatewayLoginProductLimitProfile plp = new TTUSAPI.DataObjects.GatewayLoginProductLimitProfile(kvp.Value);
                        plp.GatewayID = GetGatewayIDByname(SB2MBGWMap[m_GatewaysSingleBroker[kvp.Value.GatewayID].GatewayName]);
                        m_GWRiskLimits.Add(plp);
                        Trace.WriteLine(string.Format("SBGW: {0} MBGW: {1} newID:{2}", 
                            m_GatewaysSingleBroker[kvp.Value.GatewayID].GatewayName,
                            SB2MBGWMap[ m_GatewaysSingleBroker[kvp.Value.GatewayID].GatewayName],
                            plp.GatewayID ));
                    }
                    else
                    {
                        Trace.WriteLine(string.Format("SB Gateway {0} not mapped to MB environment", m_GatewaysSingleBroker[kvp.Value.GatewayID].GatewayName));
                        m_NAGWRiskLimits.Add(new TTUSAPI.DataObjects.GatewayLoginProductLimitProfile(kvp.Value));
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine(string.Format("{0}", e.Message));
                }
            }
            Trace.WriteLine(string.Format("{0} limits to add", m_GWRiskLimits.Count));
            Trace.WriteLine(string.Format("{0} limits from unavailable markets", m_NAGWRiskLimits.Count));
        }
        
        public static void UploadLimits() 
        {
            ASG.Utility.DisplayCurrentMethodName();
            TTUS_API.success = 0;
            TTUS_API.failed = 0;
            foreach (TTUSAPI.DataObjects.GatewayLoginProductLimitProfile plp in m_GWRiskLimits)
            {
                insertProductLimits(m_UpdateGWLogin, plp);
            }
            Trace.WriteLine(string.Format("{0} Limits sent to server", success));
            Trace.WriteLine(string.Format("{0} Limits failed", failed));
            Trace.WriteLine(string.Format("{0} total processed", failed + success));
            AppLogic.NumberofLimits = success;
        }

        private static int GetGatewayIDByname(string name)
        {
            foreach (int id in m_Gateways.Keys)
            {
                if (m_Gateways[id].GatewayName == name)
                    return id;
            }
            return -1;
        }

        #endregion

        public static void ShutDown()
        {
            m_TTUSAPI.Logoff();
            m_TTUSAPI.Dispose();
        }
    }
}
