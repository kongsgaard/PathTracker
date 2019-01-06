using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PathTracker_Backend;
using System.IO;


namespace PathTrackerTest {

    [TestClass]
    public class ComponentTest {

        [TestMethod]
        public void Component_ZonePropertyExtractorMapMods() {
            TestSetup setup = new TestSetup();

            MockProcessScreenshotCapture processScreenshotCapture = new MockProcessScreenshotCapture();

            processScreenshotCapture.SetupImageAddToQueue(Directory.GetCurrentDirectory() + "//TestData//Screenshots//Zone3_Mods.jpeg");
            processScreenshotCapture.SetupImageAddToQueue(Directory.GetCurrentDirectory() + "//TestData//Screenshots//Zone2_Mods.jpeg");
            processScreenshotCapture.SetupImageAddToQueue(Directory.GetCurrentDirectory() + "//TestData//Screenshots//Zone1_NoMods.jpeg");

            ZonePropertyExtractor zonePropertyExtractor = new ZonePropertyExtractor(processScreenshotCapture, setup.settings, setup.resourceManager);
            Zone testZone = new Zone("TestZone1", setup.resourceManager);
            testZone.ZoneID = "TestZone1";

            zonePropertyExtractor.SetZone(testZone);

            List<MapMod> mods_zone1;
            MapModParseStatus parseStatus_zone1; 
            (mods_zone1, parseStatus_zone1) = zonePropertyExtractor.GetMapMods();

            Assert.IsTrue(parseStatus_zone1 == MapModParseStatus.ModsParsed);
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone1, "Twinned"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone1, "Shocking"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone1, "of Vulnerability"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone1, "of Drought"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone1, "of Flames"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone1, "Undead"));

            Assert.AreEqual(mods_zone1.Count,6);
            Assert.IsTrue(mods_zone1.Where(mod => mod.Type == "MapAffix").ToList().Count == 6);

            Zone testZone2 = new Zone("TestZone2", setup.resourceManager);
            testZone2.ZoneID = "TestZone2";

            List<MapMod> mods_zone2;
            MapModParseStatus parseStatus_zone2;
            (mods_zone2, parseStatus_zone2) = zonePropertyExtractor.GetMapMods();
            Assert.IsTrue(parseStatus_zone2 == MapModParseStatus.ModsParsed);
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone2, "of Temporal Chains"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone2, "Splitting"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone2, "MapAtlasMapsHave20PercentQuality"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone2, "Titan's"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone2, "of Drought"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone2, "of Stasis"));

            Assert.AreEqual(mods_zone2.Where(mod => mod.Type == "Sextant").ToList().Count,1);
            Assert.AreEqual(mods_zone2.Where(mod => mod.Type == "MapAffix").ToList().Count,5);
            
            Zone testZone3 = new Zone("TestZone3", setup.resourceManager);
            testZone3.ZoneID = "TestZone3";
            List<MapMod> mods_zone3;
            MapModParseStatus parseStatus_zone3;
            (mods_zone3, parseStatus_zone3) = zonePropertyExtractor.GetMapMods();

            Assert.AreEqual(parseStatus_zone3, MapModParseStatus.NotPresent);
            Assert.AreEqual(mods_zone3.Count, 0);
        }
        
    }
}
