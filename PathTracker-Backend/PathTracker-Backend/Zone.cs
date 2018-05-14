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
        public string ZoneName { get; set; }

        public Zone(string zoneName) {
            ZoneName = zoneName;
        }

        public void AddItemsToJson(List<Item> items) {
            JArray itemArr = (JArray)zoneJson["itemDelta"];
            foreach (Item item in items) {
                itemArr.Add(item);
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
                                


        
    }

    public enum ZoneType { MonsterZone, SubZone, TownZone}
}
