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
        public void TestMethod1() {

            LogCreator.Setup();

            ISettings settings = new MockSettings();
            SetupSettings(settings);

            IDiskSaver mongoDiskSaver = new MongoDBSaver(settings);
            IWebRequestManager webRequestManager = new MockWebRequestManager();
            IZonePropertyExtractor zonePropertyExtractor = new MockZonePropertyExtractor();

            ICurrencyRates currencyRates = new MockCurrenyRates();

            WriteLineToFile("First line", settings.GetValue("ClientTxtPath"), FileMode.Create);

            ComponentManager manager = new ComponentManager(mongoDiskSaver, webRequestManager, zonePropertyExtractor, settings, currencyRates);

            Task t = new Task(() => manager.StartClientTxtListener());
            t.Start();
            System.Threading.Thread.Sleep(2000); //Wait for ClientTxtListenrer to start
            manager.StartInventoryListener();

            System.Threading.Thread.Sleep(2000);

            WriteLineToFile("Second line", settings.GetValue("ClientTxtPath"), FileMode.Append);

            System.Threading.Thread.Sleep(4000);

            Console.ReadLine();
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
    }
}
