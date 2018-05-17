using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;

namespace PathTracker_Backend
{
    public class Zone
    {
        public string ZoneID { get; set; }
        public string ZoneName { get; set; }
        public ZoneType Type { get; set; }
        public DateTime LastExitedZone { get; set; }
        public ItemDeltaCalculator deltaCalculator { get; set; }

        public List<Item> AddedNonStackableItems = new List<Item>();
        public List<Item> RemovedNonStackableItems = new List<Item>();
        public Dictionary<string, int> DeltaStackableItems = new Dictionary<string, int>();

        public Zone(string zoneName) {
            ZoneName = zoneName;
            deltaCalculator = new ItemDeltaCalculator();
        }
        
        public void CalculateAndAddToDelta() {
            List<Item> addedNonStackableItems = new List<Item>();
            List<Item> removedNonStackableItems = new List<Item>();
            Dictionary<string, int> deltaStackableItems = new Dictionary<string, int>();
            (deltaStackableItems, addedNonStackableItems, removedNonStackableItems) = deltaCalculator.CalculateDelta(ZoneName);

            AddedNonStackableItems.AddRange(addedNonStackableItems);
            RemovedNonStackableItems.AddRange(removedNonStackableItems);

            foreach(var kvp in deltaStackableItems) {
                if (DeltaStackableItems.ContainsKey(kvp.Key)) {
                    DeltaStackableItems[kvp.Key] += deltaStackableItems[kvp.Key];
                }
                else {
                    DeltaStackableItems[kvp.Key] = deltaStackableItems[kvp.Key];
                }
            }
        }

        public void AddItemsToJson(List<Item> items) {
            JArray itemArr = (JArray)zoneJson["itemDelta"];
            foreach (Item item in items) {
                itemArr.Add(JsonConvert.SerializeObject(item));
            }
        }

        public void AddStackableItemsToJson(Dictionary<string, int> StackableCountDictionary) {
            JArray stackableArr = (JArray)zoneJson["stackableItemDelta"];
            var currentDict = stackableArr.ToObject<Dictionary<string, int>>();

            foreach(var kvp in StackableCountDictionary) {
                if (currentDict.ContainsKey(kvp.Key)) {
                    currentDict[kvp.Key] = currentDict[kvp.Key] + StackableCountDictionary[kvp.Key];
                }
                else {
                    currentDict[kvp.Key] = StackableCountDictionary[kvp.Key];
                }
            }

            JProperty updatedProperty = 
                new JProperty("stackableItemDelta",
                    new JArray(currentDict));
            
            zoneJson["stackableItemDelta"].Replace(updatedProperty);
        }

        JObject zoneJson = 
            new JObject(
                new JProperty("itemDelta",
                    new JArray()),
                new JProperty("stackableItemDelta",
                    new JArray()),
                new JProperty("experience",0),
                new JProperty("zoneName",""),
                new JProperty("zoneID"),
                new JProperty("mods",
                    new JArray()));
                                

        public string ToJSON() {
            JObject zoneJson =
            new JObject(
                new JProperty("itemsAdded",
                    new JArray(AddedNonStackableItems)),
                new JProperty("itemsRemoved",
                    new JArray(RemovedNonStackableItems)),
                new JProperty("stackableItemDelta",
                    new JArray(DeltaStackableItems)),
                new JProperty("experience", 0),
                new JProperty("zoneName", ZoneName),
                new JProperty("zoneID", ZoneID),
                new JProperty("mods",
                    new JArray()));



            return "";
        }

        public void MergeZoneIntoThis(Zone zone) {
            LastExitedZone = zone.LastExitedZone;

            //Merge stackable items
            foreach (var kvp in zone.DeltaStackableItems) {
                if (DeltaStackableItems.ContainsKey(kvp.Key)) {
                    DeltaStackableItems[kvp.Key] += zone.DeltaStackableItems[kvp.Key];
                }
                else {
                    DeltaStackableItems[kvp.Key] = zone.DeltaStackableItems[kvp.Key];
                }
            }

            //Merge added and removed
            var nullifiedWasAdded = new List<Item>(AddedNonStackableItems.Intersect(zone.RemovedNonStackableItems, new ItemComparer()));
            foreach(var nowRemoved in nullifiedWasAdded) {
                AddedNonStackableItems.Remove(nowRemoved);
            }

            var nullifiedWasRemoved = new List<Item>(RemovedNonStackableItems.Intersect(zone.AddedNonStackableItems, new ItemComparer()));
            foreach(var nowAdded in nullifiedWasRemoved) {
                RemovedNonStackableItems.Remove(nowAdded);
            }

            var newAdded = zone.AddedNonStackableItems.Except(AddedNonStackableItems, new ItemComparer()).Except(RemovedNonStackableItems, new ItemComparer());
            var newRemoved = zone.RemovedNonStackableItems.Except(RemovedNonStackableItems, new ItemComparer()).Except(AddedNonStackableItems, new ItemComparer());

            AddedNonStackableItems.AddRange(newAdded);
            RemovedNonStackableItems.AddRange(newRemoved);
        }
        


    }

    public enum ZoneType { MonsterZone, SubZone, TownZone}
}
