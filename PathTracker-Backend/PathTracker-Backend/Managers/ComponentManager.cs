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

        InventoryListener inventoryListener = null;
        ClientTxtListener clientTxtListener = null;
        Dictionary<string, StashtabListener> stashtabListeners = new Dictionary<string, StashtabListener>();
        IWebRequestManager requestCoordinator;
        ZoneManager zoneManager;
        ISettings Settings;
        ResourceManager Resource;

        public ComponentManager(IDiskSaver diskSaver, IWebRequestManager webRequestManager, IZonePropertyExtractor zonePropertyExtractor, 
                                ISettings settings, ICurrencyRates currencyRates, ResourceManager resource) {
            Resource = resource;
            Settings = settings;
            zoneManager = new ZoneManager(diskSaver, zonePropertyExtractor, Settings, currencyRates, Resource);
            requestCoordinator = webRequestManager;
        }

        public void StartClientTxtListener() {

            if(clientTxtListener == null) {
                clientTxtListener = new ClientTxtListener(zoneManager, Settings);
                clientTxtListener.StartListening();
            }
            else {
            }
        }

        public void StartInventoryListener() {
            if(clientTxtListener == null) {
                throw new Exception("Cant start InventoryListener with ClientTxtListerner is null");
            }

            if (inventoryListener == null) {
                inventoryListener = new InventoryListener(requestCoordinator, Settings);
                inventoryListener.StartListening();
                zoneManager.NewZoneEntered += inventoryListener.NewZoneEntered;
            }
            else {
            }
        }

        public void StartStashtabListener(string StashName) {
            if (clientTxtListener == null) {
                throw new Exception("Cant start InventoryListener with ClientTxtListerner is null");
            }

            if (!stashtabListeners.ContainsKey(StashName)) {
                stashtabListeners[StashName] = new StashtabListener(StashName, requestCoordinator, Settings);
                stashtabListeners[StashName].StartListening();
                zoneManager.NewZoneEntered += stashtabListeners[StashName].NewZoneEntered;
            }
            else {
            }
        }
    }
}
