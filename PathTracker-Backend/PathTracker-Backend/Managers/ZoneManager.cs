﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using log4net;
using System.IO;
using System.Text.RegularExpressions;
using log4net.Config;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace PathTracker_Backend
{
    public class ZoneManager
    {
        private ISettings Settings;
        public event EventHandler<ZoneChangeArgs> NewZoneEntered;
        List<Zone> ZoneList = new List<Zone>();

        Dictionary<string, Zone> ZoneDict = new Dictionary<string, Zone>();
        Dictionary<string, FileInfo> MinimapFiles = new Dictionary<string, FileInfo>();

        Thread extractMods = null;
        IZonePropertyExtractor modExtractor;

        IDiskSaver DiskSaver = null;
        
        ItemValuator itemValuator;

        ResourceManager Resource;

        public Zone currentZone = null;

        public ZoneManager(IDiskSaver saver, IZonePropertyExtractor zonePropertyExtractor, ISettings settings, ICurrencyRates currencyRates, ResourceManager resource) {
            Resource = resource;
            Settings = settings;
            itemValuator = new ItemValuator(currencyRates, Resource);
            PopulateMinimapFiles();
            DiskSaver = saver;
            modExtractor = zonePropertyExtractor;
            
        }

        private void PopulateMinimapFiles() {
            var minimapDir = new DirectoryInfo(Settings.GetValue("MinimapFolder"));
            
            foreach(var file in minimapDir.GetFiles()) {
                MinimapFiles.Add(file.Name, file);
            }
        }

        private FileInfo FindMostRecentWrittenFile() {

            string deleteOld = Settings.GetValue("DeleteOldMinimapFiles");
            DateTime deleteThreshold = DateTime.Now.AddHours(-24);
            FileInfo mostRecent = null;

            List<FileInfo> updatedFileInfos = new List<FileInfo>();
            List<FileInfo> newFileInfos = new List<FileInfo>();

            var minimapDir = new DirectoryInfo(Settings.GetValue("MinimapFolder"));

            foreach (var file in minimapDir.GetFiles()) {
                
                
                if(file.LastWriteTime < deleteThreshold && deleteOld == "true") {
                    File.Delete(file.FullName);
                    continue;
                }

                if (MinimapFiles.ContainsKey(file.Name)) {
                    if(MinimapFiles[file.Name].LastWriteTime != file.LastWriteTime) {
                        updatedFileInfos.Add(file);
                    }
                }
                else {
                    newFileInfos.Add(file);
                }
                
                if(mostRecent == null) {
                    mostRecent = file;
                }
                else if (mostRecent.LastWriteTime < file.LastWriteTime) {
                    mostRecent = file;
                }
            }
            
            foreach (var file in updatedFileInfos.Union(newFileInfos)) {
                MinimapFiles[file.Name] = file;
            }

            return mostRecent;
        }


        public void ZoneEntered(string zoneName) {

            

            Zone fromZone = null;
            ItemDeltaCalculator fromZoneDeltaCalculator = null;
            ExperienceDeltaCalculator fromZoneExperienceDeltaCalculator = null;
            
            Zone enteredZone = new Zone(zoneName, Resource);
            ItemDeltaCalculator enteredZoneDeltaCalculator = enteredZone.deltaCalculator;
            ExperienceDeltaCalculator enteredZoneExperienceDeltaCalculator = enteredZone.experienceCalculator;


            if (currentZone != null) {
                currentZone.timeLeft = DateTime.Now;
                currentZone.zoneTimer.Stop();
                fromZone = currentZone;
                fromZoneDeltaCalculator = fromZone.deltaCalculator;
                fromZoneExperienceDeltaCalculator = fromZone.experienceCalculator;
            }

            ZoneChangeArgs newZone = new ZoneChangeArgs(zoneName, fromZoneDeltaCalculator, enteredZoneDeltaCalculator, fromZoneExperienceDeltaCalculator, enteredZoneExperienceDeltaCalculator);


            //Call all delegates which subscribe to the NewZoneEntered event handler
            Delegate[] delegates = NewZoneEntered.GetInvocationList();
            WaitHandle[] waitHandles = new WaitHandle[delegates.Length];

            for (int i = 0; i < delegates.Length; i++) {
                int iparam = i;
                EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                Thread thread = new Thread(() => CallDelegate(delegates[iparam], waitHandle, waitHandles, iparam, newZone, this));
                thread.IsBackground = true;
                thread.Start();
            }

            //Wait for all threads to start
            while (Interlocked.Read(ref threadStarted) < delegates.Length) {
                Thread.Sleep(100);
            }

            //Wait for all threads to finnish
            WaitHandle.WaitAll(waitHandles);

            threadStarted = 0;
            
            
            //If the previous zone is null, calculate delta
            if (fromZone != null) {
                fromZone.CalculateAndAddToDelta();
                
                int maxLoops = 50;
                int loop = 0;
                while (true) {
                    FileInfo mostRecentChange = FindMostRecentWrittenFile();
                    if (mostRecentChange != null) {
                        fromZone.ZoneID = mostRecentChange.Name;
                        fromZone.LastExitedZone = mostRecentChange.LastWriteTime;

                        if (ZoneDict.ContainsKey(mostRecentChange.Name)) {
                            //Minimap already existed, so its a map which already exists.
                            ZoneDict[fromZone.ZoneID].MergeZoneIntoThis(fromZone);
                        }
                        else {
                            ZoneDict.Add(mostRecentChange.Name, fromZone);
                        }
                        break;
                    }
                    else {
                        if (loop > maxLoops) {
                            throw new Exception("Did not find modified minimap file within +5seconds");
                        }
                        Thread.Sleep(100);
                        loop++;
                    }
                }

                fromZone.CalculatZoneWorth(itemValuator);
            }





            if (extractMods != null) {
                if (extractMods.ThreadState == System.Threading.ThreadState.Running) {
                    modExtractor.setKeepWatching(false);

                    System.Threading.Thread.Sleep(2500);
                    extractMods.Abort();
                }
            }

            if(DiskSaver is null) {
                Console.WriteLine("Could not write zone to disk, because disksaver was null");
            }
            else if(fromZone != null) {
                foreach(var change in fromZone.ModifiedNonStackableItems) {
                    DiskSaver.UpdateZoneWithItemValue(change.Item2, itemValuator, change.Item1, fromZone);
                }
                DiskSaver.SaveToDisk(fromZone);
            }

            modExtractor.SetZone(enteredZone);
            extractMods = new Thread(modExtractor.WatchForMinimapTab);

            extractMods.Start();
            extractMods.Priority = ThreadPriority.BelowNormal;

            currentZone = enteredZone;

            itemValuator.currencyRates.Update();

        }
        
        private void writeZoneDisk(Zone zone) {

            string currentDir = Directory.GetCurrentDirectory();

            string saveToDir = currentDir + "\\ZoneJson\\";

            if (!Directory.Exists(saveToDir)) {
                Directory.CreateDirectory(saveToDir);
            }
            
            File.WriteAllText(saveToDir + zone.ZoneID + ".json", zone.ToJSON());

            Console.WriteLine("Write to zone to json at :" + currentDir + zone.ZoneID + ".json");
        }

        private long threadStarted = 0;

        private void CallDelegate(Delegate del, EventWaitHandle waitHandle, WaitHandle[] waitHandles, int i, ZoneChangeArgs newZoneArgs, object sender) {
            waitHandles[i] = waitHandle;
            Interlocked.Increment(ref threadStarted);
            del.DynamicInvoke(sender, newZoneArgs);
            waitHandle.Set();
        }
    }

    public class ZoneChangeArgs : EventArgs {
        public string ZoneName;
        public ItemDeltaCalculator FromZoneDeltaCalculator;
        public ItemDeltaCalculator EnteredZoneDeltaCalculator;

        public ExperienceDeltaCalculator FromZoneExperienceDeltaCalculator;
        public ExperienceDeltaCalculator EnteredZoneExperienceDeltaCalculator;

        public ZoneChangeArgs(string zoneName, ItemDeltaCalculator fromZoneDeltaCalculator, ItemDeltaCalculator enteredZoneDeltaCalculator,
                            ExperienceDeltaCalculator fromZoneExperienceDeltaCalculator, ExperienceDeltaCalculator enteredZoneExperienceDeltaCalculator) {
            ZoneName = zoneName;
            FromZoneDeltaCalculator = fromZoneDeltaCalculator;
            EnteredZoneDeltaCalculator = enteredZoneDeltaCalculator;

            FromZoneExperienceDeltaCalculator = fromZoneExperienceDeltaCalculator;
            EnteredZoneExperienceDeltaCalculator = enteredZoneExperienceDeltaCalculator;
        }
    }
}
