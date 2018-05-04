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
    public class InventoryListener {
        private int MsListenDelay;
        private Stopwatch ListenTimer = new Stopwatch();
        private RequestCoordinator Coordinator;

        private List<Item> CurrentInventory = null;

        private static readonly ILog InventoryLog = log4net.LogManager.GetLogger(LogManager.GetRepository(Assembly.GetEntryAssembly()).Name, "InventoryLogger");

        public InventoryListener(int msListenDelay, RequestCoordinator coordinator) {
            MsListenDelay = msListenDelay;
            Coordinator = coordinator;
            
            log4net.GlobalContext.Properties["InventoryLogFileName"] = Directory.GetCurrentDirectory() + "//Logs//InventoryLog";
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }

        public void StartListening() {

            Inventory current = Coordinator.GetInventory();
            CurrentInventory = current.Items.Where(x => x.InventoryId == "MainInventory").ToList();
            
            ListenTimer.Start();

            while (true) {

                if(ListenTimer.ElapsedMilliseconds >= MsListenDelay) {
                    Inventory newInventory = Coordinator.GetInventory();
                    ListenTimer.Restart();
                    InventoryDiffer(CurrentInventory, newInventory.Items);
                }
                else {
                    System.Threading.Thread.Sleep(MsListenDelay - (int)ListenTimer.ElapsedMilliseconds);
                }
            }
        }

        private void InventoryDiffer(List<Item> previous, List<Item> current) {
            List<Item> currentFiltered = current.Where(x => x.InventoryId == "MainInventory").ToList();

            var added = currentFiltered.Except(previous, new ItemComparer()).ToList();
            var removed = previous.Except(currentFiltered, new ItemComparer()).ToList();



            foreach (Item item in added) {
                Console.WriteLine("ADDED:" + item.Name + " " + item.TypeLine);
            }

            foreach (Item item in removed) {
                Console.WriteLine("REMOVED:" + item.Name + " " + item.TypeLine);
            }

            CurrentInventory = currentFiltered;



        }



    }
}
