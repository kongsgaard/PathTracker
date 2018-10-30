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

        private ISettings Settings;

        string currentDir;

        MongoClient client = null;

        public DiskFolderSaver(ISettings settings) {
            
            currentDir = Directory.GetCurrentDirectory() + Settings.GetValue("DiskSaverFolderPath");

            if (!Directory.Exists(currentDir)) {
                Directory.CreateDirectory(currentDir);
            }

            Settings = settings;
        }

        public void SaveToDisk(Zone zone) {
            
            File.WriteAllText(currentDir + zone.ZoneID + ".json", zone.ToJSON());

            Console.WriteLine("Write to zone to json at :" + currentDir + zone.ZoneID + ".json");
        }

        public void UpdateZoneWithItemValue(Item item, ItemValuator itemValuator, ItemChangeType changeType, Zone fromZone) {
            throw new NotImplementedException();
        }
    }
}
