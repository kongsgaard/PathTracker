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
    public class InventoryListener : IListener {
        private Stopwatch ListenTimer = new Stopwatch();
        private RequestCoordinator Coordinator;
        private SettingsManager Settings = SettingsManager.Instance;

        private static readonly ILog InventoryLog = log4net.LogManager.GetLogger(LogManager.GetRepository(Assembly.GetEntryAssembly()).Name, "InventoryLogger");

        public InventoryListener(RequestCoordinator coordinator) {
            Coordinator = coordinator;
            
            log4net.GlobalContext.Properties["InventoryLogFileName"] = Directory.GetCurrentDirectory() + "//Logs//InventoryLog";
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }

        public void StartListening() {
        }

        public void Listen(ItemDeltaCalculator fromZoneDeltaCalculator, ItemDeltaCalculator enteredZoneDeltaCalculator) {
            Inventory newInventory = Coordinator.GetInventory();
            if(fromZoneDeltaCalculator != null) {
                fromZoneDeltaCalculator.UpdateLeftZoneWithItems(newInventory.Items);
            }
            enteredZoneDeltaCalculator.UpdateEnteredZoneWithItems(newInventory.Items);
            
            InventoryLog.Info("Inventory (account:" + Settings.GetValue("Account") + ",character:" + Settings.GetValue("CurrentCharacter") + ") fetched");
        }

        public void NewZoneEntered(object sender, ZoneChangeArgs args) {
            InventoryLog.Info("New zone entered event fired. Zone:" + args.ZoneName);
            Listen(args.FromZoneDeltaCalculator,args.EnteredZoneDeltaCalculator);
        }

        public void StopListening() {
            throw new NotImplementedException();
        }
    }
}
