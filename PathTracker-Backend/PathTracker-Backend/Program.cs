﻿using System;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using log4net;
using log4net.Config;
using System.Linq;
using System.Diagnostics;

namespace PathTracker_Backend {
    class Program {
        

        static void Main(string[] args) {
            
            RequestCoordinator requestCoordinator = new RequestCoordinator();

            InventoryListener inventoryListener = new InventoryListener(5000, requestCoordinator);
            
            Task tsk1 = new Task(inventoryListener.StartListening);

            //tsk1.Start();


            //FileSystemWatcher watcher = new FileSystemWatcher();
            //string fileMinimap = "C:\\Users\\emilk\\Documents\\My Games\\Path of Exile\\Minimap";
            string filepath = "C:\\PoE\\logs\\Client.txt";


            int numLines = 0;
            


            Stopwatch watch = new Stopwatch();
            watch.Start();
            long numBytes = 0;


            while (true) {
                if (watch.ElapsedMilliseconds >= 1000) { 
                    using (FileStream stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                        
                        

                        long currentBytes = stream.Length;
                        stream.Position = numBytes;

                        int numNewBytes = (int)(currentBytes - numBytes);
                        byte[] newBytes = new byte[numNewBytes];

                        if(currentBytes - numBytes > 0) {
                            stream.Read(newBytes, 0, numNewBytes);
                            string newText = System.Text.Encoding.Default.GetString(newBytes);
                            Console.WriteLine(newText);
                        }



                        numBytes = stream.Length;

                    }
                    //Console.WriteLine(numBytes + " bytes");
                    watch.Restart();
                    numLines = 0;
                }
                else {
                    System.Threading.Thread.Sleep(1000 - (int)watch.ElapsedMilliseconds);
                }

            }




            //watcher.Path = Path.GetDirectoryName(fileMinimap);
            //watcher.Filter = Path.GetFileName(filepath);

            //watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            //
            //watcher.Changed += new FileSystemEventHandler(OnChanged);
            //watcher.Created += new FileSystemEventHandler(OnChanged);
            //watcher.Deleted += new FileSystemEventHandler(OnChanged);
            //watcher.Renamed += new RenamedEventHandler(OnRenamed);
            //
            //watcher.EnableRaisingEvents = true;


            //Console.WriteLine("Press \'q\' to quit the sample.");
            //while (Console.Read() != 'q') ;
            //
            //Task.WaitAll(tsk1);

            //SettingsManager Settings = SettingsManager.Instance;
            //
            //Console.WriteLine(Settings.GetValue("Account"));
            //Settings.SetValue("Account", "SomeTest");
            //
            //
            //Console.WriteLine(Settings.GetValue("Account"));

            Console.ReadLine();
        }

        private static void OnChanged(object source, FileSystemEventArgs e) {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
        }

        private static void OnRenamed(object source, RenamedEventArgs e) {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }
    }
}
