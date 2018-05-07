using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.IO.Compression;
using System.IO;
using log4net;
using log4net.Config;
using System.Reflection;
using System.Diagnostics;
using System.Linq;

namespace PathTracker_Backend
{
    public class RequestCoordinator {

        private SettingsManager Settings = SettingsManager.Instance;
        private static readonly ILog RequestCoordinatorLog = log4net.LogManager.GetLogger(LogManager.GetRepository(Assembly.GetEntryAssembly()).Name, "RequestCoordinatorLogger");
        private Dictionary<string, List<StashTab>> LeagueStashtabDictionary = new Dictionary<string, List<StashTab>>();

        public RequestCoordinator() {

            log4net.GlobalContext.Properties["RequestCoordinatorLogFileName"] = Directory.GetCurrentDirectory() + "//Logs//RequestCoordinatorLog";
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }

        /// <summary>
        /// Return a JSON string representing the Inventory object 
        /// </summary>
        /// <param name="currentChar">Character to get inventory for. If left blank, the CurrenctCharacter from Settings is used</param>
        /// <returns></returns>
        public Inventory GetInventory(string currentChar="") {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            string account = Settings.GetValue("Account");
            if(currentChar == "") {
                currentChar = Settings.GetValue("CurrentCharacter");
            }
            string SessID = Settings.GetValue("POESESSID");
            
            string apiEndpoint = $"https://www.pathofexile.com/character-window/get-items?character=" + currentChar + "&accountName=" + account;
            
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiEndpoint);

            CookieContainer cont = new CookieContainer();
            webRequest.CookieContainer = cont;

            Cookie cookiePOESESSID = new Cookie("POESESSID", SessID, "/", ".pathofexile.com");
            Cookie cookieStoredData = new Cookie("stored_data", "1", "/", ".pathofexile.com");
            webRequest.CookieContainer.Add(cookiePOESESSID);
            webRequest.CookieContainer.Add(cookieStoredData);

            webRequest.Headers.Add("Accept-Encoding", "gzip,deflate");

            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

            LogHeader(webResponse.Headers);
            string responseDecompressed = DecompressToString(webResponse);
            Inventory deserialized = JsonConvert.DeserializeObject<Inventory>(responseDecompressed);

            RequestCoordinatorLog.Info("Finnished inventory request (Account:" + account + ",character: " + currentChar + ") request in " + timer.ElapsedMilliseconds + "ms");


            return deserialized; 
        }

        /// <summary>
        /// Function to get stash tab by name and league. Assumes that the stash tab name is unique within the league
        /// </summary>
        /// <param name="name">Name of the stash tab</param>
        /// <param name="league">Name of the league</param>
        /// <param name="initializeTabs">This value is only internal, and should not be set</param>
        /// <returns></returns>
        public StashApiRequest GetStashtab(string name, string league= "", bool initializeTabs=false) {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            string fetchLeague = league;
            if (league == "") {
                fetchLeague = Settings.GetValue("CurrentLeague");
            }
            string SessID = Settings.GetValue("POESESSID");
            string account = Settings.GetValue("Account");

            if(!(LeagueStashtabDictionary.ContainsKey(fetchLeague)) && initializeTabs==false) {
                GetStashtab(name: name, league: fetchLeague, initializeTabs: true);
            }

            int tabIndex = 0;
            if(initializeTabs == false) {
                if (LeagueStashtabDictionary.ContainsKey(fetchLeague)) {
                    List<StashTab> tabs = LeagueStashtabDictionary[fetchLeague];
                    tabIndex = tabs.Single(x => x.Name == name).Index;
                }
                else {
                    throw new Exception("Should never happen... Dave");
                }
            }
            
            string tabIndexString = "";
            if (initializeTabs==false) {
                tabIndexString = "&tabIndex=" + tabIndex.ToString();
            }
            
            string _apiEndpoint = $"https://www.pathofexile.com/character-window/get-stash-items?league=" + fetchLeague + "&accountName=" + account + "&tabs=1" + tabIndexString;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(_apiEndpoint);
            
            CookieContainer cont = new CookieContainer();
            Cookie cookie = new Cookie("POESESSID", SessID, "/", ".pathofexile.com");
            Cookie cookie1 = new Cookie("stored_data", "1", "/", ".pathofexile.com");
            webRequest.CookieContainer = cont;
            webRequest.CookieContainer.Add(cookie);
            webRequest.CookieContainer.Add(cookie1);
            
            webRequest.Headers.Add("Accept-Encoding", "gzip,deflate");

            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

            LogHeader(webResponse.Headers);
            string decompressedStashTab = DecompressToString(webResponse);

            StashApiRequest apiRequest = JsonConvert.DeserializeObject<StashApiRequest>(decompressedStashTab);
            
            RequestCoordinatorLog.Info("Finnished stash tab (Account:"+account+",name: "+ name + ",league: " + league +") request in " + timer.ElapsedMilliseconds + "ms");

            //Verify that the selected tab has the correct name. If the tab was moved, the cached index will be wrong and needs correcting.
            if (initializeTabs) {
                LeagueStashtabDictionary[fetchLeague] = apiRequest.StashTabs;
                return null;
            }
            else {
                int newIndex = apiRequest.StashTabs.Single(x => x.Name == name).Index;
                if (newIndex != tabIndex) {
                    int oldIndex = LeagueStashtabDictionary[fetchLeague].Single(x => x.Name == name).Index;
                    RequestCoordinatorLog.Info("Requesting stash tab " + name + " again. Index changed from " + oldIndex.ToString() + " to " + newIndex.ToString());
                    LeagueStashtabDictionary[fetchLeague] = apiRequest.StashTabs;
                    apiRequest = GetStashtab(name: name, league: fetchLeague);
                }
            }
            

            return apiRequest;
        }
        
        private string DecompressToString(WebResponse response) {
            byte[] decompFile = null;
            using (Stream stream = response.GetResponseStream()) {
                byte[] tmpArr = ReadFully(stream);
                Stream stream1 = new MemoryStream(tmpArr);
                using (GZipStream decompStream = new GZipStream(stream1, CompressionMode.Decompress)) {
                    decompFile = ReadFully(decompStream);
                }
            }
            return System.Text.Encoding.Default.GetString(decompFile);
        }

        private void LogHeader(WebHeaderCollection headers) {

            string[] xRateLimitAccount = { "X-Rate-Limit-Account" };
            string[] xRateLimitAccountState = { "X-Rate-Limit-Account-State" };
            for (int i = 0; i < headers.Count; ++i) {
                string header = headers.GetKey(i);
                foreach (string value in headers.GetValues(i)) {

                    if (header == "X-Rate-Limit-Account") {
                        xRateLimitAccount = value.Split(',');
                    }
                    else if (header == "X-Rate-Limit-Account-State") {
                        xRateLimitAccountState = value.Split(',');
                    }
                }
            }

            string rateLimit = "";
            for (int i = 0; i < xRateLimitAccount.Length; i++) {
                rateLimit = rateLimit + xRateLimitAccountState[i] + " " + xRateLimitAccount[i];
                if (i < xRateLimitAccount.Length - 1) {
                    rateLimit = rateLimit + " | ";
                }
            }

            RequestCoordinatorLog.Info(rateLimit);
        }

        private static byte[] ReadFully(Stream stream, int bufferSize = 32768) {
            byte[] buffer = new byte[bufferSize];
            using (MemoryStream ms = new MemoryStream()) {
                while (true) {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }
        
    }
}
