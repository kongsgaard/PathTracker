﻿using System;
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

namespace PathTracker_Backend {
    class Program {
        
        static void Main(string[] args) {
            
            Thread thread = new Thread(() => LowLevelKeyboardHook.KeyboardHook());
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();

            LogCreator.Setup();
            ISettings settings = new FileSettings("settings.json");

            IDiskSaver mongoDiskSaver = new MongoDBSaver(settings);
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
        
    }
}
