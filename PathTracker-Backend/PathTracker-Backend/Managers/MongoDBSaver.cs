using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Linq;


namespace PathTracker_Backend
{
    public class MongoDBSaver : IDiskSaver 
    {
        
        private SettingsManager Settings = SettingsManager.Instance;

        Dictionary<string, List<string>> ItemIDToZoneIDMap = new Dictionary<string, List<string>>();

        MongoClient client = null;

        public MongoDBSaver() {
            client = new MongoClient(Settings.GetValue("MongoDBConnectionString"));
        }

        public void SaveToDisk(Zone zone) {

            var db = client.GetDatabase("PathTracker");

            BsonDocument doc = BsonDocument.Parse(zone.ToJSON());

            var docs = db.GetCollection<BsonDocument>("ZoneDocuments");

            docs.InsertOneAsync(doc);

            Console.WriteLine("Write zone("+ zone.ZoneName + ") info to MongoDB at " + Settings.GetValue("MongoDBConnectionString"));
        }
        
        public void UpdateItemValue(Item item, ItemValuator itemValuator) {

            var db = client.GetDatabase("PathTracker");
            
            var docs = db.GetCollection<ItemIDToZoneID>("ItemZoneMap");

            var query = from p in docs.AsQueryable()
                        where p.ItemID == item.Id
                        select p.CurrentZoneID;

            var list = query.ToList();

            if(list.Count > 1) {
                throw new Exception("ItemZoneMap should not contain more than 1 element per item, contained multiple for item with id:" + item.Id);
            }
            else if(list.Count < 1) {

                docs.InsertOneAsync(new ItemIDToZoneID(item.Id, item.CurrentZoneID));
            }
            else {

                #region Extract old zone and remove item, and save it back to DB
                string oldZoneID = list.First();

                var zoneDocs = db.GetCollection<Zone>("ZoneDocuments");

                var zoneQuery = from z in zoneDocs.AsQueryable()
                                where z.ZoneID == oldZoneID
                                select z;

                if(zoneQuery.Count() != 1) {
                    throw new Exception("Could not return single zone from Zonedocs with id:" + oldZoneID);
                }
                else {
                    Zone oldZone = zoneQuery.First();

                    oldZone.RemoveItem(item);

                    oldZone.CalculatZoneWorth(itemValuator);

                    var varZoneFilter = Builders<Zone>.Filter.Eq(s => s.ZoneID, oldZoneID);
                    var zoneResult = zoneDocs.ReplaceOne(varZoneFilter, oldZone);
                }
                #endregion

                //Update map in database to new currentZone
                var updateFilter = Builders<ItemIDToZoneID>.Filter.Eq(s => s.ItemID, item.Id);
                var updateStatement = Builders<ItemIDToZoneID>.Update.Set(s => s.CurrentZoneID, item.CurrentZoneID);
                var result = docs.UpdateMany(updateFilter, updateStatement);

            }
        }
    }

    public class ItemIDToZoneID {

        public ItemIDToZoneID(string itemID, string zoneID) {
            ItemID = itemID;
            CurrentZoneID = zoneID;
        }

        public string ItemID { get; set; }

        public string CurrentZoneID { get; set; }    
    }

}
