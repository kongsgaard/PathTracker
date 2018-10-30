using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PathTracker_Backend;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Linq;

namespace PathTrackerTest {
    public class MongoDBAsserter {
        ISettings Settings;

        MongoClient client = null;

        public MongoDBAsserter(ISettings settings) {
            Settings = settings;
            client = new MongoClient(Settings.GetValue("MongoDBConnectionString"));
        }

        private Zone GetSingleZone(string ZoneID) {
            var db = client.GetDatabase(Settings.GetValue("MongoDBDatabaseName"));

            var docs = db.GetCollection<Zone>(Settings.GetValue("MongoDBCollectionName"));

            var query = from z in docs.AsQueryable()
                        where z.ZoneID == ZoneID
                        select z;

            var list = query.ToList();

            if (list.Count != 1) {
                throw new Exception();
            }
            else
                return list.First();
        }

        //Return true if the item was added to the ZoneID
        public bool ZoneAddedItems(Item item, string ZoneID) {

            Zone zone = GetSingleZone(ZoneID);

            Item i = zone.AddedNonStackableItems.Single(x => x.itemId == item.itemId);

            if (i.name != item.name) {
                return false;
            }

            return true;
        }

        //Return true if the item was not added to the ZoneID
        public bool ZoneNotAddedItems(Item item, string ZoneID) {

            Zone zone = GetSingleZone(ZoneID);

            var i = zone.AddedNonStackableItems.Where(x => x.itemId == item.itemId).ToList();

            if (i.Count > 0) {
                return false;
            }

            return true;

        }

        public bool ItemZoneMapPair(string itemID, string ZoneID) {
            var db = client.GetDatabase(Settings.GetValue("MongoDBDatabaseName"));

            var docs = db.GetCollection<ItemIDToZoneID>(Settings.GetValue("MongoDBCollectionName") + "_ItemZoneMap");

            var query = from map in docs.AsQueryable()
                        where map.ItemID == itemID && map.CurrentZoneID == ZoneID
                        select map;

            var list = query.ToList();

            if (list.Count != 1) {
                throw new Exception();
            }
            else
                return true;
        }

        public bool ZoneConfirmedChaosValue(double chaosAdded, string ZoneID) {
            Zone zone = GetSingleZone(ZoneID);

            if (zone.ConfirmedChaosAdded == chaosAdded) {
                return true;
            }
            else
                return false;
        }

        public bool ZoneTentativeChaosValue(double chaosAdded, string ZoneID) {
            Zone zone = GetSingleZone(ZoneID);

            if (zone.TentativeChaosAdded == chaosAdded) {
                return true;
            }
            else
                return false;
        }
    }
}
