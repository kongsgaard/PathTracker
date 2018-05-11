using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;

namespace PathTracker_Backend
{
    public class ItemDeltaCalculator
    {
        //Track old/new inventories
        Dictionary<string, int> OldStackableCountDictionary = new Dictionary<string, int>();
        Dictionary<string, int> NewStackableCountDictionary = new Dictionary<string, int>();

        List<Item> OldNonStackableItems = new List<Item>();
        List<Item> NewNonStackableItems = new List<Item>();


        //For delta
        List<Item> AddedNonStackables = new List<Item>();
        List<Item> RemovedNonStackables = new List<Item>();

        Dictionary<string, int> DeltaStackableCountDictionary = new Dictionary<string, int>();
        
        public void CalculateDelta() {
            OldStackableCountDictionary = new Dictionary<string, int>();
            NewStackableCountDictionary = new Dictionary<string, int>();
            OldNonStackableItems = new List<Item>();
            NewNonStackableItems = new List<Item>();

            DeltaStackableCountDictionary = new Dictionary<string, int>();
            AddedNonStackables = new List<Item>();
            RemovedNonStackables = new List<Item>();

            IterateItemList(OldItems, OldStackableCountDictionary, OldNonStackableItems);
            IterateItemList(NewItems, NewStackableCountDictionary, NewNonStackableItems);

            CalculateStackableDelta(DeltaStackableCountDictionary, OldStackableCountDictionary, NewStackableCountDictionary);


            //Reset for next delta calculation
            OldItems = new List<Item>();
            NewItems = new List<Item>();
        }

        private void CalculateStackableDelta(Dictionary<string, int> DeltaCount, Dictionary<string, int> OldCount, Dictionary<string, int> NewCount) {
            List<string> typeLines = new List<string>();

            var typeLinesOld =
                from x in OldCount
                select x.Key;

            var typeLinesNew =
                from x in NewCount
                select x.Key;

            var typeLinesDelta = typeLinesNew.Union(typeLinesOld);

            foreach(string typeline in typeLinesDelta) {
                if(OldCount.ContainsKey(typeline) && NewCount.ContainsKey(typeline)) {
                    //Only add if there is a delta
                    if(NewCount[typeline] - OldCount[typeline] != 0) {
                        DeltaCount[typeline] = NewCount[typeline] - OldCount[typeline];
                    }
                }
                else if (NewCount.ContainsKey(typeline) && !OldCount.ContainsKey(typeline)) {
                    DeltaCount[typeline] = NewCount[typeline];
                }
                else if (OldCount.ContainsKey(typeline) && !NewCount.ContainsKey(typeline)) {
                    DeltaCount[typeline] = OldCount[typeline];
                }
                else {
                    throw new Exception("Dang, should never happen. Detected stackable type:" + typeline + " failed to calculate delta");
                }
            }

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
            NewItems.AddRange(NewItems);

            AddItemsMutex.ReleaseMutex();
        }
    }
}
