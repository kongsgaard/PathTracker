﻿using System;
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
        private RequestCoordinator Coordinator;
        private ItemDeltaCalculator DeltaCalculator;
        private string StashName = "";
        private List<Item> CurrentStashItems = null;
        private SettingsManager Settings = SettingsManager.Instance;
        private static readonly ILog StashtabLog = log4net.LogManager.GetLogger(LogManager.GetRepository(Assembly.GetEntryAssembly()).Name, "StashtabLogger");

        public StashtabListener(string stashName, RequestCoordinator coordinator, ItemDeltaCalculator itemDeltaCalculator) {
            Coordinator = coordinator;
            DeltaCalculator = itemDeltaCalculator;
            StashName = stashName;
            
            log4net.GlobalContext.Properties["StashtabLogFileName"] = Directory.GetCurrentDirectory() + "//Logs//StashtabLog";
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }

        public void StartListening() {

            StashApiRequest currentStashTab = Coordinator.GetStashtab(StashName);
            CurrentStashItems = currentStashTab.Items;
        }

        public void Listen() {
            StashApiRequest newStashTab = Coordinator.GetStashtab(StashName);

            DeltaCalculator.ItemsUpdated(CurrentStashItems, newStashTab.Items);

            CurrentStashItems = newStashTab.Items;

            StashtabLog.Info("Stash (account:" + Settings.GetValue("account") + ",name:" + StashName + ", league:" + Settings.GetValue("league") + ") fetched");
        }

        public void NewZoneEntered(object sender, NewZoneArgs args) {
            StashtabLog.Info("New zone entered event fired for stash:"+StashName+". Zone:" + args.ZoneName);
            Listen();
        }

        public void StopListening() {
            throw new NotImplementedException();
        }

    }
}