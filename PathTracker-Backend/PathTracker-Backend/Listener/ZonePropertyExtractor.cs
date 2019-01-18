﻿using System;
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

                    (List<MapMod> parsedMods, ParseStatus status) = GetMapMods();

                    switch (status) {
                        case ParseStatus.NotPresent:
                            Console.WriteLine("ModsNotPresent!");
                                break;
                        case ParseStatus.PresentNotParsedCorrectly:
                            CheckForMapMods = true;
                            break;
                        case ParseStatus.Parsed:
                            zone.mapMods = parsedMods;
                            keepWatching = false;
                            Console.WriteLine("ModsFound! Stop watching");
                            break;
                            
                    }

                    if(status == ParseStatus.NotPresent) {
                        
                    }

                    keepWatchingWatch.Restart();
                }
                else {
                    System.Threading.Thread.Sleep(watchingDelay);
                }

            }
        }

        
        public (ZoneInfo, ParseStatus) GetZoneInfo(Bitmap bmp) {

            string currentDir = Directory.GetCurrentDirectory();
            long unixTimestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            string baseFileName = zone.ZoneID + "_" + unixTimestamp;

            Rectangle zoneInfoRect = new Rectangle(1650,40,262,145);
            
            System.Drawing.Imaging.PixelFormat format = bmp.PixelFormat;
            
            Bitmap mapMods = bmp.Clone(zoneInfoRect, format);
            
            Bitmap zoomed = new Bitmap(mapMods, mapMods.Width * 2, mapMods.Height * 2);

            Bitmap ZoomedGrayscale = MakeGrayscale3(zoomed);

            string imgGrayScaleFilePath = currentDir + "\\tmp\\" + baseFileName + "_zoneInfoGrayscale.jpeg";
            string ocrGrayscaleOutputPath = currentDir + "\\tmp\\" + baseFileName + "_zoneInfoGrayscale_hocr";
            
            ZoomedGrayscale.Save(imgGrayScaleFilePath, ImageFormat.Jpeg);
            
            generateOCR(imgGrayScaleFilePath, ocrGrayscaleOutputPath);

            return ParseZoneinfo(ocrGrayscaleOutputPath);
            
        }

        public (ZoneInfo, ParseStatus) ParseZoneinfo(string ocrFile) {

            var modLines = File.ReadAllLines(ocrFile + ".txt");

            ZoneInfo zoneInfo = new ZoneInfo();

            int LinesWithContent = 0;

            List<Tuple<string, ZoneInfoLines>> hintLines = new List<Tuple<string, ZoneInfoLines>>();
            hintLines.Add(new Tuple<string, ZoneInfoLines>("depth", ZoneInfoLines.DelveDepth));
            hintLines.Add(new Tuple<string, ZoneInfoLines>("monster level:", ZoneInfoLines.MonsterLevel));
            hintLines.Add(new Tuple<string, ZoneInfoLines>("league", ZoneInfoLines.League));

            foreach (string line in modLines) {

                var actualLineLower = line.ToLower();

                if (line.Length < 3) {
                    continue;
                }
                else {
                    LinesWithContent++;
                }

                List<Tuple<int, string, ZoneInfoLines>> distances = new List<Tuple<int, string, ZoneInfoLines>>();
                foreach (var hintLine in hintLines) {

                    var possibleLinesLower = hintLine.Item1.ToLower();

                    int levDist = Toolbox.LevenshteinDistance(actualLineLower, possibleLinesLower);
                    distances.Add(new Tuple<int, string, ZoneInfoLines>(levDist, hintLine.Item1, hintLine.Item2));
                }

                int minDist = int.MaxValue;
                ZoneInfoLines chosentype = ZoneInfoLines.None;
                foreach (var kvp in distances) {
                    if (kvp.Item1 < minDist && minDist < line.Length-2) {
                        minDist = kvp.Item1;
                        chosentype = kvp.Item3;
                    }
                }

                switch (chosentype) {
                    case ZoneInfoLines.League:
                        zoneInfo.league = line;
                        break;
                    case ZoneInfoLines.DelveDepth:
                        zoneInfo.delveDepth = line;
                        break;
                    case ZoneInfoLines.MonsterLevel:
                        zoneInfo.monsterLevel = line;
                        break;
                    default:
                        zoneInfo = null;
                        break;
                }

            }

            ParseStatus parseStatus = ParseStatus.NotPresent;

            if(zoneInfo != null) {
                parseStatus = ParseStatus.Parsed;
            }

            return (zoneInfo, parseStatus);
        }

        public (List<MapMod>, ParseStatus, ZoneInfo, ParseStatus) GetZoneProperties() {



            return (null, ParseStatus.NotPresent, null, ParseStatus.NotPresent);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// List<MapMod>: the parsed mods
        /// MapModParseStatus: Value indicating whether the mapmod parsing was sucessful
        /// </returns>
        public (List<MapMod>, ParseStatus) GetMapMods() {

            List<MapMod> returnMods = new List<MapMod>();
            ParseStatus modsCorrectlyParsed = ParseStatus.NotPresent;

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

            
            zone.ZoneInfo = GetZoneInfo(bmp).Item1;


            long unixTimestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            string baseFileName = zone.ZoneID + "_" + unixTimestamp;

            string originalFilePath = currentDir + "\\tmp\\" + baseFileName + "_original.jpeg";
            string imgFilePath = currentDir + "\\tmp\\" + baseFileName + "_onlyMapModsZoomedColored.jpeg";
            string imgGrayScaleFilePath = currentDir + "\\tmp\\" + baseFileName + "_onlyMapModsZoomedGrayScale.jpeg";
            string ocrOutputPath = currentDir + "\\tmp\\" + baseFileName + "_filteredZoomed_hocr";
            string ocrGrayscaleOutputPath = currentDir + "\\tmp\\" + baseFileName + "_filteredZoomedGRAYSCALE_hocr";

            int MinHue = 220;
            int MaxHue = 260;
            float MinLumi = 0.05f;
            float MaxLumi = 1f;
            float MinSat = 0.05f;
            float MaxSat = 1f;

            bmp.Save(originalFilePath, ImageFormat.Jpeg);
            //(Rectangle MapModRect, Bitmap bmpColored) = FindMapModRectangle(bmp, 20, 40, 205, 1920, 0.15, minHue: MinHue, maxHue: MaxHue, minLumi: MinLumi, maxLumi: MaxLumi, minSat: MinSat, maxSat: MaxSat, Color.FromArgb(0, 0, 0));

            Rectangle MapModRectangle = FindMapModRectangle(bmp, new int[]{18, 20}, 20, 205, 1920, 0.15, minHue: MinHue, maxHue: MaxHue, minLumi: MinLumi, maxLumi: MaxLumi, minSat: MinSat, maxSat: MaxSat, Color.FromArgb(0, 0, 0));
            

            Bitmap MapModsBmp = bmp.Clone(MapModRectangle, bmp.PixelFormat);

            //Bitmap MapModsBmpAllColored = ReplaceColor(MapModsBmp, minHue: MinHue, maxHue: MaxHue, minLumi: MinLumi, maxLumi: MaxLumi, minSat: MinSat, maxSat: MaxSat, Color.FromArgb(0, 0, 0));

            Bitmap MapModsBmpZoomed = new Bitmap(MapModsBmp, MapModsBmp.Width * 2, MapModsBmp.Height * 2);

            Bitmap MapModsBmpZoomedGrayscale = MakeGrayscale3(MapModsBmpZoomed);

            MapModsBmpZoomedGrayscale.Save(imgGrayScaleFilePath, ImageFormat.Jpeg);

            MapModsBmpZoomed.Save(imgFilePath, ImageFormat.Jpeg);

            generateOCR(imgGrayScaleFilePath, ocrGrayscaleOutputPath);

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
                                    
                                    //r = replacementOutsideThreshold.R;
                                    //g = replacementOutsideThreshold.G;
                                    //b = replacementOutsideThreshold.B;
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

        private static unsafe Rectangle FindMapModRectangle(Bitmap source, int[] lineHeightPixelsArr, int blockWidthPixels, int startHeight, int startWidth, double blockPercentWithinThreshold, float minHue, float maxHue,
                                                    float minLumi, float maxLumi, float minSat, float maxSat, Color replacementOutsideThreshold) {

            const int pixelSize = 4; // 32 bits per pixel

            

            Rectangle MapModBoundaries = new Rectangle();

            int MapModMaxHeight = 0;
            int MapModMaxWidth = 0;

            foreach (int lineHeightPixels in lineHeightPixelsArr) {

                int currentLine = 0;
                int currentBlock = 0;

                int topBlocksInLine = 1;
                int maxLine = 1;

                BitmapData sourceData = null;

                sourceData = source.LockBits(
                  new Rectangle(0, 0, source.Width, source.Height),
                  ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                try {
                    
                    int maxWidthPixels = source.Width;
                    int maxBlocks = (maxWidthPixels) / blockWidthPixels;

                    int maxHeightPixels = source.Height - startHeight;
                    int maxLines = (maxHeightPixels - startHeight) / lineHeightPixels;

                    int numPixelsInBlock = lineHeightPixels * blockWidthPixels;

                    int currentWidthPixel = 0;
                    int currentHeightPixel = 0;

                    int previousMaxBlocks = 0;

                    for (currentLine = 0; currentLine < maxLines; currentLine++) {

                        for (currentBlock = 0; currentBlock < maxBlocks; currentBlock++) {

                            double pixelsInCurrentBlock = 0;

                            for (int currentHeightPixelInLine = 0; currentHeightPixelInLine < lineHeightPixels; currentHeightPixelInLine++) {

                                currentHeightPixel = startHeight + ((currentLine * lineHeightPixels) + currentHeightPixelInLine);

                                byte* sourceRow = (byte*)sourceData.Scan0 + (currentHeightPixel * sourceData.Stride);
                                
                                for (int currentWidthPixelInBlock = 0; currentWidthPixelInBlock < blockWidthPixels; currentWidthPixelInBlock++) {

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
                                }

                            }

                            if (pixelsInCurrentBlock / numPixelsInBlock > blockPercentWithinThreshold) {

                            }
                            else {

                                previousMaxBlocks = currentBlock;

                                if (currentBlock > topBlocksInLine) {
                                    if (pixelsInCurrentBlock / numPixelsInBlock > 0.02) {
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
                    
                    int modRectHeight = maxLine * lineHeightPixels;

                    if(modRectWidth > MapModMaxWidth) {
                        MapModMaxWidth = modRectWidth;
                    }

                    if (modRectHeight > MapModMaxHeight) {
                        MapModMaxHeight = modRectHeight;
                    }
                    
                    if (sourceData != null)
                        source.UnlockBits(sourceData);
                }
            }

            MapModBoundaries = new Rectangle(startWidth - MapModMaxWidth, startHeight, MapModMaxWidth, MapModMaxHeight);
            
            return MapModBoundaries;
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

        public static Bitmap MakeGrayscale3(Bitmap original) {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
               {
                new float[] {.3f, .3f, .3f, 0, 0},
                new float[] {.59f, .59f, .59f, 0, 0},
                new float[] {.11f, .11f, .11f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
               });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
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


            ParseStatus status;
            if (LinesWithContent > 0) {
                status = ParseStatus.Parsed;
            }
            else {
                status = ParseStatus.NotPresent;
            }

            return (ActualChosenMods, status);
        }
    }

    public enum ParseStatus { Parsed, PresentNotParsedCorrectly, NotPresent }
    
    public enum ZoneInfoLines { MonsterLevel, DelveDepth, League, None }
}
