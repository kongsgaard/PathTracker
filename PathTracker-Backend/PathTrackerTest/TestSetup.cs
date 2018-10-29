using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PathTracker_Backend;

namespace PathTrackerTest {
    public class TestSetup {

        MockWebRequestManager mockWebRequest;
        MockZonePropertyExtractor mockZoneProperty;
        MockCurrenyRates mockCurreny;
        MongoDBSaver dBSaver;
        MockSettings settings;

        public TestSetup(MockWebRequestManager mockWebRequestManager, MockZonePropertyExtractor mockZonePropertyExtractor, MockCurrenyRates mockCurrenyRates,
                        MongoDBSaver mongoDBSaver, MockSettings mockSettings) {
            mockWebRequest = mockWebRequestManager;
            mockZoneProperty = mockZonePropertyExtractor;
            mockCurreny = mockCurrenyRates;
            dBSaver = mongoDBSaver;
            settings = mockSettings;
        }

        private Inventory currentInventory = new Inventory();
        private Dictionary<string,StashApiRequest> currentStash = new Dictionary<string, StashApiRequest>();

        private List<string> ClientTxtLines = new List<string>();
        private List<string> zoneIDs = new List<string>();
        public void NewZone(string newZoneName, string ZoneID) {

            ClientTxtLines.Add($"2018/10/16 17:06:50 32531406 a34 [INFO Client 11332] : You have entered {newZoneName}");
            zoneIDs.Add(ZoneID);



        }

        public void InitializeInventory(Inventory initial) {
            currentInventory = initial;
        }

        public void InitializeStashTab(StashApiRequest apiRequest, string StashName) {
            currentStash[StashName] = apiRequest;
        }

        public void AddItemsToStash(string stashName, List<Item> items) {
            var stash = currentStash[stashName];
            stash.Items.AddRange(items);
        }

        public void AddItemsToInventory(List<Item> items) {
            currentInventory.Items.AddRange(items);
        }

        public void UpdateItemNoteStash(string itemID, string newNote, string stashName) {
            var stash = currentStash[stashName];

            var item = stash.Items.Single(x => x.Id == itemID);
            item.Note = newNote;
        }

        public void SetupSettings() {
            string CurrentDir = Directory.GetCurrentDirectory();

            settings.SetValue("Account", "TestAccount");
            settings.SetValue("CurrentCharacter", "SpydigeSander");
            settings.SetValue("CurrentLeague", "Standard");
            settings.SetValue("ClientTxtPath", CurrentDir + "//TestData//Client.txt");
            settings.SetValue("MinimapFolder", CurrentDir + "//Minimap//");
            settings.SetValue("DeleteOldMinimapFiles", "false");
            settings.SetValue("TesseractDict", "D:\\Tesseract\\Tesseract-OCR");
            settings.SetValue("MongoDBConnectionString", "mongodb://127.0.0.1:27017");
            settings.SetValue("MongoDBDatabaseName", "PathTrackerTest");

        }



    }
}
