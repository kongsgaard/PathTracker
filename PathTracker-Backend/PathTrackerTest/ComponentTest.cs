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
            
            ZonePropertyExtractor zonePropertyExtractor = new ZonePropertyExtractor(processScreenshotCapture, setup.settings, setup.resourceManager);

            //-------------------------------------  Testzone1  -------------------------------------
            #region TestZone1
            processScreenshotCapture.SetupImageAddToQueue(Directory.GetCurrentDirectory() + "//TestData//Screenshots//Zone3_Mods.jpeg");
            Zone testZone = new Zone("TestZone1", setup.resourceManager);
            testZone.ZoneID = "TestZone1";
            zonePropertyExtractor.SetZone(testZone);

            List<MapMod> mods_zone1;
            ParseStatus parseStatus_zone1;
            ZoneInfo zone1Info;
            ZoneProperty zone1Property = zonePropertyExtractor.GetZoneProperties();

            mods_zone1 = zone1Property.mapMods;
            parseStatus_zone1 = zone1Property.mapModsParseStatus;
            zone1Info = zone1Property.zoneInfo;

            Assert.IsTrue(parseStatus_zone1 == ParseStatus.Parsed);
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone1, "Twinned"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone1, "Shocking"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone1, "of Vulnerability"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone1, "of Drought"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone1, "of Flames"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone1, "Undead"));

            Assert.AreEqual(mods_zone1.Count,6);
            Assert.IsTrue(mods_zone1.Where(mod => mod.Type == "MapAffix").ToList().Count == 6);

            Assert.AreEqual(zone1Property.zoneInfoParseStatus, ParseStatus.Parsed);
            Assert.AreEqual(zone1Info.monsterLevelNumeric, "73");
            #endregion

            //-------------------------------------  Testzone2  -------------------------------------
            #region TestZone2
            processScreenshotCapture.SetupImageAddToQueue(Directory.GetCurrentDirectory() + "//TestData//Screenshots//Zone2_Mods.jpeg");
            Zone testZone2 = new Zone("TestZone2", setup.resourceManager);
            testZone2.ZoneID = "TestZone2";
            zonePropertyExtractor.SetZone(testZone2);

            List<MapMod> mods_zone2;
            ParseStatus parseStatus_zone2;
            ZoneInfo zone2Info;
            ZoneProperty zone2Property = zonePropertyExtractor.GetZoneProperties();
            mods_zone2 = zone2Property.mapMods;
            parseStatus_zone2 = zone2Property.mapModsParseStatus;
            zone2Info = zone2Property.zoneInfo;

            Assert.IsTrue(parseStatus_zone2 == ParseStatus.Parsed);
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone2, "of Temporal Chains"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone2, "Splitting"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone2, "MapAtlasMapsHave20PercentQuality"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone2, "Titan's"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone2, "of Drought"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone2, "of Stasis"));

            Assert.AreEqual(mods_zone2.Where(mod => mod.Type == "Sextant").ToList().Count,1);
            Assert.AreEqual(mods_zone2.Where(mod => mod.Type == "MapAffix").ToList().Count,5);

            Assert.AreEqual(zone2Property.zoneInfoParseStatus, ParseStatus.Parsed);
            Assert.AreEqual(zone2Info.monsterLevelNumeric, "74");
            #endregion

            //-------------------------------------  Testzone3  -------------------------------------
            #region TestZone3
            processScreenshotCapture.SetupImageAddToQueue(Directory.GetCurrentDirectory() + "//TestData//Screenshots//Zone1_NoMods.jpeg");
            Zone testZone3 = new Zone("TestZone3", setup.resourceManager);
            testZone3.ZoneID = "TestZone3";
            zonePropertyExtractor.SetZone(testZone3);
            List<MapMod> mods_zone3;
            ParseStatus parseStatus_zone3;
            ZoneInfo zone3Info;

            ZoneProperty zone3Property = zonePropertyExtractor.GetZoneProperties();
            mods_zone3 = zone3Property.mapMods;
            parseStatus_zone3 = zone3Property.mapModsParseStatus;
            zone3Info = zone3Property.zoneInfo;

            Assert.AreEqual(parseStatus_zone3, ParseStatus.NotPresent);
            Assert.AreEqual(mods_zone3.Count, 0);

            Assert.AreEqual(zone3Property.zoneInfoParseStatus, ParseStatus.NotPresent);
            Assert.AreEqual(zone3Info.monsterLevelNumeric, null);
            #endregion

            //-------------------------------------  Testzone4  -------------------------------------
            #region TestZone4
            processScreenshotCapture.SetupImageAddToQueue(Directory.GetCurrentDirectory() + "//TestData//Screenshots//Zone1_Mods.jpeg");
            Zone testZone4 = new Zone("TestZone4", setup.resourceManager);
            testZone4.ZoneID = "TestZone4";
            zonePropertyExtractor.SetZone(testZone4);

            List<MapMod> mods_zone4;
            ParseStatus parseStatus_zone4;
            ZoneInfo zone4Info;
            ZoneProperty zone4Property = zonePropertyExtractor.GetZoneProperties();
            mods_zone4 = zone4Property.mapMods;
            parseStatus_zone4 = zone4Property.mapModsParseStatus;
            zone4Info = zone4Property.zoneInfo;

            Assert.IsTrue(parseStatus_zone4 == ParseStatus.Parsed);
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone4, "of Temporal Chains"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone4, "Armoured"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone4, "Titan's"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone4, "Otherworldly"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone4, "of Drought"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone4, "of Smothering"));

            Assert.AreEqual(mods_zone4.Where(mod => mod.Type == "Sextant").ToList().Count, 0);
            Assert.AreEqual(mods_zone4.Where(mod => mod.Type == "MapAffix").ToList().Count, 6);

            Assert.AreEqual(zone4Property.zoneInfoParseStatus, ParseStatus.Parsed);
            Assert.AreEqual(zone4Info.monsterLevelNumeric, "73");
            #endregion

            //-------------------------------------  Testzone5  -------------------------------------
            #region TestZone5
            processScreenshotCapture.SetupImageAddToQueue(Directory.GetCurrentDirectory() + "//TestData//Screenshots//Zone4_Mods.jpg");
            Zone testZone5 = new Zone("TestZone5", setup.resourceManager);
            testZone5.ZoneID = "TestZone5";
            zonePropertyExtractor.SetZone(testZone5);

            List<MapMod> mods_zone5;
            ParseStatus parseStatus_zone5;
            ZoneInfo zone5Info;
            ZoneProperty zone5Property = zonePropertyExtractor.GetZoneProperties();
            mods_zone5 = zone5Property.mapMods;
            parseStatus_zone5 = zone5Property.mapModsParseStatus;
            zone5Info = zone5Property.zoneInfo;

            Assert.IsTrue(parseStatus_zone5 == ParseStatus.Parsed);
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone5, "of Ice"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone5, "Antagonist's"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone5, "Shocking"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone5, "Abhorrent"));

            Assert.AreEqual(mods_zone5.Where(mod => mod.Type == "Sextant").ToList().Count, 0);
            Assert.AreEqual(mods_zone5.Where(mod => mod.Type == "MapAffix").ToList().Count, 4);

            Assert.AreEqual(zone5Property.zoneInfoParseStatus, ParseStatus.Parsed);
            Assert.AreEqual(zone5Info.monsterLevelNumeric, "77");
            #endregion

            //-------------------------------------  Testzone6  -------------------------------------
            #region TestZone6
            processScreenshotCapture.SetupImageAddToQueue(Directory.GetCurrentDirectory() + "//TestData//Screenshots//Zone5_Mods.jpg");
            Zone testZone6 = new Zone("TestZone6", setup.resourceManager);
            testZone6.ZoneID = "TestZone6";
            zonePropertyExtractor.SetZone(testZone6);

            List<MapMod> mods_zone6;
            ParseStatus parseStatus_zone6;
            ZoneInfo zone6Info;
            ZoneProperty zone6Property = zonePropertyExtractor.GetZoneProperties();
            mods_zone6 = zone6Property.mapMods;
            parseStatus_zone6 = zone6Property.mapModsParseStatus;
            zone6Info = zone6Property.zoneInfo;

            Assert.IsTrue(parseStatus_zone6 == ParseStatus.Parsed);
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone6, "Skeletal"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone6, "of Venom"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone6, "Titan's"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone6, "Otherworldly"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone6, "of Toughness"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone6, "of Miring"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone6, "Conflagrating"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone6, "of Rust"));

            Assert.AreEqual(mods_zone6.Where(mod => mod.Type == "Sextant").ToList().Count, 0);
            Assert.AreEqual(mods_zone6.Where(mod => mod.Type == "MapAffix").ToList().Count, 8);

            Assert.AreEqual(zone6Property.zoneInfoParseStatus, ParseStatus.Parsed);
            Assert.AreEqual(zone6Info.monsterLevelNumeric, "81");
            #endregion


            //-------------------------------------  Testzone7  -------------------------------------
            #region TestZone7
            processScreenshotCapture.SetupImageAddToQueue(Directory.GetCurrentDirectory() + "//TestData//Screenshots//Zone6_Mods.jpg");
            Zone testZone7 = new Zone("TestZone7", setup.resourceManager);
            testZone7.ZoneID = "TestZone7";
            zonePropertyExtractor.SetZone(testZone7);

            List<MapMod> mods_zone7;
            ParseStatus parseStatus_zone7;
            ZoneInfo zone7Info;
            ZoneProperty zone7Property = zonePropertyExtractor.GetZoneProperties();
            mods_zone7 = zone7Property.mapMods;
            parseStatus_zone7 = zone7Property.mapModsParseStatus;
            zone7Info = zone7Property.zoneInfo;

            Assert.IsTrue(parseStatus_zone7 == ParseStatus.Parsed);
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone7, "Armoured"));

            Assert.AreEqual(mods_zone7.Where(mod => mod.Type == "Sextant").ToList().Count, 0);
            Assert.AreEqual(mods_zone7.Where(mod => mod.Type == "MapAffix").ToList().Count, 1);

            Assert.AreEqual(zone7Property.zoneInfoParseStatus, ParseStatus.Parsed);
            Assert.AreEqual(zone7Info.monsterLevelNumeric, "76");
            #endregion

            //-------------------------------------  Testzone8  -------------------------------------
            #region TestZone8
            processScreenshotCapture.SetupImageAddToQueue(Directory.GetCurrentDirectory() + "//TestData//Screenshots//Zone7_Mods.jpg");
            Zone testZone8 = new Zone("TestZone8", setup.resourceManager);
            testZone8.ZoneID = "TestZone8";
            zonePropertyExtractor.SetZone(testZone8);

            List<MapMod> mods_zone8;
            ParseStatus parseStatus_zone8;
            ZoneInfo zone8Info;
            ZoneProperty zone8Property = zonePropertyExtractor.GetZoneProperties();
            mods_zone8 = zone8Property.mapMods;
            parseStatus_zone8 = zone8Property.mapModsParseStatus;
            zone8Info = zone8Property.zoneInfo;

            Assert.IsTrue(parseStatus_zone8 == ParseStatus.Parsed);
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone8, "of Rust"));

            Assert.AreEqual(mods_zone8.Where(mod => mod.Type == "Sextant").ToList().Count, 0);
            Assert.AreEqual(mods_zone8.Where(mod => mod.Type == "MapAffix").ToList().Count, 1);

            Assert.AreEqual(zone8Property.zoneInfoParseStatus, ParseStatus.Parsed);
            Assert.AreEqual(zone8Info.monsterLevelNumeric, "74");
            #endregion

            //-------------------------------------  Testzone9  -------------------------------------
            #region TestZone9
            processScreenshotCapture.SetupImageAddToQueue(Directory.GetCurrentDirectory() + "//TestData//Screenshots//Zone8_Mods.jpg");
            Zone testZone9 = new Zone("TestZone9", setup.resourceManager);
            testZone9.ZoneID = "TestZone9";
            zonePropertyExtractor.SetZone(testZone9);

            List<MapMod> mods_zone9;
            ParseStatus parseStatus_zone9;
            ZoneInfo zone9Info;
            ZoneProperty zone9Property = zonePropertyExtractor.GetZoneProperties();
            mods_zone9 = zone9Property.mapMods;
            parseStatus_zone9 = zone9Property.mapModsParseStatus;
            zone9Info = zone9Property.zoneInfo;

            Assert.IsTrue(parseStatus_zone9 == ParseStatus.Parsed);
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone9, "Fecund"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone9, "Savage"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone9, "of Toughness"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone9, "of Impotence"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone9, "MapHasQuality"));

            Assert.AreEqual(mods_zone9.Where(mod => mod.Type == "Sextant").ToList().Count, 0);
            Assert.AreEqual(mods_zone9.Where(mod => mod.Type == "MapAffix").ToList().Count, 4);
            Assert.AreEqual(mods_zone9.Where(mod => mod.Type == "Other").ToList().Count, 1);

            Assert.AreEqual(zone9Property.zoneInfoParseStatus, ParseStatus.Parsed);
            Assert.AreEqual(zone9Info.monsterLevelNumeric, "79");
            #endregion

            //-------------------------------------  Testzone10  -------------------------------------
            #region TestZone10
            processScreenshotCapture.SetupImageAddToQueue(Directory.GetCurrentDirectory() + "//TestData//Screenshots//Zone9_Mods.jpg");
            Zone testZone10 = new Zone("TestZone10", setup.resourceManager);
            testZone10.ZoneID = "TestZone10";
            zonePropertyExtractor.SetZone(testZone10);

            List<MapMod> mods_zone10;
            ParseStatus parseStatus_zone10;
            ZoneInfo zone10Info;
            ZoneProperty zone10Property = zonePropertyExtractor.GetZoneProperties();
            mods_zone10 = zone10Property.mapMods;
            parseStatus_zone10 = zone10Property.mapModsParseStatus;
            zone10Info = zone10Property.zoneInfo;

            Assert.IsTrue(parseStatus_zone10 == ParseStatus.Parsed);
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone10, "Antagonist's"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone10, "Multifarious"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone10, "of Balance"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone10, "of Elemental Weakness"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone10, "Resistant"));
            Assert.IsTrue(setup.MapModsListContainsMod(mods_zone9, "MapHasQuality"));

            Assert.AreEqual(mods_zone10.Where(mod => mod.Type == "Sextant").ToList().Count, 0);
            Assert.AreEqual(mods_zone10.Where(mod => mod.Type == "MapAffix").ToList().Count, 5);
            Assert.AreEqual(mods_zone9.Where(mod => mod.Type == "Other").ToList().Count, 1);

            Assert.AreEqual(zone10Property.zoneInfoParseStatus, ParseStatus.Parsed);
            Assert.AreEqual(zone10Info.monsterLevelNumeric, "79");
            #endregion

            //-------------------------------------  Testzone11  -------------------------------------
            #region TestZone11
            processScreenshotCapture.SetupImageAddToQueue(Directory.GetCurrentDirectory() + "//TestData//Screenshots//Zone11_Mods.jpg");
            Zone testZone11 = new Zone("TestZone11", setup.resourceManager);
            testZone11.ZoneID = "TestZone11";
            zonePropertyExtractor.SetZone(testZone11);

            List<MapMod> mods_zone11;
            ParseStatus parseStatus_zone11;
            ZoneInfo zone11Info;
            ZoneProperty zone11Property = zonePropertyExtractor.GetZoneProperties();
            mods_zone11 = zone11Property.mapMods;
            parseStatus_zone11 = zone11Property.mapModsParseStatus;
            zone11Info = zone11Property.zoneInfo;

            Assert.IsTrue(parseStatus_zone11 == ParseStatus.Parsed);

            Assert.AreEqual(mods_zone11.Where(mod => mod.Type == "Sextant").ToList().Count, 0);
            Assert.AreEqual(mods_zone11.Where(mod => mod.Type == "MapAffix").ToList().Count, 0);

            Assert.AreEqual(zone11Property.zoneInfoParseStatus, ParseStatus.Parsed);
            Assert.AreEqual(zone11Info.monsterLevelNumeric, "11");
            Assert.AreEqual(zone11Info.delveDepthNumeric, "9");
            #endregion
        }

    }
}
