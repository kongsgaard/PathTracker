using System;
using PathTracker_Backend;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Collections.Generic;



namespace PathTrackerTest {
    [TestClass]
    public class FlowTests {
        [TestMethod]
        public void TestMethod1() {

            LogCreator.Setup();



            IDiskSaver mongoDiskSaver = new MongoDBSaver();
            IWebRequestManager webRequestManager = new WebRequestManager();
            IZonePropertyExtractor zonePropertyExtractor = new ZonePropertyExtractor(new Win32ProcessScreenshotCapture());


            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing is meh.") },
                { @"c:\demo\jQuery.js", new MockFileData("some js") },
                { @"c:\demo\image.gif", new MockFileData(new byte[] { 0x12, 0x34, 0x56, 0xd2 }) }
            });

            

            ComponentManager manager = new ComponentManager(mongoDiskSaver, webRequestManager, zonePropertyExtractor);

            Task t = new Task(() => manager.StartClientTxtListener(fileSystem));
            t.Start();
            System.Threading.Thread.Sleep(2000); //Wait for ClientTxtListenrer to start
            manager.StartInventoryListener();

            t.Wait();


        }
    }
}
