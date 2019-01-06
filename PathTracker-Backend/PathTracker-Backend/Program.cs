using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;

namespace PathTracker_Backend {
    class Program {
        

        static void Main(string[] args) {

            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            
            Thread thread = new Thread(() => LowLevelKeyboardHook.KeyboardHook());
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();
            
            ISettings settings = new FileSettings("settings.json");
            IDiskSaver mongoDiskSaver = new MongoDBSaver(settings, true);
            //IDiskSaver folderDiskSaver = new DiskFolderSaver(settings);
            ResourceManager resourceManager = new ResourceManager();


            IWebRequestManager webRequestManager = new WebRequestManager(settings);
            IZonePropertyExtractor zonePropertyExtractor = new ZonePropertyExtractor(new Win32ProcessScreenshotCapture(), settings, resourceManager);

            ICurrencyRates currencyRates = new PoeNinjaCurrencyRates(settings);

            ComponentManager manager = new ComponentManager(mongoDiskSaver, webRequestManager, zonePropertyExtractor, settings, currencyRates, resourceManager);
            
            Task t = new Task(() => manager.StartClientTxtListener());
            t.Start();
            System.Threading.Thread.Sleep(2000); //Wait for ClientTxtListenrer to start
            manager.StartInventoryListener();
            
            t.Wait();
            Console.ReadLine();
        }
        
        public static LowLevelKeyboardHook keyboardHook = new LowLevelKeyboardHook();
        
        /// <summary>
        /// Method that gets called when console window is closed
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        static bool ConsoleEventCallback(int eventType) {
            if (eventType == 2) {
                ISettings settings = new FileSettings("settings.json");

                MongoDBSaver saver = new MongoDBSaver(settings, false);

                saver.Dispose();
            }
            return false;
        }
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
                                               // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

    }
}
