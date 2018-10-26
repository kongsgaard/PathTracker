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

        ISettings Settings;
        
        MongoClient client = null;

        public MongoDBSaver(ISettings settings) {
            Settings = settings;
            client = new MongoClient(Settings.GetValue("MongoDBConnectionString"));
            
        }

        public void SaveToDisk(Zone zone) {

            var db = client.GetDatabase(Settings.GetValue("MongoDBDatabaseName"));

            BsonDocument doc = BsonDocument.Parse(zone.ToJSON());

            var docs = db.GetCollection<BsonDocument>("ZoneDocuments");

            docs.InsertOneAsync(doc);

            Console.WriteLine("Write zone info to MongoDB at " + Settings.GetValue("MongoDBConnectionString"));
        }
        
    }
}
