using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Text;
using log4net;
using log4net.Core;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using log4net.Config;
using log4net.Layout;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;
using Newtonsoft.Json;
using System.IO.Abstractions;
using System.IO;

namespace PathTracker_Backend {
    class Program {
        
        static void Main(string[] args) {
            
            Thread thread = new Thread(() => LowLevelKeyboardHook.KeyboardHook());
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();

            LogCreator.Setup();

            IDiskSaver mongoDiskSaver = new MongoDBSaver();
            IDiskSaver folderDiskSaver = new DiskFolderSaver();
            IFileSystem fileSystem = new FileSystem();

            ComponentManager manager = new ComponentManager(mongoDiskSaver);
            
            Task t = new Task(() => manager.StartClientTxtListener(fileSystem));
            t.Start();
            System.Threading.Thread.Sleep(2000); //Wait for ClientTxtListenrer to start
            manager.StartInventoryListener();
            //manager.StartStashtabListener("C");
            
            t.Wait();
            //211 to 252
            //float f = ZonePropertyExtractor.CalculateHue(108, 81, 218);
            


            //Program.keyboardHook.OnKeyPressed += kbh_OnKeyPressed;
            //Program.keyboardHook.OnKeyUnpressed += kbh_OnKeyUnPressed;



            //LogCreator.Setup();
            //
            //Zone z = new Zone("z1");
            //z.ZoneID = "TestZone";
            //
            //ZonePropertyExtractor zonePropertyExtractor = new ZonePropertyExtractor();
            //zonePropertyExtractor.zone = z;
            //
            //
            //zonePropertyExtractor.WatchForMinimapTab();

            //Console.WriteLine("Done!");
            Console.ReadLine();
        }

        public static LowLevelKeyboardHook keyboardHook = new LowLevelKeyboardHook();
        
    }
}
