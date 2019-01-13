using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PathTracker_Backend {
    public class ZonePropertyExtractor : IZonePropertyExtractor {

        IProcessWindowScreenshotCapture processWindowScreenshotCapture;
        ISettings Settings;
        ResourceManager Resource;

        public ZonePropertyExtractor(IProcessWindowScreenshotCapture capture, ISettings settings, ResourceManager resource) {
            Program.keyboardHook.OnKeyPressed += kbh_OnKeyPressed;
            Program.keyboardHook.OnKeyUnpressed += kbh_OnKeyUnPressed;
            processWindowScreenshotCapture = capture;
            Settings = settings;
            Resource = resource;
        }

        private bool MapmodsShown = true;
        private bool CheckForMapMods = true;
        private bool LMenuDown = false;
        private bool InventoryOpen = false;


        public void kbh_OnKeyPressed(object sender, Keys e) {
            if (e == Keys.LMenu) {
                LMenuDown = true;
            }
            else if (e == Keys.Tab) {
                if (LMenuDown == false) {
                    CheckForMapMods = true;
                    Console.WriteLine("Checking for mods!");
                }
            }
        }

        public void kbh_OnKeyUnPressed(object sender, Keys e) {
            if (e == Keys.LMenu) {
                LMenuDown = false;
            }
        }

        private Zone zone;

        public Zone GetZone() {
            return zone;
        }

        public void SetZone(Zone _zone) {
            zone = _zone;
        }

        private Stopwatch keepWatchingWatch = new Stopwatch();

        public void setKeepWatching(bool val) {
            keepWatching = val;
        }

        public bool GetkeepWatching() {
            return keepWatching;
        }
        private bool keepWatching = true;
        public int watchingDelay = 500;
        public void WatchForMinimapTab() {
            keepWatching = true;
            keepWatchingWatch.Start();

            //Initial 2sec wait when watcher starts.
            System.Threading.Thread.Sleep(2000);

            while (keepWatching) {

                if(keepWatchingWatch.ElapsedMilliseconds >= watchingDelay && CheckForMapMods) {
                    CheckForMapMods = false;

                    (List<MapMod> parsedMods, MapModParseStatus status) = GetMapMods();

                    switch (status) {
                        case MapModParseStatus.NotPresent:
                            Console.WriteLine("ModsNotPresent!");
                                break;
                        case MapModParseStatus.PresentNotParsedCorrectly:
                            CheckForMapMods = true;
                            break;
                        case MapModParseStatus.ModsParsed:
                            zone.mapMods = parsedMods;
                            keepWatching = false;
                            Console.WriteLine("ModsFound! Stop watching");
                            break;
                            
                    }

                    if(status == MapModParseStatus.NotPresent) {
                        
                    }

                    keepWatchingWatch.Restart();
                }
                else {
                    System.Threading.Thread.Sleep(watchingDelay);
                }

            }
        }

        
        public void GetZoneInfo(Bitmap bmp) {

            string currentDir = Directory.GetCurrentDirectory();
            long unixTimestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            string baseFileName = zone.ZoneID + "_" + unixTimestamp;

            Rectangle zoneInfoRect = new Rectangle(1500,40,410,145);
            
            System.Drawing.Imaging.PixelFormat format = bmp.PixelFormat;
            
            Bitmap mapMods = bmp.Clone(zoneInfoRect, format);
            
            Bitmap zoomed = new Bitmap(mapMods, mapMods.Width * 2, mapMods.Height * 2);

            string imgFilePath = currentDir + "\\tmp\\" + baseFileName + "_original_zoneInfo.jpeg";
            string ocrOutputPath = currentDir + "\\tmp\\" + baseFileName + "_original_zoneInfo_hocr.txt";


            zoomed.Save(imgFilePath, ImageFormat.Jpeg);

            generateOCR(imgFilePath, ocrOutputPath);

        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// List<MapMod>: the parsed mods
        /// MapModParseStatus: Value indicating whether the mapmod parsing was sucessful
        /// </returns>
        public (List<MapMod>, MapModParseStatus) GetMapMods() {

            List<MapMod> returnMods = new List<MapMod>();
            MapModParseStatus modsCorrectlyParsed = MapModParseStatus.NotPresent;

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Graphics graphics; Bitmap bmp;
            (graphics, bmp) = processWindowScreenshotCapture.GetProcessScreenshot("PathOfExile_x64");
            Console.WriteLine("CopyFromScreen ms:" + watch.ElapsedMilliseconds);
            

            string currentDir = Directory.GetCurrentDirectory();

            if (!Directory.Exists(currentDir + "\\tmp\\")) {
                Directory.CreateDirectory(currentDir + "\\tmp\\");
            }

            watch.Restart();
            int waitTicks = 10;
            for (int i = 0; i < waitTicks; i++) {
                if (!Directory.Exists(currentDir + "\\tmp\\")) {
                    if (i == waitTicks - 1) {
                        throw new Exception("Could not create dictionary within allocated time : " + currentDir + "\\tmp\\");
                    }
                    System.Threading.Thread.Sleep(200);
                }
                else {
                    break;
                }
            }
            Console.WriteLine("Waited for dir creation ms:" + watch.ElapsedMilliseconds);


            GetZoneInfo(bmp);


            long unixTimestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            string baseFileName = zone.ZoneID + "_" + unixTimestamp;

            string originalFilePath = currentDir + "\\tmp\\" + baseFileName + "_original.jpeg";
            string imgFilePath = currentDir + "\\tmp\\" + baseFileName + "_onlyMapModsZoomedColored.jpeg";
            string ocrOutputPath = currentDir + "\\tmp\\" + baseFileName + "_filteredZoomed_hocr";

            int MinHue = 220;
            int MaxHue = 260;
            float MinLumi = 0.05f;
            float MaxLumi = 1f;
            float MinSat = 0.05f;
            float MaxSat = 1f;

            bmp.Save(originalFilePath, ImageFormat.Jpeg);
            (Rectangle MapModRect, Bitmap bmpColored) = FindMapModRectangle(bmp, 20, 40, 205, 1920, 0.15, minHue: MinHue, maxHue: MaxHue, minLumi: MinLumi, maxLumi: MaxLumi, minSat: MinSat, maxSat: MaxSat, Color.FromArgb(0, 0, 0));

            Bitmap MapModsBmp = bmpColored.Clone(MapModRect, bmpColored.PixelFormat);

            Bitmap MapModsBmpAllColored = ReplaceColor(MapModsBmp, minHue: MinHue, maxHue: MaxHue, minLumi: MinLumi, maxLumi: MaxLumi, minSat: MinSat, maxSat: MaxSat, Color.FromArgb(0, 0, 0));

            Bitmap MapModsBmpZoomed = new Bitmap(MapModsBmpAllColored, MapModsBmp.Width * 2, MapModsBmp.Height * 2);

            MapModsBmpZoomed.Save(imgFilePath, ImageFormat.Jpeg);
            
            generateOCR(imgFilePath, ocrOutputPath);

            (returnMods, modsCorrectlyParsed) = ParseOCRFile(ocrOutputPath);

            return (returnMods, modsCorrectlyParsed);
        }

        private static unsafe (Rectangle, Bitmap) FindMapModRectangle(Bitmap source, int lineHeightPixels, int blockWidthPixels, int startHeight, int startWidth, double blockPercentWithinThreshold, float minHue, float maxHue, 
                                                    float minLumi, float maxLumi, float minSat, float maxSat, Color replacementOutsideThreshold) {

            const int pixelSize = 4; // 32 bits per pixel

            Bitmap target = new Bitmap(
              source.Width,
              source.Height,
              PixelFormat.Format32bppArgb);

            BitmapData sourceData = null, targetData = null;

            int currentLine = 0;
            int currentBlock = 0;

            int topBlocksInLine = 1;
            int maxLine = 1;

            Rectangle MapModBoundaries = new Rectangle();

            try {
                sourceData = source.LockBits(
                  new Rectangle(0, 0, source.Width, source.Height),
                  ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                targetData = target.LockBits(
                  new Rectangle(0, 0, target.Width, target.Height),
                  ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                int maxWidthPixels = source.Width;
                int maxBlocks = (maxWidthPixels) / blockWidthPixels;

                int maxHeightPixels = source.Height - startHeight;
                int maxLines = (maxHeightPixels - startHeight) / lineHeightPixels;

                int numPixelsInBlock = lineHeightPixels * blockWidthPixels;

                int currentWidthPixel = 0;
                int currentHeightPixel = 0;

                int previousMaxBlocks = 0;

                for(currentLine=0; currentLine < maxLines; currentLine++) {

                    for(currentBlock = 0; currentBlock < maxBlocks; currentBlock++) {

                        double pixelsInCurrentBlock = 0;

                        for (int currentHeightPixelInLine=0; currentHeightPixelInLine < lineHeightPixels; currentHeightPixelInLine++) {

                            currentHeightPixel = startHeight + ((currentLine * lineHeightPixels) + currentHeightPixelInLine);

                            byte* sourceRow = (byte*)sourceData.Scan0 + (currentHeightPixel * sourceData.Stride);
                            byte* targetRow = (byte*)targetData.Scan0 + (currentHeightPixel * targetData.Stride);


                            for (int currentWidthPixelInBlock=0; currentWidthPixelInBlock < blockWidthPixels; currentWidthPixelInBlock++) {

                                currentWidthPixel = source.Width - ((currentBlock * blockWidthPixels) + currentWidthPixelInBlock);
                                
                                byte b = sourceRow[currentWidthPixel * pixelSize + 0];
                                byte g = sourceRow[currentWidthPixel * pixelSize + 1];
                                byte r = sourceRow[currentWidthPixel * pixelSize + 2];
                                byte a = sourceRow[currentWidthPixel * pixelSize + 3];

                                Color c = Color.FromArgb(r, g, b);
                                var lumi = c.GetBrightness();
                                var hue = c.GetHue();
                                var sat = c.GetSaturation();
                                
                                if (hue > minHue && hue < maxHue && lumi > minLumi && lumi < maxLumi && sat < maxSat && sat > minSat) {
                                    pixelsInCurrentBlock++;
                                }
                                else {
                                    
                                    r = replacementOutsideThreshold.R;
                                    g = replacementOutsideThreshold.G;
                                    b = replacementOutsideThreshold.B;
                                }

                                targetRow[currentWidthPixel * pixelSize + 0] = b;
                                targetRow[currentWidthPixel * pixelSize + 1] = g;
                                targetRow[currentWidthPixel * pixelSize + 2] = r;
                                targetRow[currentWidthPixel * pixelSize + 3] = a;

                            }

                        }
                        
                        if(pixelsInCurrentBlock / numPixelsInBlock > blockPercentWithinThreshold) {
                            
                        }
                        else {

                            previousMaxBlocks = currentBlock;

                            if (currentBlock > topBlocksInLine) {
                                if(pixelsInCurrentBlock / numPixelsInBlock > 0.02) {
                                    currentBlock++;
                                }
                                topBlocksInLine = currentBlock;
                            }
                            
                            break;
                        }

                    }

                    if (currentLine > maxLine) {
                        maxLine = currentLine;
                    }

                    if (previousMaxBlocks == 0) {
                        break;
                    }

                }
            }
            finally {

                int modRectWidth = topBlocksInLine * blockWidthPixels;
                int modRectWidthStart = source.Width - topBlocksInLine * blockWidthPixels;


                int modRectHeight = maxLine * lineHeightPixels;
                int modRectHeightStart = startHeight;

                MapModBoundaries = new Rectangle(modRectWidthStart, modRectHeightStart, modRectWidth, modRectHeight);

                if (sourceData != null)
                    source.UnlockBits(sourceData);

                if (targetData != null)
                    target.UnlockBits(targetData);
            }
            
            return (MapModBoundaries, target);
        }

        private static unsafe Bitmap ReplaceColor(Bitmap source,
                                  float minHue, float maxHue, float minLumi, float maxLumi, float minSat, float maxSat,
                                  Color replacementOutsideThreshold, Color? replacementWithinThreshold = null) {
            const int pixelSize = 4; // 32 bits per pixel

            Bitmap target = new Bitmap(
              source.Width,
              source.Height,
              PixelFormat.Format32bppArgb);

            BitmapData sourceData = null, targetData = null;

            try {
                sourceData = source.LockBits(
                  new Rectangle(0, 0, source.Width, source.Height),
                  ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                targetData = target.LockBits(
                  new Rectangle(0, 0, target.Width, target.Height),
                  ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                for (int y = 0; y < source.Height; ++y) {
                    byte* sourceRow = (byte*)sourceData.Scan0 + (y * sourceData.Stride);
                    byte* targetRow = (byte*)targetData.Scan0 + (y * targetData.Stride);

                    for (int x = 0; x < source.Width; ++x) {
                        byte b = sourceRow[x * pixelSize + 0];
                        byte g = sourceRow[x * pixelSize + 1];
                        byte r = sourceRow[x * pixelSize + 2];
                        byte a = sourceRow[x * pixelSize + 3];

                        Color c = Color.FromArgb(r, g, b);
                        var lumi = c.GetBrightness();
                        var hue = c.GetHue();
                        var sat = c.GetSaturation();

                        
                        
                        if (hue > minHue && hue < maxHue && lumi > minLumi && lumi < maxLumi && sat < maxSat && sat > minSat) {
                            if(replacementWithinThreshold != null) {
                                Color casted = (Color)replacementWithinThreshold;
                                r = casted.R;
                                g = casted.G;
                                b = casted.B;
                            }
                        }
                        else {
                            r = replacementOutsideThreshold.R;
                            g = replacementOutsideThreshold.G;
                            b = replacementOutsideThreshold.B;
                        }

                        targetRow[x * pixelSize + 0] = b;
                        targetRow[x * pixelSize + 1] = g;
                        targetRow[x * pixelSize + 2] = r;
                        targetRow[x * pixelSize + 3] = a;
                    }
                }
            }
            finally {
                if (sourceData != null)
                    source.UnlockBits(sourceData);

                if (targetData != null)
                    target.UnlockBits(targetData);
            }

            return target;
        }

        private void generateOCR(string imgBaseFile, string ocrOutputFile) {

            int maxTimeoutMs = 60000;
            
            string tesseractDict = Settings.GetValue("TesseractDict");

            if(tesseractDict == null) {
                throw new Exception("Tried to OCR with tesseract when TesseractDict was not set in the settings");
            }

            Process p = new Process();
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = "/C " + tesseractDict + "\\tesseract " + imgBaseFile + " " + ocrOutputFile + " & exit";
            psi.CreateNoWindow = false;
            psi.UseShellExecute = false;

            p.StartInfo = psi;

            p.Start();
            p.PriorityClass = ProcessPriorityClass.BelowNormal;
            p.WaitForExit(maxTimeoutMs);
            
        }

        private (List<MapMod>, MapModParseStatus) ParseOCRFile(string ocrFile) {
            var modLines = File.ReadAllLines(ocrFile + ".txt");

            MapMods possibleMapMods = Resource.PossibleMapModsList;

            var possibleLines = Resource.PossibleMapModLines;
            var PossibleModsDict = new Dictionary<string, List<MapMod>>(Resource.LineToMapModsDict);

            Dictionary<string, MapMod> ChosenCandidateMods = new Dictionary<string, MapMod>();

            List<Tuple<string, string, int>> chosenLines = new List<Tuple<string, string, int>>();
            List<MapMod> ActualChosenMods = new List<MapMod>();

            int LinesWithContent = 0;

            foreach(var kvp in PossibleModsDict) {
                foreach(var mod in kvp.Value) {
                    foreach(var modLine in mod.ModLines) {
                        modLine.IsFound = false;
                    }
                }
            }

            foreach(var actualLine in modLines) {

                var actualLineLower = actualLine.ToLower();

                if(actualLine.Length < 5) {
                    continue;
                }
                else {
                    LinesWithContent++;
                }

                List<Tuple<int, string>> distances = new List<Tuple<int, string>>();
                foreach(var possibleLine in possibleLines) {

                    var possibleLinesLower = possibleLine.ToLower();

                    int levDist = Toolbox.LevenshteinDistance(actualLineLower, possibleLinesLower);
                    distances.Add(new Tuple<int, string>(levDist, possibleLine));
                }

                int minDist = int.MaxValue;
                string chosenLine = null;
                foreach(var kvp in distances) {
                    if(kvp.Item1 < minDist) {
                        minDist = kvp.Item1;
                        chosenLine = kvp.Item2;
                    }
                }

                var possibleMods = PossibleModsDict[chosenLine];

                foreach(var mod in possibleMods) {
                    var tempMod = MapMod.CopyObject(mod);
                    if (ChosenCandidateMods.ContainsKey(tempMod.Name)) {
                        MapMod currentMultiLineMod = ChosenCandidateMods[tempMod.Name];

                        foreach(var line in currentMultiLineMod.ModLines) {
                            if(line.LineText == chosenLine) {
                                line.IsFound = true;
                            }
                        }
                    }
                    else {
                        ChosenCandidateMods[mod.Name] = tempMod;

                        foreach (var line in ChosenCandidateMods[tempMod.Name].ModLines) {
                            if (line.LineText == chosenLine) {
                                line.IsFound = true;
                            }
                        }
                    }
                }

                
            }

            foreach(var kvp in ChosenCandidateMods) {
                bool allFound = true;
                foreach(var line in kvp.Value.ModLines) {
                    allFound = allFound && line.IsFound;
                }

                if (allFound) {
                    ActualChosenMods.Add(kvp.Value);
                }
            }


            MapModParseStatus status;
            if (LinesWithContent > 0) {
                status = MapModParseStatus.ModsParsed;
            }
            else {
                status = MapModParseStatus.NotPresent;
            }

            return (ActualChosenMods, status);
        }
    }

    public enum MapModParseStatus { ModsParsed, PresentNotParsedCorrectly, NotPresent }
}
