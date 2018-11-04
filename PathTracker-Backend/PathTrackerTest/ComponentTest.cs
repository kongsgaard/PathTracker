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

            processScreenshotCapture.SetupImageAddToQueue(Directory.GetCurrentDirectory() + "//TestData//Unavngivet.png");

            ZonePropertyExtractor zonePropertyExtractor = new ZonePropertyExtractor(processScreenshotCapture, setup.settings, setup.resourceManager);
            Zone testZone = new Zone("SomeZone", setup.resourceManager);
            testZone.ZoneID = "TestZone1";

            zonePropertyExtractor.SetZone(testZone);

            zonePropertyExtractor.GetMapMods();
        }
    }
}
