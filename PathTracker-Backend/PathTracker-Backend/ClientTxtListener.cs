using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using log4net;
using System.IO;
using System.Text.RegularExpressions;
using log4net.Config;
using System.Threading.Tasks;
using System.Threading;

namespace PathTracker_Backend {
    public class ClientTxtListener : IListener {

        private string _clientTxtPath = "";
        public string ClientTxtPath {
            get { return _clientTxtPath; }
            set {
                if (!File.Exists(value)) {
                    throw new Exception("When setting ClientTxtPath, could not find file:" + value);
                }
                _clientTxtPath = value;
            }
        }

        public int MsListenDelay;
        private Stopwatch ListenTimer = new Stopwatch();
        private SettingsManager Settings = SettingsManager.Instance;
        private static readonly ILog ClientTxtLog = log4net.LogManager.GetLogger(LogManager.GetRepository(Assembly.GetEntryAssembly()).Name, "ClientTxtLogger");
        private ZoneManager zoneManager;
        
        public ClientTxtListener(ZoneManager paramZoneManager) {
            log4net.GlobalContext.Properties["ClientTxtLogFileName"] = Directory.GetCurrentDirectory() + "//Logs//ClientTxtLog";
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            
            MsListenDelay = 1000;
            ClientTxtPath = Settings.GetValue("ClientTxtPath");
            ClientTxtLog.Info("Client.txt file set to path: " + ClientTxtPath);

            zoneManager = paramZoneManager;
        }
        
        public void StartListening() {

            ClientTxtLog.Info("Starting listener");
            ListenTimer.Start();
            using (FileStream stream = File.Open(ClientTxtPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                long numBytes = stream.Length;

                while (true) {
                    if (ListenTimer.ElapsedMilliseconds >= MsListenDelay) {

                        long currentBytes = stream.Length;
                        stream.Position = numBytes;

                        int numNewBytes = (int)(currentBytes - numBytes);
                        byte[] newBytes = new byte[numNewBytes];
                        string newText = "";
                        if (numNewBytes > 0) {
                            stream.Read(newBytes, 0, numNewBytes);
                            newText = System.Text.Encoding.Default.GetString(newBytes);
                            ClientTxtLog.Info(newBytes.Length.ToString() + " new bytes in " + ClientTxtPath);
                        }

                        numBytes = stream.Length;

                        ListenTimer.Restart();

                        if(numNewBytes > 0) {
                            ParseClientText(newText);
                        }
                        
                    }
                    else {
                        System.Threading.Thread.Sleep(MsListenDelay - (int)ListenTimer.ElapsedMilliseconds);
                    }
                }

            }

        }

        public void StopListening() {
            throw new NotImplementedException();
        }


        private void ParseClientText(string newText) {
            string[] newLines = newText.Split('\n');

            ClientTxtLog.Info("Parsing " + newLines.Length + " lines in " + ClientTxtPath);

            foreach (string line in newLines) {
                string tst = line;
                ParseLine(line);
            }
        }

        //Matches pattern: 2018/05/07 17:22:04 113950185 9b0 [INFO Client 13026] : You have entered CAPTUREDZONE
        private static string ZoneEnterPattern = @"^[0-9]{4}\/[0-9]{2}\/[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2} [0-9]* [a-zA-Z0-9]* \[INFO Client [0-9]*\] : You have entered ([a-zA-Z0-9 '-]*)";
        private Regex ZoneRegex = new Regex(ZoneEnterPattern);
        private void ParseLine(string line) {
            Match zoneMatch = ZoneRegex.Match(line);

            if (zoneMatch.Success && zoneMatch.Groups.Count > 1) {
                if (zoneMatch.Groups[1].Captures.Count > 0) {
                    string zoneName = zoneMatch.Groups[1].Captures[0].ToString();
                    ClientTxtLog.Info("Entered: " + zoneName);

                    zoneManager.ZoneEntered(zoneName);
                }
            }
        }
    }
    

}
