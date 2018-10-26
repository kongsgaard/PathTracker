using System;
using System.Collections.Generic;
using System.Text;
using log4net.Config;
using log4net;
using System.Reflection;
using System.IO;
using System.IO.Abstractions;

namespace PathTracker_Backend {
    public class ComponentManager {
        private static readonly ILog EventManagerLog = LogCreator.CreateLog("EventManager");

        InventoryListener inventoryListener = null;
        ClientTxtListener clientTxtListener = null;
        Dictionary<string, StashtabListener> stashtabListeners = new Dictionary<string, StashtabListener>();
        IWebRequestManager requestCoordinator;
        ZoneManager zoneManager;
        ISettings Settings;
        
        public ComponentManager(IDiskSaver diskSaver, IWebRequestManager webRequestManager, IZonePropertyExtractor zonePropertyExtractor, ISettings settings) {
            Settings = settings;
            zoneManager = new ZoneManager(diskSaver, zonePropertyExtractor, Settings);
            requestCoordinator = webRequestManager;
        }

        public void StartClientTxtListener() {

            if(clientTxtListener == null) {
                clientTxtListener = new ClientTxtListener(zoneManager, Settings);
                clientTxtListener.StartListening();
                EventManagerLog.Info("Starting new ClientTxtListener with ClientTxtPath:" + clientTxtListener.ClientTxtPath);
            }
            else {
                EventManagerLog.Info("ClientTxtListener listener already started with ClientTxtPath:" + clientTxtListener.ClientTxtPath);
            }
        }

        public void StartInventoryListener() {
            if(clientTxtListener == null) {
                throw new Exception("Cant start InventoryListener with ClientTxtListerner is null");
            }

            if (inventoryListener == null) {
                inventoryListener = new InventoryListener(requestCoordinator, Settings);
                inventoryListener.StartListening();
                EventManagerLog.Info("Starting new InventoryListener");
                zoneManager.NewZoneEntered += inventoryListener.NewZoneEntered;
            }
            else {
                EventManagerLog.Info("InventoryListener listener already started");
            }
        }

        public void StartStashtabListener(string StashName) {
            if (clientTxtListener == null) {
                throw new Exception("Cant start InventoryListener with ClientTxtListerner is null");
            }

            if (!stashtabListeners.ContainsKey(StashName)) {
                stashtabListeners[StashName] = new StashtabListener(StashName, requestCoordinator, Settings);
                stashtabListeners[StashName].StartListening();
                EventManagerLog.Info("Starting new stashtabListeners for stash:" + StashName);
                zoneManager.NewZoneEntered += stashtabListeners[StashName].NewZoneEntered;
            }
            else {
                EventManagerLog.Info("stashtabListeners listener already started for StashName:" + StashName);
            }
        }
    }
}
