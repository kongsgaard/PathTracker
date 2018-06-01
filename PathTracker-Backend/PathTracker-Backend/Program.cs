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

namespace PathTracker_Backend {
    class Program {
        
        static void Main(string[] args) {


            //
            //LogCreator.Setup();
            //
            //ComponentManager manager = new ComponentManager();
            //
            //Task t = new Task(manager.StartClientTxtListener);
            //t.Start();
            //System.Threading.Thread.Sleep(2000); //Wait for ClientTxtListenrer to start
            //manager.StartInventoryListener();
            //manager.StartStashtabListener("C");
            //
            //t.Wait();
            //211 to 252
            //float f = ZonePropertyExtractor.CalculateHue(108, 81, 218);
            
            Thread thread = new Thread(() => LowLevelKeyboardHook.KeyboardHook());
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();

            //Program.keyboardHook.OnKeyPressed += kbh_OnKeyPressed;
            //Program.keyboardHook.OnKeyUnpressed += kbh_OnKeyUnPressed;



            LogCreator.Setup();

            ZonePropertyExtractor zonePropertyExtractor = new ZonePropertyExtractor();

            zonePropertyExtractor.WatchForMinimapTab();

            //Console.WriteLine("Done!");
            Console.ReadLine();
        }

        public static LowLevelKeyboardHook keyboardHook = new LowLevelKeyboardHook();
        
    }
}
