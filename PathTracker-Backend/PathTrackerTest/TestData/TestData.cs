using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PathTracker_Backend;
using Newtonsoft.Json;
using System.IO;

namespace PathTrackerTest {
    public class TestData {

        public Inventory GetTestDataInventory() {

            string currentDir = Directory.GetCurrentDirectory();
            Inventory inv = JsonConvert.DeserializeObject<Inventory>(File.ReadAllText(currentDir + "/TestData/TestDataInventory.json"));

            return inv;
        }

        public StashApiRequest GetTestDataQuadStash() {
            string currentDir = Directory.GetCurrentDirectory();
            StashApiRequest tab = JsonConvert.DeserializeObject<StashApiRequest>(File.ReadAllText(currentDir + "/TestData/TestDataStashTabQuad.json"));

            return tab;
        }

        public StashApiRequest GetTestDataCurrencyStash() {
            string currentDir = Directory.GetCurrentDirectory();
            StashApiRequest tab = JsonConvert.DeserializeObject<StashApiRequest>(File.ReadAllText(currentDir + "/TestData/TestDataStashTabCurrency.json"));

            return tab;
        }

        public Item GetTestDataLeatherBelt() {
            string currentDir = Directory.GetCurrentDirectory();
            Item item = JsonConvert.DeserializeObject<Item>(File.ReadAllText(currentDir + "/TestData/TestDataStashItemLeatherBelt.json"));

            return item;
        }

    }
}
