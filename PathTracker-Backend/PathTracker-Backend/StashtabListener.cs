using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;
using log4net;
using System.Reflection;
using log4net.Config;
using System.Linq;
using System.Collections;

namespace PathTracker_Backend {
    public class StashtabListener {

        private int MsListenDelay;
        private Stopwatch ListenTimer = new Stopwatch();
        private RequestCoordinator Coordinator;
        private string StashName = "";
        private List<Item> CurrentStashItems = null;
        private SettingsManager Settings = SettingsManager.Instance;
        private static readonly ILog StashtabLog = log4net.LogManager.GetLogger(LogManager.GetRepository(Assembly.GetEntryAssembly()).Name, "StashtabLogger");

        public StashtabListener(string stashName, int msListenDelay, RequestCoordinator coordinator) {
            MsListenDelay = msListenDelay;
            Coordinator = coordinator;
            StashName = stashName;
            
            log4net.GlobalContext.Properties["StashtabLogFileName"] = Directory.GetCurrentDirectory() + "//Logs//StashtabLog";
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }

        public void StartListening() {

            StashApiRequest currentStashTab = Coordinator.GetStashtab(StashName);
            CurrentStashItems = currentStashTab.Items;

            ListenTimer.Start();

            while (true) {

                if (ListenTimer.ElapsedMilliseconds >= MsListenDelay) {
                    StashApiRequest newStashTab = Coordinator.GetStashtab(StashName);
                    ListenTimer.Restart();
                    
                    (List<Item > added, List<Item > removed) = Toolbox.ItemDiffer(CurrentStashItems, newStashTab.Items);

                    string logAdded = "Added - ";
                    string logRemoved = "Removed - ";

                    foreach (Item item in added) {
                        logAdded = logAdded + item.Name + " " + item.TypeLine + " & ";
                    }
                    foreach (Item item in removed) {
                        logRemoved = logRemoved + item.Name + " " + item.TypeLine + " & ";
                    }

                    StashtabLog.Info("Stash (account:" + Settings.GetValue("account")+ ",name:"+StashName + ", league:"+Settings.GetValue("league")+") changes: " + logAdded + "||" + logRemoved);
                }
                else {
                    System.Threading.Thread.Sleep(MsListenDelay - (int)ListenTimer.ElapsedMilliseconds);
                }
            }
        }

    }
}