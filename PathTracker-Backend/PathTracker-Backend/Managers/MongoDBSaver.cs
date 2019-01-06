using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Core.Operations;
using System.Diagnostics;
using System.IO;


namespace PathTracker_Backend
{
    public class MongoDBSaver : IDiskSaver, IDisposable
    {

        ISettings Settings;
        
        MongoClient client = null;
        Process mongod = null;

        public MongoDBSaver(ISettings settings) {
            Settings = settings;
            client = new MongoClient(Settings.GetValue("MongoDBConnectionString"));

            if (!IsServerConnceted) {
                StartupMongoDBClient();
            }
            
        }

        public void Dispose() {
            if (mongod != null) {
                mongod.CloseMainWindow();
            }
        }

        ~MongoDBSaver() {
            if(mongod != null) {
                mongod.CloseMainWindow();
            }
        }

        public bool IsServerConnceted {
            get {
                return client.Cluster.Description.Servers.Single().State == MongoDB.Driver.Core.Servers.ServerState.Connected;
            }
        }

        private void StartupMongoDBClient() {
            //starting the mongod server (when app starts)
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = Settings.GetValue("MongoDBExePath");
            //start.WindowStyle = ProcessWindowStyle.Hidden;

            string MongoDBDataDir = Directory.GetCurrentDirectory() + "\\MongoDBData";
            if(!Directory.Exists(MongoDBDataDir)) {
                Directory.CreateDirectory(MongoDBDataDir);
            }
            start.Arguments = "--dbpath " + MongoDBDataDir;

            mongod = Process.Start(start);
            
        }

        public void SaveToDisk(Zone zone) {

            var db = client.GetDatabase(Settings.GetValue("MongoDBDatabaseName"));

            //BsonDocument doc = BsonDocument.Parse(zone.ToJSON());

            var docs = db.GetCollection<BsonDocument>(Settings.GetValue("MongoDBCollectionName"));
            
            docs.InsertOneAsync(zone.ToBsonDocument());
            
            foreach(Item addedItem in zone.AddedNonStackableItems) {

                var mapDocs = db.GetCollection<ItemIDToZoneID>(Settings.GetValue("MongoDBCollectionName") + "_ItemZoneMap");
                var filter = Builders<ItemIDToZoneID>.Filter.Eq(x => x.ItemID, addedItem.itemId);
                var options = new UpdateOptions();
                options.IsUpsert = true;
                mapDocs.ReplaceOne(filter, new ItemIDToZoneID(addedItem.itemId, zone.ZoneID), options);

            }

            Console.WriteLine("Write zone("+ zone.ZoneName + ") info to MongoDB at " + Settings.GetValue("MongoDBConnectionString"));
        }

        private HashSet<string> KnownItems = new HashSet<string>();

        public void DropCollection(string collectionName) {
            var db = client.GetDatabase(Settings.GetValue("MongoDBDatabaseName"));
            db.DropCollection(collectionName);

            db.DropCollection(collectionName + "_ItemZoneMap");
        }

        public void UpdateZoneWithItemValue(Item item, ItemValuator itemValuator, ItemChangeType changeType, Zone fromZone) {

            var db = client.GetDatabase(Settings.GetValue("MongoDBDatabaseName"));
            
            var docs = db.GetCollection<ItemIDToZoneID>(Settings.GetValue("MongoDBCollectionName") + "_ItemZoneMap");

            var query = from p in docs.AsQueryable()
                        where p.ItemID == item.itemId
                        select p.CurrentZoneID;

            var list = query.ToList();

            if(list.Count > 1) {
                throw new Exception("ItemZoneMap should not contain more than 1 element per item, contained multiple for item with id:" + item.itemId);
            }
            else if(list.Count < 1) {
                //Cant find item from previous zone, so insert into map, and add to from zome
                var filter = Builders<ItemIDToZoneID>.Filter.Eq(x => x.ItemID, item.itemId);
                var options = new UpdateOptions();
                options.IsUpsert = true;
                docs.ReplaceOne(filter, new ItemIDToZoneID(item.itemId, fromZone.ZoneID), options);

                fromZone.AddedNonStackableItems.Add(item);
            }
            else {
                
                string oldZoneID = list.First();

                var zoneDocs = db.GetCollection<Zone>(Settings.GetValue("MongoDBCollectionName"));

                var zoneQuery = from z in zoneDocs.AsQueryable()
                                where z.ZoneID == oldZoneID
                                select z;

                var zoneResult = zoneQuery.ToList();

                if (zoneResult.Count() != 1) {
                    throw new Exception("Could not return single zone from Zonedocs with id:" + oldZoneID);
                }
                else {
                    Zone oldZone = zoneResult.First();

                    if(changeType == ItemChangeType.EnchantedModChanged) {
                        oldZone.RemoveItem(item);

                        oldZone.CalculatZoneWorth(itemValuator);

                        var varZoneFilter = Builders<Zone>.Filter.Eq(s => s.ZoneID, oldZoneID);
                        var zoneReplaceResult = zoneDocs.ReplaceOne(varZoneFilter, oldZone);

                        fromZone.AddedNonStackableItems.Add(item);

                        //Update map in database to new currentZone
                        var updateFilter = Builders<ItemIDToZoneID>.Filter.Eq(s => s.ItemID, item.itemId);
                        var updateStatement = Builders<ItemIDToZoneID>.Update.Set(s => s.CurrentZoneID, fromZone.ZoneID);
                        var result = docs.UpdateMany(updateFilter, updateStatement);
                    }
                    else if(changeType == ItemChangeType.NoteChanged) {
                        oldZone.UpdateItem(item);
                        oldZone.CalculatZoneWorth(itemValuator);

                        var varZoneFilter = Builders<Zone>.Filter.Eq(s => s.ZoneID, oldZoneID);
                        var zoneReplaceResult = zoneDocs.ReplaceOne(varZoneFilter, oldZone);
                    }
                    else {
                        throw new Exception("Changed item with unsupported itemchangetype found");
                    }
                    
                }

                
            }
        }


    }

    public class ItemIDToZoneID {

        public ItemIDToZoneID(string itemID, string zoneID) {
            ItemID = itemID;
            CurrentZoneID = zoneID;
        }

        public ItemIDToZoneID() { }

        [BsonId]
        public string _id { get; set; }

        [BsonElement(elementName: "ItemID")]
        public string ItemID { get; set; }

        [BsonElement(elementName: "CurrentZoneID")]
        public string CurrentZoneID { get; set; }    
    }

}
