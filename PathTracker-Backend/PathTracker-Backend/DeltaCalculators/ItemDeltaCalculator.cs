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
    public class ItemDeltaCalculator {

        private static readonly ILog ItemDeltaLog = LogCreator.CreateLog("ItemDeltaCalculator");

        //Track old/new inventories
        Dictionary<string, int> enteredWithStackableCountDictionary = new Dictionary<string, int>();
        Dictionary<string, int> leftWithStackableCountDictionary = new Dictionary<string, int>();

        List<Item> enteredWithNonStackableItems = new List<Item>();
        List<Item> leftWithNonStackableItems = new List<Item>();


        //For delta
        List<Item> AddedNonStackables = new List<Item>();
        List<Item> RemovedNonStackables = new List<Item>();
        
        Dictionary<string, int> DeltaStackableCountDictionary = new Dictionary<string, int>();

        public (Dictionary<string,int>, List<Item>, List<Item>) CalculateDelta(string zoneName) {
            enteredWithStackableCountDictionary = new Dictionary<string, int>();
            leftWithStackableCountDictionary = new Dictionary<string, int>();
            enteredWithNonStackableItems = new List<Item>();
            leftWithNonStackableItems = new List<Item>();

            DeltaStackableCountDictionary = new Dictionary<string, int>();
            AddedNonStackables = new List<Item>();
            RemovedNonStackables = new List<Item>();

            IterateItemList(enteredZoneWithItems, enteredWithStackableCountDictionary, enteredWithNonStackableItems);
            IterateItemList(leftZoneWithItems, leftWithStackableCountDictionary, leftWithNonStackableItems);

            DeltaStackableCountDictionary = CalculateStackableDelta(enteredWithStackableCountDictionary, leftWithStackableCountDictionary);
            (AddedNonStackables, RemovedNonStackables) = CalculateNonStackableDelta(leftWithNonStackableItems, enteredWithNonStackableItems);

            string logAdded = "Added - ";
            string logRemoved = "Removed - ";
            string logStackableDelta = "Stackables - ";

            foreach (Item item in AddedNonStackables) {
                logAdded = logAdded + item.Name + " " + item.TypeLine + " & ";
            }
            foreach (Item item in RemovedNonStackables) {
                logRemoved = logRemoved + item.Name + " " + item.TypeLine + " & ";
            }
            foreach (var v in DeltaStackableCountDictionary) {
                logStackableDelta = logStackableDelta + v.Key + ":" + v.Value + " & ";
            }

            ItemDeltaLog.Info("DeltaCalculator for ZoneName changes: " + logAdded + "||" + logRemoved + "||" + logStackableDelta);

            return (DeltaStackableCountDictionary, AddedNonStackables, RemovedNonStackables);
        }

        private Dictionary<string, int> CalculateStackableDelta(Dictionary<string, int> enteredWithCount, Dictionary<string, int> leftWithCount) {
            List<string> typeLines = new List<string>();

            var typeLinesEnteredWith =
                from x in enteredWithCount
                select x.Key;

            var typeLinesLeftWith =
                from x in leftWithCount
                select x.Key;

            var typeLinesDelta = typeLinesLeftWith.Union(typeLinesEnteredWith);

            Dictionary<string, int> DeltaCount = new Dictionary<string, int>();

            foreach (string typeline in typeLinesDelta) {
                if (enteredWithCount.ContainsKey(typeline) && leftWithCount.ContainsKey(typeline)) {
                    //Only add if there is a delta
                    if (leftWithCount[typeline] - enteredWithCount[typeline] != 0) {
                        DeltaCount[typeline] = leftWithCount[typeline] - enteredWithCount[typeline];
                    }
                }
                else if (leftWithCount.ContainsKey(typeline) && !enteredWithCount.ContainsKey(typeline)) {
                    DeltaCount[typeline] = leftWithCount[typeline] - 0;
                }
                else if (enteredWithCount.ContainsKey(typeline) && !leftWithCount.ContainsKey(typeline)) {
                    DeltaCount[typeline] = 0 - enteredWithCount[typeline];
                }
                else {
                    throw new Exception("Dang, should never happen. Detected stackable type:" + typeline + " failed to calculate delta");
                }
            }

            return DeltaCount;
            
        }

        private (List<Item>, List<Item>) CalculateNonStackableDelta(List<Item> leftWithNonStackables, List<Item> enteredWithNonStackables) {

            var added = leftWithNonStackables.Except(enteredWithNonStackables, new ItemComparer()).ToList();
            var removed = enteredWithNonStackables.Except(leftWithNonStackables, new ItemComparer()).ToList();

            return (added, removed);
        }

        /// <summary>
        /// Iterates the item list and puts the input in two seperate lists, stackable (curency, div-cards etc.) and non-stackable items
        /// </summary>
        /// <param name="itemList"></param>
        /// <param name="stackableCountDictionary"></param>
        /// <param name="nonStackableItems"></param>
        private void IterateItemList(List<Item> itemList, Dictionary<string, int> stackableCountDictionary, List<Item> nonStackableItems) {

            foreach (Item item in itemList) {
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
        

        private Mutex addEnteredZoneWithItems = new Mutex();
        private Mutex addLeftZoneWithItems = new Mutex();

        private List<Item> _enteredZoneWithItems = new List<Item>();
        private List<Item> enteredZoneWithItems {
            get {
                List<Item> rtItems;
                addEnteredZoneWithItems.WaitOne();
                rtItems = new List<Item>(_enteredZoneWithItems);
                addEnteredZoneWithItems.ReleaseMutex();
                return rtItems;
            }
        }

        private List<Item> _leftZoneWithItems = new List<Item>();
        private List<Item> leftZoneWithItems {
            get {
                List<Item> rtItems;
                addLeftZoneWithItems.WaitOne();
                rtItems = new List<Item>(_leftZoneWithItems);
                addLeftZoneWithItems.ReleaseMutex();
                return rtItems;
            }
        }

        public void UpdateLeftZoneWithItems(List<Item> items) {
            addLeftZoneWithItems.WaitOne();
            _leftZoneWithItems.AddRange(items);
            addLeftZoneWithItems.ReleaseMutex();
        }

        public void UpdateEnteredZoneWithItems(List<Item> items) {
            addEnteredZoneWithItems.WaitOne();
            _enteredZoneWithItems.AddRange(items);
            addEnteredZoneWithItems.ReleaseMutex();
        }




    }
}
