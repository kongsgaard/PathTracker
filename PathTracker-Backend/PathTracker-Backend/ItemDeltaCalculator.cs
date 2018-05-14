using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using log4net;
using System.Reflection;
using System.IO;
using log4net.Config;

namespace PathTracker_Backend
{
    public class ItemDeltaCalculator
    {

        private static readonly ILog ItemDeltaLog = log4net.LogManager.GetLogger(LogManager.GetRepository(Assembly.GetEntryAssembly()).Name, "ItemDeltaLogger");

        //Track old/new inventories
        Dictionary<string, int> OldStackableCountDictionary = new Dictionary<string, int>();
        Dictionary<string, int> NewStackableCountDictionary = new Dictionary<string, int>();

        List<Item> OldNonStackableItems = new List<Item>();
        List<Item> NewNonStackableItems = new List<Item>();


        //For delta
        List<Item> AddedNonStackables = new List<Item>();
        List<Item> RemovedNonStackables = new List<Item>();

        public ItemDeltaCalculator() {
            log4net.GlobalContext.Properties["ItemDeltaLogFileName"] = Directory.GetCurrentDirectory() + "//Logs//ItemDeltaLog";
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }

        Dictionary<string, int> DeltaStackableCountDictionary = new Dictionary<string, int>();
        
        public void CalculateDelta(string ZoneName) {
            OldStackableCountDictionary = new Dictionary<string, int>();
            NewStackableCountDictionary = new Dictionary<string, int>();
            OldNonStackableItems = new List<Item>();
            NewNonStackableItems = new List<Item>();

            DeltaStackableCountDictionary = new Dictionary<string, int>();
            AddedNonStackables = new List<Item>();
            RemovedNonStackables = new List<Item>();

            IterateItemList(OldItems, OldStackableCountDictionary, OldNonStackableItems);
            IterateItemList(NewItems, NewStackableCountDictionary, NewNonStackableItems);

            DeltaStackableCountDictionary = CalculateStackableDelta(OldStackableCountDictionary, NewStackableCountDictionary);
            (AddedNonStackables, RemovedNonStackables) = CalculateNonStackableDelta(NewNonStackableItems, OldNonStackableItems);

            string logAdded = "Added - ";
            string logRemoved = "Removed - ";
            string logStackableDelta = "Stackables - ";

            foreach (Item item in AddedNonStackables) {
                logAdded = logAdded + item.Name + " " + item.TypeLine + " & ";
            }
            foreach (Item item in RemovedNonStackables) {
                logRemoved = logRemoved + item.Name + " " + item.TypeLine + " & ";
            }
            foreach(var v in DeltaStackableCountDictionary) {
                logStackableDelta = logStackableDelta + v.Key + ":" + v.Value + " & ";
            }

            ItemDeltaLog.Info("DeltaCalculator for ZoneName changes: " + logAdded + "||" + logRemoved + "||" + logStackableDelta);


            //Reset for next delta calculation
            OldItems = new List<Item>();
            NewItems = new List<Item>();
        }

        private Dictionary<string, int> CalculateStackableDelta(Dictionary<string, int> OldCount, Dictionary<string, int> NewCount) {
            List<string> typeLines = new List<string>();

            var typeLinesOld =
                from x in OldCount
                select x.Key;

            var typeLinesNew =
                from x in NewCount
                select x.Key;

            var typeLinesDelta = typeLinesNew.Union(typeLinesOld);

            Dictionary<string, int> DeltaCount = new Dictionary<string, int>();

            foreach (string typeline in typeLinesDelta) {
                if(OldCount.ContainsKey(typeline) && NewCount.ContainsKey(typeline)) {
                    //Only add if there is a delta
                    if(NewCount[typeline] - OldCount[typeline] != 0) {
                        DeltaCount[typeline] = NewCount[typeline] - OldCount[typeline];
                    }
                }
                else if (NewCount.ContainsKey(typeline) && !OldCount.ContainsKey(typeline)) {
                    DeltaCount[typeline] = NewCount[typeline] - 0;
                }
                else if (OldCount.ContainsKey(typeline) && !NewCount.ContainsKey(typeline)) {
                    DeltaCount[typeline] = 0 - OldCount[typeline];
                }
                else {
                    throw new Exception("Dang, should never happen. Detected stackable type:" + typeline + " failed to calculate delta");
                }
            }

            return DeltaCount;

        }

        private (List<Item>, List<Item>) CalculateNonStackableDelta(List<Item> newNonStackables, List<Item> oldNonStackables) {

            var added = newNonStackables.Except(oldNonStackables, new ItemComparer()).ToList();
            var removed = oldNonStackables.Except(newNonStackables, new ItemComparer()).ToList();

            return (added, removed);
        }

        private void IterateItemList(List<Item> itemList, Dictionary<string, int> stackableCountDictionary, List<Item> nonStackableItems) {

            foreach(Item item in itemList) {
                if (item.MaxStackSize == 0) {
                    nonStackableItems.Add(item);
                }
                else {
                    if (stackableCountDictionary.ContainsKey(item.TypeLine)) {
                        stackableCountDictionary[item.TypeLine] += item.StackSize;
                    }
                    else {
                        stackableCountDictionary[item.TypeLine] = item.StackSize;
                    }
                }
            }

        }

        private List<Item> OldItems = new List<Item>();
        private List<Item> NewItems = new List<Item>();
        private Mutex AddItemsMutex = new Mutex();
        public void ItemsUpdated(List<Item> oldItems, List<Item> newItems) {
            AddItemsMutex.WaitOne();

            OldItems.AddRange(oldItems);
            NewItems.AddRange(newItems);

            AddItemsMutex.ReleaseMutex();
        }
    }
}
