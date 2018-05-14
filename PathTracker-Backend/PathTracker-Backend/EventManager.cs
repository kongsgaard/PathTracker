using System;
using System.Collections.Generic;
using System.Text;
using log4net.Config;
using log4net;
using System.Reflection;
using System.IO;

namespace PathTracker_Backend {
    public class EventManager {
        private static readonly ILog EventManagerLog = log4net.LogManager.GetLogger(LogManager.GetRepository(Assembly.GetEntryAssembly()).Name, "EventManagerLogger");

        InventoryListener inventoryListener = null;
        ClientTxtListener clientTxtListener = null;
        Dictionary<string, StashtabListener> stashtabListeners = new Dictionary<string, StashtabListener>();
        RequestCoordinator requestCoordinator = new RequestCoordinator();

        public EventManager() {

            log4net.GlobalContext.Properties["EventManagerLogFileName"] = Directory.GetCurrentDirectory() + "//Logs//EventManagerLog";
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }

        public void StartClientTxtListener() {

            if(clientTxtListener == null) {
                clientTxtListener = new ClientTxtListener();
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
                inventoryListener = new InventoryListener(requestCoordinator, deltaCalculator);
                inventoryListener.StartListening();
                EventManagerLog.Info("Starting new InventoryListener");
                clientTxtListener.NewZoneEntered += inventoryListener.NewZoneEntered;
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
                stashtabListeners[StashName] = new StashtabListener(StashName, requestCoordinator, deltaCalculator);
                stashtabListeners[StashName].StartListening();
                EventManagerLog.Info("Starting new stashtabListeners for stash:" + StashName);
                clientTxtListener.NewZoneEntered += stashtabListeners[StashName].NewZoneEntered;
            }
            else {
                EventManagerLog.Info("stashtabListeners listener already started for StashName:" + StashName);
            }
        }
    }
}
