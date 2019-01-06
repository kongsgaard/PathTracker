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
        
        [TestCleanup]
        public void TestCleanup() {
            TestSetup setup = new TestSetup();

            setup.dBSaver.Dispose();
        }

        /// <summary>
        /// Test adding an item to a zone
        /// </summary>
        [TestMethod]
        public void Flow_AddItemTest() {
            
            TestSetup setup = new TestSetup();
            setup.settings.SetValue("MongoDBCollectionName", "AddItemTest");
            setup.dBSaver.DropCollection(setup.settings.GetValue("MongoDBCollectionName"));
            
            ComponentManager manager = new ComponentManager(setup.dBSaver, setup.mockWebRequest, setup.mockZoneProperty, setup.settings, setup.mockCurreny, setup.resourceManager);
            List<string> StashTabsToListen = new List<string> { "Stash1" };
            Thread mainProgram = new Thread(() => MainProgram(manager, StashTabsToListen));
            mainProgram.Start();

            TestData testData = new TestData();
            setup.InitializeInventory(testData.GetTestDataInventory());
            setup.InitializeStashTab(testData.GetTestDataQuadStash(), "Stash1");

            setup.Login("LoginZone");
            setup.NewZone("StartZone", "StartZone");
            Item itemToAdd = testData.GetTestDataLeatherBelt();
            setup.AddItemsToInventory(new List<Item> { itemToAdd });
            setup.NewZone("FinalZone", "FinalZone");
            setup.NewZone("FinalZoneLast", "FinalZoneLast");

            setup.RunTest();

            MongoDBAsserter mongoDBAsserter = new MongoDBAsserter(setup.settings);

            Assert.IsTrue(mongoDBAsserter.ZoneAddedItems(itemToAdd, "StartZone"));
            Assert.IsTrue(mongoDBAsserter.ZoneNotAddedItems(itemToAdd, "FinalZone"));
        }

        /// <summary>
        /// Test adding an item with a recipe value to a zone
        /// </summary>
        [TestMethod]
        public void Flow_TestRecipeValuator() {

            TestSetup setup = new TestSetup();
            setup.settings.SetValue("MongoDBCollectionName", "TestRecipeValuator");
            setup.dBSaver.DropCollection(setup.settings.GetValue("MongoDBCollectionName"));

            double DivineValue = 11;
            setup.mockCurreny.TypelineChaosValue["Divine Orb"] = DivineValue;

            ComponentManager manager = new ComponentManager(setup.dBSaver, setup.mockWebRequest, setup.mockZoneProperty, setup.settings, setup.mockCurreny, setup.resourceManager);
            List<string> StashTabsToListen = new List<string> { "Stash1" };
            Thread mainProgram = new Thread(() => MainProgram(manager, StashTabsToListen));
            mainProgram.Start();

            TestData testData = new TestData();
            setup.InitializeInventory(testData.GetTestDataInventory());
            setup.InitializeStashTab(testData.GetTestDataQuadStash(), "Stash1");

            setup.Login("LoginZone");
            setup.NewZone("StartZone", "StartZone");
            Item itemToAdd = testData.GetTestDataSixLink();
            setup.AddItemsToStash("Stash1",new List<Item> { itemToAdd });
            setup.NewZone("FinalZone", "FinalZone");

            setup.RunTest();

            MongoDBAsserter mongoDBAsserter = new MongoDBAsserter(setup.settings);

            Assert.IsTrue(mongoDBAsserter.ZoneAddedItems(itemToAdd, "StartZone"));
            Assert.IsTrue(mongoDBAsserter.ZoneConfirmedChaosValue(DivineValue, "StartZone"));
            Assert.IsTrue(mongoDBAsserter.ZoneTentativeChaosValue(0, "StartZone"));
        }

        /// <summary>
        /// Test adding an item with a recipe value and a note value. The note value should decide the price
        /// </summary>
        [TestMethod]
        public void Flow_TestNoteAndRecipeValuator() {
            TestSetup setup = new TestSetup();
            setup.settings.SetValue("MongoDBCollectionName", "TestNoteAndRecipeValuator");
            setup.dBSaver.DropCollection(setup.settings.GetValue("MongoDBCollectionName"));

            double DivineValue = 11;
            double ExaltedValue = 50;
            setup.mockCurreny.TypelineChaosValue["Divine Orb"] = DivineValue;
            setup.mockCurreny.TypelineChaosValue["Exalted Orb"] = ExaltedValue;

            ComponentManager manager = new ComponentManager(setup.dBSaver, setup.mockWebRequest, setup.mockZoneProperty, setup.settings, setup.mockCurreny, setup.resourceManager);
            List<string> StashTabsToListen = new List<string> { "Stash1" };
            Thread mainProgram = new Thread(() => MainProgram(manager, StashTabsToListen));
            mainProgram.Start();

            TestData testData = new TestData();
            setup.InitializeInventory(testData.GetTestDataInventory());
            setup.InitializeStashTab(testData.GetTestDataQuadStash(), "Stash1");

            setup.Login("LoginZone");
            setup.NewZone("StartZone", "StartZone");
            Item itemToAdd = testData.GetTestDataSixLink();
            itemToAdd.note = "~price 1 exa";
            setup.AddItemsToStash("Stash1", new List<Item> { itemToAdd });
            setup.NewZone("FinalZone", "FinalZone");

            setup.RunTest();

            MongoDBAsserter mongoDBAsserter = new MongoDBAsserter(setup.settings);

            Assert.IsTrue(mongoDBAsserter.ZoneAddedItems(itemToAdd, "StartZone"));
            Assert.IsTrue(mongoDBAsserter.ZoneTentativeChaosValue(ExaltedValue, "StartZone"));
            Assert.IsTrue(mongoDBAsserter.ZoneConfirmedChaosValue(0, "StartZone"));
        }

        /// <summary>
        /// Test adding an item with a note, and later changing it, seeing the tenative value is updated in the earlier zone
        /// </summary>
        [TestMethod]
        public void Flow_TestNoteChange() {
            TestSetup setup = new TestSetup();
            setup.settings.SetValue("MongoDBCollectionName", "TestNoteChange");
            setup.dBSaver.DropCollection(setup.settings.GetValue("MongoDBCollectionName"));
            
            double ExaltedValue = 50;
            setup.mockCurreny.TypelineChaosValue["Exalted Orb"] = ExaltedValue;

            ComponentManager manager = new ComponentManager(setup.dBSaver, setup.mockWebRequest, setup.mockZoneProperty, setup.settings, setup.mockCurreny, setup.resourceManager);
            List<string> StashTabsToListen = new List<string> { "Stash1" };
            Thread mainProgram = new Thread(() => MainProgram(manager, StashTabsToListen));
            mainProgram.Start();

            TestData testData = new TestData();
            setup.InitializeInventory(testData.GetTestDataInventory());
            setup.InitializeStashTab(testData.GetTestDataQuadStash(), "Stash1");

            setup.Login("LoginZone");

            setup.NewZone("StartZone", "StartZone");
            Item itemToAdd = testData.GetTestDataLeatherBelt();
            itemToAdd.note = "~price 1 exa";
            setup.AddItemsToStash("Stash1", new List<Item> { itemToAdd });

            setup.NewZone("SecondZone", "SecondZone");
            setup.UpdateItemNoteStash(itemToAdd.itemId, "~price 2 exa", "Stash1");

            setup.NewZone("FinalZone", "FinalZone");
            
            setup.RunTest();

            MongoDBAsserter mongoDBAsserter = new MongoDBAsserter(setup.settings);

            Assert.IsTrue(mongoDBAsserter.ZoneAddedItems(itemToAdd, "StartZone"));
            Assert.IsTrue(mongoDBAsserter.ZoneTentativeChaosValue(2 * ExaltedValue, "StartZone"));
            Assert.IsTrue(mongoDBAsserter.ZoneNotAddedItems(itemToAdd, "SecondZone"));
            Assert.IsTrue(mongoDBAsserter.ZoneTentativeChaosValue(0, "SecondZone"));
            Assert.IsTrue(mongoDBAsserter.ItemZoneMapPair(itemToAdd.itemId, "StartZone"));
        }

        [TestMethod]
        public void Flow_ZoneExtractModsToDB() {
            TestSetup setup = new TestSetup();
            setup.settings.SetValue("MongoDBCollectionName", "ZoneExtractModsToDB");
            setup.dBSaver.DropCollection(setup.settings.GetValue("MongoDBCollectionName"));

            
            MockProcessScreenshotCapture processScreenshotCapture = new MockProcessScreenshotCapture();
            processScreenshotCapture.SetupImageAddToQueue(Directory.GetCurrentDirectory() + "//TestData//Screenshots//Zone3_Mods.jpeg");
            ZonePropertyExtractor zonePropertyExtractor = new ZonePropertyExtractor(processScreenshotCapture, setup.settings, setup.resourceManager);
            
            ComponentManager manager = new ComponentManager(setup.dBSaver, setup.mockWebRequest, zonePropertyExtractor, setup.settings, setup.mockCurreny, setup.resourceManager);

            TestData testData = new TestData();
            setup.InitializeInventory(testData.GetTestDataInventory());

            List<string> StashTabsToListen = new List<string> { };
            Thread mainProgram = new Thread(() => MainProgram(manager, StashTabsToListen));
            mainProgram.Start();

            setup.Login("LoginZone");
            setup.NewZone("StartZone", "StartZone", 15000);
            setup.NewZone("FinalZone", "FinalZone");
            setup.RunTest();
        }

        public void MainProgram(ComponentManager manager, List<string> tabs) {
            Task t = new Task(() => manager.StartClientTxtListener());
            t.Start();
            System.Threading.Thread.Sleep(100); //Wait for ClientTxtListenrer to start
            
            Task t1 = new Task(manager.StartInventoryListener);
            t1.Start();
            
            
            foreach(string tab in tabs) {
                Task t2 = new Task(() => manager.StartStashtabListener(tab));
                t2.Start();
            }
            
            Task.WaitAll(t);
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
            newItem.inventoryId = "MainInventory";
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
