using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using System.IO;

namespace PathTracker_Backend
{
    public class DiskFolderSaver : IDiskSaver 
    {
        
        private SettingsManager Settings = SettingsManager.Instance;

        string currentDir;

        MongoClient client = null;

        public DiskFolderSaver() {
            
            currentDir = Directory.GetCurrentDirectory() + Settings.GetValue("DiskSaverFolderPath");

            if (!Directory.Exists(currentDir)) {
                Directory.CreateDirectory(currentDir);
            }
        }

        public void SaveToDisk(Zone zone) {
            
            File.WriteAllText(currentDir + zone.ZoneID + ".json", zone.ToJSON());

            Console.WriteLine("Write to zone to json at :" + currentDir + zone.ZoneID + ".json");
        }

        public void UpdateItemValue(Item item) {
            throw new NotImplementedException();
        }
    }
}
