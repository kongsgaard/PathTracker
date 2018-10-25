using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;

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

        public void UpdateItemValue(Item item) {

            throw new NotImplementedException();
        }
    }
}
