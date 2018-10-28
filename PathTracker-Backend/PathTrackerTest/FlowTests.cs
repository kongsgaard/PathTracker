using System;
using PathTracker_Backend;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;



namespace PathTrackerTest {
    [TestClass]
    public class FlowTests {
        [TestMethod]
        public void FlowTest() {

            LogCreator.Setup();

            ISettings settings = new MockSettings();
            SetupSettings(settings);

            IDiskSaver mongoDiskSaver = new MongoDBSaver(settings);
            IWebRequestManager webRequestManager = new MockWebRequestManager();
            IZonePropertyExtractor zonePropertyExtractor = new MockZonePropertyExtractor();

            ICurrencyRates currencyRates = new MockCurrenyRates();

            WriteLineToFile("First line", settings.GetValue("ClientTxtPath"), FileMode.Append);

            var LinesForClientTxt = SetupZonesForTesting((MockWebRequestManager)webRequestManager, (MockZonePropertyExtractor)zonePropertyExtractor);

            ComponentManager manager = new ComponentManager(mongoDiskSaver, webRequestManager, zonePropertyExtractor, settings, currencyRates);

            Task t = new Task(() => manager.StartClientTxtListener());
            t.Start();
            System.Threading.Thread.Sleep(500); //Wait for ClientTxtListenrer to start
            manager.StartInventoryListener();

            System.Threading.Thread.Sleep(500);

            foreach(string line in LinesForClientTxt) {
                WriteLineToFile(line, settings.GetValue("ClientTxtPath"), FileMode.Append);
                System.Threading.Thread.Sleep(2000);
            }

        }

        private ISettings SetupSettings(ISettings settings) {

            string CurrentDir = Directory.GetCurrentDirectory();

            settings.SetValue("CurrentCharacter", "SpydigeSander");
            settings.SetValue("CurrentLeague", "Standard");
            settings.SetValue("ClientTxtPath", CurrentDir+"//TestData//Client.txt");
            settings.SetValue("MinimapFolder", "C:/Users/emilk/Documents/My Games/Path of Exile/Minimap");
            settings.SetValue("DeleteOldMinimapFiles", "true");
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

        private List<string> SetupZonesForTesting(MockWebRequestManager webRequestManager, MockZonePropertyExtractor zonePropertyExtractor) {
            TestData data = new TestData();

            List<string> textLines = new List<string>();

            textLines.Add(@"2018 / 10 / 16 17:06:50 32531406 a34[INFO Client 11332] : You have entered Wasteland.");
            textLines.Add(@"2018/10/16 17:05:36 32457343 a34 [INFO Client 11332] : You have entered Enlightened Hideout.");

            Inventory i1 = data.GetTestDataInventory();
            Inventory i2 = data.GetTestDataInventory();

            Item newItem = data.GetTestDataLeatherBelt();
            i2.Items.Add(newItem);

            webRequestManager.AddInventoryToQueue(i1, "SpydigeSander");
            webRequestManager.AddInventoryToQueue(i2, "SpydigeSander");
            
            return textLines;
        }

        private Inventory GetTestInventory(string character) {
            Inventory inventory = new Inventory();

            inventory.Character = new Character();
            inventory.Character.Name = character;
            

            return inventory;
        }
    }
}
