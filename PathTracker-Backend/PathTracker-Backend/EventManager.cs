using System;
using System.Collections.Generic;
using System.Text;
using log4net.Config;
using log4net;
using System.Reflection;
using System.IO;

namespace PathTracker_Backend {
    public class EventManager {
        private static readonly ILog EventManagerLog = LogCreator.CreateLog("EventManager");

        InventoryListener inventoryListener = null;
        ClientTxtListener clientTxtListener = null;
        Dictionary<string, StashtabListener> stashtabListeners = new Dictionary<string, StashtabListener>();
        RequestCoordinator requestCoordinator = new RequestCoordinator();
        ZoneManager zoneManager = new ZoneManager();
        
        public void StartClientTxtListener() {

            if(clientTxtListener == null) {
                clientTxtListener = new ClientTxtListener(zoneManager);
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
                inventoryListener = new InventoryListener(requestCoordinator);
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
                stashtabListeners[StashName] = new StashtabListener(StashName, requestCoordinator);
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
