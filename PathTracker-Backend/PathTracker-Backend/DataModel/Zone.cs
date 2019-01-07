using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using System.Diagnostics;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PathTracker_Backend
{
    public class Zone
    {
        [BsonId]
        public ObjectId _id { get; set; }

        [BsonElement(elementName: "ZoneID")]
        public string ZoneID { get; set; }

        [BsonElement(elementName: "ZoneName")]
        public string ZoneName { get; set; }

        [BsonElement(elementName: "ZoneInfo")]
        public ZoneInfo ZoneInfo { get; set; }

        [BsonElement(elementName: "Type")]
        public ZoneType Type { get; set; }

        [BsonElement(elementName: "LastExitedZone")]
        public DateTime LastExitedZone { get; set; }

        [BsonIgnore]
        public ItemDeltaCalculator deltaCalculator { get; set; }

        [BsonIgnore]
        public ExperienceDeltaCalculator experienceCalculator { get; set; }

        [BsonIgnore]
        public ResourceManager Resource;

        [BsonElement(elementName: "mapMods")]
        public List<MapMod> mapMods = new List<MapMod>();

        [BsonElement(elementName: "AddedNonStackableItems")]
        public List<Item> AddedNonStackableItems = new List<Item>();

        [BsonElement(elementName: "RemovedNonStackableItems")]
        public List<Item> RemovedNonStackableItems = new List<Item>();

        [BsonIgnore]
        public List<Tuple<ItemChangeType, Item>> ModifiedNonStackableItems = new List<Tuple<ItemChangeType, Item>>();

        [BsonElement(elementName: "DeltaStackableItems")]
        public Dictionary<string, int> DeltaStackableItems = new Dictionary<string, int>();

        //Character name, Character progress dictionary. Enables multiple character deltas from same map
        [BsonElement(elementName: "characterProgress")]
        public Dictionary<string,CharacterProgress> characterProgress = new Dictionary<string, CharacterProgress>();

        [BsonElement(elementName: "SecondsInZone")]
        public double SecondsInZone = 0;

        [BsonIgnore]
        public Stopwatch zoneTimer = new Stopwatch();

        [BsonElement(elementName: "timeEntered")]
        public DateTime timeEntered;

        [BsonElement(elementName: "timeLeft")]
        public DateTime timeLeft;

        [BsonElement(elementName: "ConfirmedChaosAdded")]
        public double ConfirmedChaosAdded = 0;

        [BsonElement(elementName: "TentativeChaosAdded")]
        public double TentativeChaosAdded = 0;

        [BsonElement(elementName: "ConfirmedChaosRemoved")]
        public double ConfirmedChaosRemoved = 0;

        public Zone(string zoneName, ResourceManager resource) {
            Resource = resource;
            ZoneName = zoneName;
            deltaCalculator = new ItemDeltaCalculator();
            experienceCalculator = new ExperienceDeltaCalculator(Resource);
            zoneTimer.Start();
            timeEntered = DateTime.Now;
        }
        
        public void CalculateAndAddToDelta() {
            List<Item> addedNonStackableItems = new List<Item>();
            List<Item> removedNonStackableItems = new List<Item>();
            List<Tuple<ItemChangeType, Item>> modifiedNonStackableItems = new List<Tuple<ItemChangeType, Item>>();
            Dictionary<string, int> deltaStackableItems = new Dictionary<string, int>();

            (deltaStackableItems, addedNonStackableItems, removedNonStackableItems, modifiedNonStackableItems) = deltaCalculator.CalculateDelta(ZoneName);

            AddedNonStackableItems.AddRange(addedNonStackableItems);
            RemovedNonStackableItems.AddRange(removedNonStackableItems);
            ModifiedNonStackableItems.AddRange(modifiedNonStackableItems);

            DeltaStackableItems = Toolbox.AddDictionaries(DeltaStackableItems, deltaStackableItems);
            
            CharacterProgress progress = experienceCalculator.CalculateDelta();
            characterProgress[progress.Name] = progress;
        }
        
        public string ToJSON() {

            JProperty itemsAdded = new JProperty("AddedNonStackableItems", JArray.FromObject(AddedNonStackableItems));
            JProperty itemsRemoved = new JProperty("RemovedNonStackableItems", JArray.FromObject(RemovedNonStackableItems));
            JProperty stackableItemsDelta = new JProperty("DeltaStackableItems", JObject.FromObject(DeltaStackableItems));
            JProperty charProgress = new JProperty("characterProgress", JObject.FromObject(characterProgress));
            JProperty zoneName = new JProperty("ZoneName", ZoneName);
            JProperty zoneID = new JProperty("ZoneID", ZoneID);
            JProperty mpMods = new JProperty("mapMods", JArray.FromObject(mapMods));
            JProperty timeInZone = new JProperty("SecondsInZone", SecondsInZone += zoneTimer.ElapsedMilliseconds / (double)1000);
            JProperty JtimeEntered = new JProperty("timeEntered", timeEntered);
            JProperty JtimeLeft = new JProperty("timeLeft", timeLeft);

            JProperty JConfirmedChaosAdded = new JProperty("ConfirmedChaosAdded", ConfirmedChaosAdded);
            JProperty JTentativeChaosAdded = new JProperty("TentativeChaosAdded", TentativeChaosAdded);
            JProperty JConfirmedChaosRemoved = new JProperty("ConfirmedChaosRemoved", ConfirmedChaosRemoved);

            JObject zoneJson = new JObject(itemsAdded, itemsRemoved, stackableItemsDelta, charProgress, zoneName, zoneID, mpMods, timeInZone, JtimeEntered, JtimeLeft,
                                            JConfirmedChaosAdded, JTentativeChaosAdded, JConfirmedChaosRemoved);
            
            return zoneJson.ToString();
        }

        /// <summary>
        /// Merges the values from the input zone into this.
        /// Used when the same zone is entered multiple times.
        /// Using "this" notation to make it more clear when refering to fields on this object
        /// </summary>
        /// <param name="zone"></param>
        public void MergeZoneIntoThis(Zone zone) {
            this.LastExitedZone = zone.LastExitedZone;

            //Merge stackable items
            this.DeltaStackableItems = Toolbox.AddDictionaries(this.DeltaStackableItems, zone.DeltaStackableItems);

            this.SecondsInZone = zone.SecondsInZone;

            /* Merge added and removed
             * Special case here is when an added item form the previous zone is now removed, and the other way around.
             * In this case we do not want both items to appear on the Removed and Added list, so it becomes nullified instead (i.e) removed from the appropriate list 
             */
            var nullifiedWasAdded = new List<Item>(this.AddedNonStackableItems.Intersect(zone.RemovedNonStackableItems, new ItemComparer()));
            foreach(var nowRemoved in nullifiedWasAdded) {
                this.AddedNonStackableItems.Remove(nowRemoved);
            }

            var nullifiedWasRemoved = new List<Item>(this.RemovedNonStackableItems.Intersect(zone.AddedNonStackableItems, new ItemComparer()));
            foreach(var nowAdded in nullifiedWasRemoved) {
                this.RemovedNonStackableItems.Remove(nowAdded);
            }

            var newAdded = zone.AddedNonStackableItems.Except(this.AddedNonStackableItems, new ItemComparer()).Except(this.RemovedNonStackableItems, new ItemComparer());
            var newRemoved = zone.RemovedNonStackableItems.Except(this.RemovedNonStackableItems, new ItemComparer()).Except(this.AddedNonStackableItems, new ItemComparer());

            this.AddedNonStackableItems.AddRange(newAdded);
            this.RemovedNonStackableItems.AddRange(newRemoved);

            //Merge the character progress into the zone
            string newZoneCharacterName = zone.characterProgress.Single(x => true).Key;
            if (this.characterProgress.ContainsKey(newZoneCharacterName)) {
                this.characterProgress[newZoneCharacterName].ExperienceProgress = this.characterProgress[newZoneCharacterName].ExperienceProgress + zone.characterProgress[newZoneCharacterName].ExperienceProgress;
                this.characterProgress[newZoneCharacterName].TotalExperienceNonPenalized = this.characterProgress[newZoneCharacterName].TotalExperienceNonPenalized + zone.characterProgress[newZoneCharacterName].TotalExperienceNonPenalized;
                this.characterProgress[newZoneCharacterName].EquippedItems = zone.characterProgress[newZoneCharacterName].EquippedItems;

                this.characterProgress[newZoneCharacterName].LevelProgress = Toolbox.AddDictionaries(this.characterProgress[newZoneCharacterName].LevelProgress, zone.characterProgress[newZoneCharacterName].LevelProgress);
            }
            else {
                this.characterProgress[newZoneCharacterName] = zone.characterProgress[newZoneCharacterName];
            }
        }
        
        public void RemoveItem(Item item) {
            var countRemoved = AddedNonStackableItems.RemoveAll(x => x.itemId == item.itemId);
            if(countRemoved != 1) {
                throw new Exception("Tried to remove item from zone, with item id:" + item.itemId + " || Could not remove single");
            }
        }

        public void UpdateItem(Item item) {
            var countRemoved = AddedNonStackableItems.RemoveAll(x => x.itemId == item.itemId);
            if (countRemoved != 1) {
                throw new Exception("Tried to remove item from zone, with item id:" + item.itemId + " || Could not remove single");
            }
            AddedNonStackableItems.Add(item);
        }

        public void CalculatZoneWorth(ItemValuator itemValuator) {


            TentativeChaosAdded = 0;
            ConfirmedChaosAdded = 0;
            ConfirmedChaosRemoved = 0;

            foreach (Item i in AddedNonStackableItems) {
                var value = itemValuator.ItemChaosValue(i);
                ItemValue itemValue = new ItemValue();
                itemValue.currentChaosValue = value.Item1;
                itemValue.valueMode = value.Item2;
                itemValue.setAt = LastExitedZone;
                itemValue.zoneID = ZoneID;

                i.itemValues.values.Add(itemValue);
                i.itemValues.currentChaosValue = value.Item1;
                i.itemValues.valueMode = value.Item2;

                if (value.Item2 == ItemValueMode.Tentative) {
                    TentativeChaosAdded += value.Item1;
                }
                else if (value.Item2 == ItemValueMode.Confirmed) {
                    ConfirmedChaosAdded += value.Item1;
                }
            }

            foreach (var s in DeltaStackableItems) {
                if (s.Value < 0) {
                    ConfirmedChaosRemoved += itemValuator.CurrencyChaosValue(s.Key, s.Value);
                }
                else {
                    ConfirmedChaosAdded += itemValuator.CurrencyChaosValue(s.Key, s.Value);
                }
            }

        }


    }

    public enum ZoneType { MonsterZone, SubZone, TownZone}
}
