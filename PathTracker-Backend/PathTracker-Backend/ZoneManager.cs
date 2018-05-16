using System;
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
        private SettingsManager Settings = SettingsManager.Instance;
        public event EventHandler<ZoneChangeArgs> NewZoneEntered;
        List<Zone> ZoneList = new List<Zone>();
        private static readonly ILog ZoneManagerLog = LogCreator.CreateLog("ZoneManager");

        Dictionary<string, Zone> ZoneDict = new Dictionary<string, Zone>();
        Dictionary<string, FileInfo> MinimapFiles = new Dictionary<string, FileInfo>();

        public Zone currentZone = null;

        public ZoneManager() {
            PopulateMinimapFiles();
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
                    break;
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

            Delegate[] delegates = NewZoneEntered.GetInvocationList();
            WaitHandle[] waitHandles = new WaitHandle[delegates.Length];


            Zone fromZone = null;
            Zone enteredZone = new Zone(zoneName);
            ItemDeltaCalculator fromZoneDeltaCalculator = null;
            ItemDeltaCalculator enteredZoneDeltaCalculator = enteredZone.deltaCalculator;

            if (currentZone != null) {
                fromZone = currentZone;
                fromZoneDeltaCalculator = fromZone.deltaCalculator;
            }

            ZoneChangeArgs newZone = new ZoneChangeArgs(zoneName, fromZoneDeltaCalculator, enteredZoneDeltaCalculator);
            
            for (int i = 0; i < delegates.Length; i++) {
                int iparam = i;
                EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                Thread thread = new Thread(() => CallDelegate(delegates[iparam], waitHandle, waitHandles, iparam, newZone, this));
                thread.Start();
            }
            
            //Wait for all threads to start
            while (Interlocked.Read(ref threadStarted) < delegates.Length) {
                Thread.Sleep(100);
            }
            
            //Wait for all threads to finnish
            WaitHandle.WaitAll(waitHandles);
            ZoneManagerLog.Info("All threads done after entering zone:" + zoneName);

            threadStarted = 0;

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
                            ZoneManagerLog.Info("Returned from existing map with ZoneName:" + fromZone.ZoneName + " ID:" + fromZone.ZoneID);
                            ZoneDict[fromZone.ZoneID].MergeZoneIntoThis(fromZone);
                        }
                        else {
                            ZoneManagerLog.Info("Return from new map with ZoneName:" + fromZone.ZoneName + " ID:" + fromZone.ZoneID);
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
            }
            currentZone = enteredZone;
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
        public ZoneChangeArgs(string zoneName, ItemDeltaCalculator fromZoneDeltaCalculator, ItemDeltaCalculator enteredZoneDeltaCalculator) {
            ZoneName = zoneName;
            FromZoneDeltaCalculator = fromZoneDeltaCalculator;
            EnteredZoneDeltaCalculator = enteredZoneDeltaCalculator;
        }
    }
}
