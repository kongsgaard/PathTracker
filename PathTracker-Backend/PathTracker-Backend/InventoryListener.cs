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
        private List<Item> CurrentInventory = null;
        private Character CurrentCharacter = null;

        private static readonly ILog InventoryLog = log4net.LogManager.GetLogger(LogManager.GetRepository(Assembly.GetEntryAssembly()).Name, "InventoryLogger");

        public InventoryListener(RequestCoordinator coordinator) {
            Coordinator = coordinator;
            
            log4net.GlobalContext.Properties["InventoryLogFileName"] = Directory.GetCurrentDirectory() + "//Logs//InventoryLog";
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }

        public void StartListening() {
            Inventory current = Coordinator.GetInventory();
            CurrentInventory = current.Items.Where(x => x.InventoryId == "MainInventory").ToList();
            CurrentCharacter = current.Character;
        }

        public void Listen() {
            Inventory newInventory = Coordinator.GetInventory();

            List<Item> newFiltered = newInventory.Items.Where(x => x.InventoryId == "MainInventory").ToList();
            (List<Item> added, List<Item> removed) = Toolbox.ItemDiffer(CurrentInventory, newInventory.Items);

            CurrentInventory = newFiltered;

            string logAdded = "Added - ";
            string logRemoved = "Removed - ";

            foreach (Item item in added) {
                logAdded = logAdded + item.Name + " " + item.TypeLine + " & ";
            }
            foreach (Item item in removed) {
                logRemoved = logRemoved + item.Name + " " + item.TypeLine + " & ";
            }

            InventoryLog.Info("Inventory (account:" + Settings.GetValue("Account") + ",character:" + Settings.GetValue("CurrentCharacter") + ") changes: " + logAdded + "||" + logRemoved);
        }

        public void NewZoneEntered(object sender, NewZoneArgs args) {
            Listen();
        }

        public void StopListening() {
            throw new NotImplementedException();
        }
    }
}
