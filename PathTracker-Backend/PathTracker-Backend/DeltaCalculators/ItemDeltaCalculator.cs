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

        //Track old/new inventories
        Dictionary<string, int> enteredWithStackableCountDictionary = new Dictionary<string, int>();
        Dictionary<string, int> leftWithStackableCountDictionary = new Dictionary<string, int>();

        List<Item> enteredWithNonStackableItems = new List<Item>();
        List<Item> leftWithNonStackableItems = new List<Item>();


        //For delta
        List<Item> AddedNonStackables = new List<Item>();
        List<Item> RemovedNonStackables = new List<Item>();
        List<Tuple<ItemChangeType, Item>> ModifiedNonStackables = new List<Tuple<ItemChangeType, Item>>();

        Dictionary<string, int> DeltaStackableCountDictionary = new Dictionary<string, int>();

        public (Dictionary<string,int>, List<Item>, List<Item>, List<Tuple<ItemChangeType, Item>>) CalculateDelta(string zoneName) {
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
            (AddedNonStackables, RemovedNonStackables, ModifiedNonStackables) = CalculateNonStackableDelta(leftWithNonStackableItems, enteredWithNonStackableItems);

            string logAdded = "Added - ";
            string logRemoved = "Removed - ";
            string logStackableDelta = "Stackables - ";

            foreach (Item item in AddedNonStackables) {
                logAdded = logAdded + item.name + " " + item.typeLine + " & ";
            }
            foreach (Item item in RemovedNonStackables) {
                logRemoved = logRemoved + item.name + " " + item.typeLine + " & ";
            }
            foreach (var v in DeltaStackableCountDictionary) {
                logStackableDelta = logStackableDelta + v.Key + ":" + v.Value + " & ";
            }


            return (DeltaStackableCountDictionary, AddedNonStackables, RemovedNonStackables, ModifiedNonStackables);
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

        private (List<Item>, List<Item>, List<Tuple<ItemChangeType, Item>>) CalculateNonStackableDelta(List<Item> leftWithNonStackables, List<Item> enteredWithNonStackables) {

            var added = leftWithNonStackables.Except(enteredWithNonStackables, new ItemComparer()).ToList();
            var removed = enteredWithNonStackables.Except(leftWithNonStackables, new ItemComparer()).ToList();
            var modified = CalculateModifiedItems(leftWithNonStackables, enteredWithNonStackables);

            return (added, removed, modified);
        }
        
        private List<Tuple<ItemChangeType, Item>> CalculateModifiedItems(List<Item> leftWithNonStackables, List<Item> enteredWithNonStackables) {

            var AlteredItems = new List<Tuple<ItemChangeType, Item>>();

            foreach(Item entered in enteredWithNonStackableItems) {

                var leftList = leftWithNonStackableItems.Where(x => x.itemId == entered.itemId);
                if(leftList.Count() > 1) {
                    throw new Exception("Should not happen - Multiple items with same ID found in delta lists");
                }
                else if(leftList.Count() == 1) {
                    ItemChangeType changeType = FindItemChange(entered, leftList.First());

                    if(changeType != ItemChangeType.NoChange) {
                        AlteredItems.Add(new Tuple<ItemChangeType, Item>(changeType, leftList.First()));
                    }
                }
            }
            return AlteredItems;
        }

        /// <summary>
        /// The order of checks matter. Any ItemChangeType that should alter in items being reallocated to a new zone should come first
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <returns></returns>
        private ItemChangeType FindItemChange(Item i1, Item i2) {

            if(i1.enchantMods != i2.enchantMods) {
                var ex1 = i1.enchantMods.Except(i2.enchantMods);
                var ex2 = i2.enchantMods.Except(i1.enchantMods);

                if(ex1.Count() > 0 || ex2.Count() > 0) {
                    return ItemChangeType.EnchantedModChanged;
                }
            }

            if(i1.note != i2.note) {
                return ItemChangeType.NoteChanged;
            }

            return ItemChangeType.NoChange;
        }
        
        /// <summary>
        /// Iterates the item list and puts the input in two seperate lists, stackable (curency, div-cards etc.) and non-stackable items
        /// </summary>
        /// <param name="itemList"></param>
        /// <param name="stackableCountDictionary"></param>
        /// <param name="nonStackableItems"></param>
        private void IterateItemList(List<Item> itemList, Dictionary<string, int> stackableCountDictionary, List<Item> nonStackableItems) {

            foreach (Item item in itemList) {
                if (item.maxStackSize == 0) {
                    nonStackableItems.Add(item);
                }
                else {
                    if (stackableCountDictionary.ContainsKey(item.typeLine)) {
                        stackableCountDictionary[item.typeLine] += item.stackSize;
                    }
                    else {
                        stackableCountDictionary[item.typeLine] = item.stackSize;
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

    public enum ItemChangeType { NoChange, NoteChanged, EnchantedModChanged }
}
