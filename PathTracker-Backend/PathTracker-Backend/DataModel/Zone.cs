﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using System.Diagnostics;

namespace PathTracker_Backend
{
    public class Zone
    {
        public string ZoneID { get; set; }
        public string ZoneName { get; set; }
        public ZoneType Type { get; set; }
        public DateTime LastExitedZone { get; set; }
        public ItemDeltaCalculator deltaCalculator { get; set; }
        public ExperienceDeltaCalculator experienceCalculator { get; set; }

        public ResourceManager Resource;

        public List<MapMod> mapMods = new List<MapMod>();

        public List<Item> AddedNonStackableItems = new List<Item>();
        public List<Item> RemovedNonStackableItems = new List<Item>();
        
        public Dictionary<string, int> DeltaStackableItems = new Dictionary<string, int>();

        //Character name, Character progress dictionary. Enables multiple character deltas from same map
        public Dictionary<string,CharacterProgress> characterProgress = new Dictionary<string, CharacterProgress>();

        public Stopwatch zoneTimer = new Stopwatch();

        public DateTime timeEntered;
        public DateTime timeLeft;

        public double ConfirmedChaosAdded = 0;
        public double TentativeChaosAdded = 0;
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
            Dictionary<string, int> deltaStackableItems = new Dictionary<string, int>();
            (deltaStackableItems, addedNonStackableItems, removedNonStackableItems) = deltaCalculator.CalculateDelta(ZoneName);

            AddedNonStackableItems.AddRange(addedNonStackableItems);
            RemovedNonStackableItems.AddRange(removedNonStackableItems);

            DeltaStackableItems = Toolbox.AddDictionaries(DeltaStackableItems, deltaStackableItems);
            
            CharacterProgress progress = experienceCalculator.CalculateDelta();
            characterProgress[progress.Name] = progress;
        }
        
        public string ToJSON() {

            JProperty itemsAdded = new JProperty("itemsAdded", JArray.FromObject(AddedNonStackableItems));
            JProperty itemsRemoved = new JProperty("itemsRemoved", JArray.FromObject(RemovedNonStackableItems));
            JProperty stackableItemsDelta = new JProperty("stackableItemDelta", JObject.FromObject(DeltaStackableItems));
            JProperty charProgress = new JProperty("characterProgres", JObject.FromObject(characterProgress));
            JProperty zoneName = new JProperty("zoneName", ZoneName);
            JProperty zoneID = new JProperty("zoneID", ZoneID);
            JProperty mpMods = new JProperty("mods", JArray.FromObject(mapMods));
            JProperty timeInZone = new JProperty("secondsInZone", zoneTimer.ElapsedMilliseconds / 1000);
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
            var countRemoved = AddedNonStackableItems.RemoveAll(x => x.Id == item.Id);
            if(countRemoved != 1) {
                throw new Exception("Tried to remove item from zone, with item id:" + item.Id + " || Could not remove single");
            }
        }

        public void CalculatZoneWorth(ItemValuator itemValuator) {


            TentativeChaosAdded = 0;
            ConfirmedChaosAdded = 0;
            ConfirmedChaosRemoved = 0;

            foreach (Item i in AddedNonStackableItems) {
                var value = itemValuator.ItemChaosValue(i);
                ItemValue itemValue = new ItemValue();
                itemValue.CurrentChaosValue = value.Item1;
                itemValue.valueMode = value.Item2;
                itemValue.setAt = LastExitedZone;
                itemValue.zoneID = ZoneID;

                i.itemValues.Values.Add(itemValue);
                i.itemValues.CurrentChaosValue = value.Item1;
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
