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
    public class InventoryListener : IListener {
        private Stopwatch ListenTimer = new Stopwatch();
        private IWebRequestManager Coordinator;
        private ISettings Settings;


        public InventoryListener(IWebRequestManager coordinator, ISettings settings) {
            Coordinator = coordinator;
            Settings = settings;
        }

        public void StartListening() {
        }

        public void Listen(ItemDeltaCalculator fromZoneDeltaCalculator, ItemDeltaCalculator enteredZoneDeltaCalculator, 
            ExperienceDeltaCalculator fromZoneExperienceDeltaCalculator, ExperienceDeltaCalculator enteredZoneExperienceDeltaCalculator) {
            Inventory newInventory = Coordinator.GetInventory();

            var InventoryItems = newInventory.Items.Where(x => x.inventoryId == "MainInventory").ToList();
            var EquippedItems = newInventory.Items.Where(x => x.inventoryId != "MainInventory").ToList();

            if (fromZoneDeltaCalculator != null) {
                fromZoneDeltaCalculator.UpdateLeftZoneWithItems(InventoryItems);
                fromZoneExperienceDeltaCalculator.ExitedWithCharacter = newInventory.Character;
                fromZoneExperienceDeltaCalculator.ExitedWithItems = EquippedItems;
            }

            enteredZoneDeltaCalculator.UpdateEnteredZoneWithItems(InventoryItems);
            enteredZoneExperienceDeltaCalculator.EnteredWithCharacter = newInventory.Character;
            enteredZoneExperienceDeltaCalculator.EnteredWithItems = EquippedItems;
            
        }

        public void NewZoneEntered(object sender, ZoneChangeArgs args) {
            Listen(args.FromZoneDeltaCalculator,args.EnteredZoneDeltaCalculator, args.FromZoneExperienceDeltaCalculator, args.EnteredZoneExperienceDeltaCalculator);
        }

        public void StopListening() {
            throw new NotImplementedException();
        }
    }
}
