using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PathTracker_Backend;

namespace PathTrackerTest {
    public class TestSetup {

        public MockWebRequestManager mockWebRequest;
        public MockZonePropertyExtractor mockZoneProperty;
        public MockCurrenyRates mockCurreny;
        public MongoDBSaver dBSaver;
        public MockSettings settings;
        public ResourceManager resourceManager;

        private string currentCharacter = "";
        private string currentLeague = "";

        public TestSetup() {

            settings = new MockSettings();
            SetupSettings();

            mockWebRequest = new MockWebRequestManager(settings);
            mockZoneProperty = new MockZonePropertyExtractor();
            mockCurreny = new MockCurrenyRates();
            mockCurreny.UpdateOnce();
            dBSaver = new MongoDBSaver(settings);
            resourceManager = new ResourceManager();

            currentCharacter = settings.GetValue("CurrentCharacter");
            currentLeague = settings.GetValue("CurrentLeague");

            WriteLineToFile("First line", settings.GetValue("ClientTxtPath"), FileMode.Create);
            Directory.CreateDirectory(settings.GetValue("MinimapFolder"));

        }

        public void RunTest() {
            System.Threading.Thread.Sleep(150);

            for (int i = 0; i < ClientTxtLines.Count; i++) {
                WriteLineToFile("ZoneInfo", settings.GetValue("MinimapFolder") + zoneIDs[i], FileMode.Append);
                System.Threading.Thread.Sleep(150);
                WriteLineToFile(ClientTxtLines[i], settings.GetValue("ClientTxtPath"), FileMode.Append);
                System.Threading.Thread.Sleep(1500);
                
                
                
            }
        }

        private Inventory currentInventory = new Inventory();
        private Dictionary<string,StashApiRequest> currentStash = new Dictionary<string, StashApiRequest>();

        public void Login(string ZoneID) {
            zoneIDs.Add(ZoneID);
        }

        private List<string> ClientTxtLines = new List<string>();
        private List<string> zoneIDs = new List<string>();
        public void NewZone(string newZoneName, string ZoneID) {

            ClientTxtLines.Add($"2018/10/16 17:06:50 32531406 a34 [INFO Client 11332] : You have entered {newZoneName}");
            zoneIDs.Add(ZoneID);

            mockWebRequest.AddInventoryToQueue(Toolbox.Clone(currentInventory), currentCharacter);
            foreach(string tab in StashTabs) {
                mockWebRequest.AddStashtabToQueue(Toolbox.Clone(currentStash[tab]), currentLeague, tab);
            }

            mockZoneProperty.AddMapModsToQueue(new List<MapMod>());

        }

        public void InitializeInventory(Inventory initial) {
            currentInventory = initial;
        }

        List<string> StashTabs = new List<string>();
        public void InitializeStashTab(StashApiRequest apiRequest, string StashName) {
            currentStash[StashName] = apiRequest;
            StashTabs.Add(StashName);
        }

        public void AddItemsToStash(string stashName, List<Item> items) {
            var stash = currentStash[stashName];
            stash.Items.AddRange(items);
        }

        public void AddItemsToInventory(List<Item> items) {
            foreach (Item i in items) {
                i.inventoryId = "MainInventory";
                currentInventory.Items.Add(i);
            }
        }

        public void UpdateItemNoteStash(string itemID, string newNote, string stashName) {
            var stash = currentStash[stashName];

            var item = stash.Items.Single(x => x.itemId == itemID);
            item.note = newNote;
        }

        public void SetupSettings() {
            string CurrentDir = Directory.GetCurrentDirectory();

            settings.SetValue("Account", "TestAccount");
            settings.SetValue("CurrentCharacter", "SpydigeSander");
            settings.SetValue("CurrentLeague", "Standard");
            settings.SetValue("ClientTxtPath", CurrentDir + "//TestData//Client.txt");
            settings.SetValue("MinimapFolder", CurrentDir + "//Minimap//");
            settings.SetValue("DeleteOldMinimapFiles", "false");
            settings.SetValue("TesseractDict", "%TESSERACTDIR%");
            settings.SetValue("MongoDBConnectionString", "mongodb://127.0.0.1:27017");
            settings.SetValue("MongoDBDatabaseName", "PathTrackerTest");

        }

        private void WriteLineToFile(string content, string Path, FileMode mode) {
            using (var sw = new StreamWriter(new FileStream(Path, mode))) {
                sw.WriteLine(content);
            }
        }

    }
}
