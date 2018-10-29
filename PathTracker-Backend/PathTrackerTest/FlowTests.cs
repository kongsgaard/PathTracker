using System;
using PathTracker_Backend;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Threading;



namespace PathTrackerTest {
    [TestClass]
    public class FlowTests {
        [TestMethod]
        public void FlowTest() {

            LogCreator.Setup();

            ISettings settings = new MockSettings();
            SetupSettings(settings);

            MongoDBSaver mongoDiskSaver = new MongoDBSaver(settings);
            MockWebRequestManager webRequestManager = new MockWebRequestManager(settings);
            MockZonePropertyExtractor zonePropertyExtractor = new MockZonePropertyExtractor();

            MockCurrenyRates currencyRates = new MockCurrenyRates();
            currencyRates.UpdateOnce();

            ResourceManager resourceManager = new ResourceManager();

            WriteLineToFile("First line", settings.GetValue("ClientTxtPath"), FileMode.Create);
            Directory.CreateDirectory(settings.GetValue("MinimapFolder"));

            List<string> LinesForClientTxt;
            List<string> zoneMinimapNames;

            (LinesForClientTxt, zoneMinimapNames) = SetupZonesForTesting(webRequestManager, zonePropertyExtractor);

            ComponentManager manager = new ComponentManager(mongoDiskSaver, webRequestManager, zonePropertyExtractor, settings, currencyRates, resourceManager);

            Thread mainProgram = new Thread(() => MainProgram(manager));
            mainProgram.Start();

            System.Threading.Thread.Sleep(2500);

            
            for(int i = 0; i < LinesForClientTxt.Count; i++) {
                WriteLineToFile("ZoneInfo", settings.GetValue("MinimapFolder") + zoneMinimapNames[i], FileMode.Append);
                WriteLineToFile(LinesForClientTxt[i], settings.GetValue("ClientTxtPath"), FileMode.Append);
                System.Threading.Thread.Sleep(2500);
            }
            


            
        }

        public void MainProgram(ComponentManager manager) {
            Task t = new Task(() => manager.StartClientTxtListener());
            t.Start();
            System.Threading.Thread.Sleep(100); //Wait for ClientTxtListenrer to start

            Task t1 = new Task(manager.StartInventoryListener);
            t1.Start();

            Task.WaitAll(t, t1);
        }

        private ISettings SetupSettings(ISettings settings) {

            string CurrentDir = Directory.GetCurrentDirectory();

            settings.SetValue("Account", "TestAccount");
            settings.SetValue("CurrentCharacter", "SpydigeSander");
            settings.SetValue("CurrentLeague", "Standard");
            settings.SetValue("ClientTxtPath", CurrentDir+"//TestData//Client.txt");
            settings.SetValue("MinimapFolder", CurrentDir+"//Minimap//");
            settings.SetValue("DeleteOldMinimapFiles", "false");
            settings.SetValue("TesseractDict", "D:\\Tesseract\\Tesseract-OCR");
            settings.SetValue("MongoDBConnectionString", "mongodb://127.0.0.1:27017");
            settings.SetValue("MongoDBDatabaseName", "PathTrackerTest");

            return settings;
        }

        private void WriteLineToFile(string content, string Path, FileMode mode) {
            using (var sw = new StreamWriter(new FileStream(Path, mode))) {
                sw.WriteLine(content);
            }
        }

        private (List<string>, List<string>) SetupZonesForTesting(MockWebRequestManager webRequestManager, MockZonePropertyExtractor zonePropertyExtractor) {
            TestData data = new TestData();

            List<string> textLines = new List<string>();

            textLines.Add("2018/10/16 17:06:50 32531406 a34 [INFO Client 11332] : You have entered Wasteland1");
            textLines.Add("2018/10/16 17:05:36 32457343 a34 [INFO Client 11332] : You have entered Enlightened Hideout");
            textLines.Add("2018/10/16 17:06:50 32531406 a34 [INFO Client 11332] : You have entered Wasteland2");

            List<string> zoneMinimapNames = new List<string>();
            zoneMinimapNames.Add("Zone1");
            zoneMinimapNames.Add("Zone2");
            zoneMinimapNames.Add("Zone3");


            Inventory i1 = data.GetTestDataInventory();
            Inventory i2 = data.GetTestDataInventory();
            Inventory i3 = data.GetTestDataInventory();

            Item newItem = data.GetTestDataLeatherBelt();
            newItem.InventoryId = "MainInventory";
            i2.Items.Add(newItem);

            webRequestManager.AddInventoryToQueue(i1, "SpydigeSander");
            webRequestManager.AddInventoryToQueue(i2, "SpydigeSander");
            webRequestManager.AddInventoryToQueue(i3, "SpydigeSander");


            zonePropertyExtractor.AddMapModsToQueue(new List<MapMod>());
            zonePropertyExtractor.AddMapModsToQueue(new List<MapMod>());
            zonePropertyExtractor.AddMapModsToQueue(new List<MapMod>());


            return (textLines, zoneMinimapNames);
        }

        private Inventory GetTestInventory(string character) {
            Inventory inventory = new Inventory();

            inventory.Character = new Character();
            inventory.Character.Name = character;
            

            return inventory;
        }
    }
}
