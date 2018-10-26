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
    public class StashtabListener : IListener {
        
        private Stopwatch ListenTimer = new Stopwatch();
        private IWebRequestManager Coordinator;
        private string StashName = "";
        private SettingsManager Settings = SettingsManager.Instance;
        private static readonly ILog StashtabLog = LogCreator.CreateLog("StashtabListener");

        public StashtabListener(string stashName, IWebRequestManager coordinator) {
            Coordinator = coordinator;
            StashName = stashName;
        }

        public void StartListening() {
        }

        public void Listen(ItemDeltaCalculator fromZoneDeltaCalculator, ItemDeltaCalculator enteredZoneDeltaCalculator) {
            StashApiRequest newStashTab = Coordinator.GetStashtab(StashName);

            if(fromZoneDeltaCalculator != null) {
                fromZoneDeltaCalculator.UpdateLeftZoneWithItems(newStashTab.Items);
            }
            enteredZoneDeltaCalculator.UpdateEnteredZoneWithItems(newStashTab.Items);
            
            StashtabLog.Info("Stash (account:" + Settings.GetValue("account") + ",name:" + StashName + ", league:" + Settings.GetValue("league") + ") fetched");
        }

        public void NewZoneEntered(object sender, ZoneChangeArgs args) {
            StashtabLog.Info("New zone entered event fired for stash:"+StashName+". Zone:" + args.ZoneName);
            Listen(args.FromZoneDeltaCalculator, args.EnteredZoneDeltaCalculator);
        }

        public void StopListening() {
            throw new NotImplementedException();
        }

    }
}