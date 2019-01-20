using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;

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
        
        private Mutex originalScreenshotBmpMutex = new Mutex();

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

                    ZoneProperty zoneProperty = GetZoneProperties();

                    //(List<MapMod> parsedMods, ParseStatus status) = GetMapMods();

                    switch (zoneProperty.mapModsParseStatus) {
                        case ParseStatus.NotPresent:
                            Console.WriteLine("ModsNotPresent!");
                                break;
                        case ParseStatus.PresentNotParsedCorrectly:
                            CheckForMapMods = true;
                            break;
                        case ParseStatus.Parsed:
                            zone.mapMods = zoneProperty.mapMods;
                            keepWatching = false;
                            Console.WriteLine("ModsFound! Stop watching");
                            break;
                            
                    }

                    if(zoneProperty.mapModsParseStatus == ParseStatus.NotPresent) {
                        
                    }

                    keepWatchingWatch.Restart();
                }
                else {
                    System.Threading.Thread.Sleep(watchingDelay);
                }

            }
        }


        public Bitmap GetOriginalScreenshot() {

            Graphics graphics; Bitmap bmp;
            (graphics, bmp) = processWindowScreenshotCapture.GetProcessScreenshot("PathOfExile_x64");

            string currentDir = Directory.GetCurrentDirectory();

            if (!Directory.Exists(currentDir + "\\tmp\\")) {
                Directory.CreateDirectory(currentDir + "\\tmp\\");
            }

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

            return bmp;
        }
        
        public (ZoneInfo, ParseStatus, string) GetZoneInfo(Bitmap bmp, string baseFileName, string currentDir) {

            string imgGrayScaleFilePath = currentDir + "\\tmp\\" + baseFileName + "_zoneInfoGrayscale.jpeg";
            string ocrGrayscaleOutputPath = currentDir + "\\tmp\\" + baseFileName + "_zoneInfoGrayscale_hocr";

            Rectangle zoneInfoRect = new Rectangle(1650,40,262,145);

            originalScreenshotBmpMutex.WaitOne();
            Bitmap bmpDeepCopy = new Bitmap(bmp);
            originalScreenshotBmpMutex.ReleaseMutex();

            System.Drawing.Imaging.PixelFormat format = bmpDeepCopy.PixelFormat;
            
            Bitmap mapMods = bmpDeepCopy.Clone(zoneInfoRect, format);
            
            Bitmap zoomed = new Bitmap(mapMods, mapMods.Width * 2, mapMods.Height * 2);

            Bitmap ZoomedGrayscale = MakeGrayscale3(zoomed);
            
            ZoomedGrayscale.Save(imgGrayScaleFilePath, ImageFormat.Jpeg);
            
            generateOCR(imgGrayScaleFilePath, ocrGrayscaleOutputPath);

            (ZoneInfo rtZoneinfo, ParseStatus zoneInfoParseStatus) = ParseZoneinfo(ocrGrayscaleOutputPath);

            return (rtZoneinfo, zoneInfoParseStatus, imgGrayScaleFilePath);
            
        }

        public (ZoneInfo, ParseStatus) ParseZoneinfo(string ocrFile) {

            var modLines = File.ReadAllLines(ocrFile + ".txt");

            ZoneInfo zoneInfo = new ZoneInfo();

            int LinesWithContent = 0;

            List<Tuple<string, ZoneInfoLines>> hintLines = new List<Tuple<string, ZoneInfoLines>>();
            hintLines.Add(new Tuple<string, ZoneInfoLines>("delve depth: ", ZoneInfoLines.DelveDepth));
            hintLines.Add(new Tuple<string, ZoneInfoLines>("monster level: ", ZoneInfoLines.MonsterLevel));
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
                    if (kvp.Item1 < minDist && (
                        (kvp.Item1 < line.Length - 11 && kvp.Item3 == ZoneInfoLines.MonsterLevel) ||
                        (kvp.Item1 < line.Length - 9 && kvp.Item3 == ZoneInfoLines.DelveDepth) ||
                        (kvp.Item1 < line.Length - 4 && kvp.Item3 == ZoneInfoLines.League) )
                        ) {
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
                        Match depthMatch = Regex.Match(line, @": (\d+)");

                        if (depthMatch.Success && depthMatch.Groups.Count > 1) {
                            if (depthMatch.Groups[1].Captures.Count > 0) {
                                string depthString = depthMatch.Groups[1].Captures[0].ToString();
                                zoneInfo.delveDepthNumeric = depthString;
                            }
                        }

                        break;
                    case ZoneInfoLines.MonsterLevel:
                        zoneInfo.monsterLevel = line;
                        Match leveMatch = Regex.Match(line, @": (\d+)");

                        if (leveMatch.Success && leveMatch.Groups.Count > 1) {
                            if (leveMatch.Groups[1].Captures.Count > 0) {
                                string levelString = leveMatch.Groups[1].Captures[0].ToString();
                                zoneInfo.monsterLevelNumeric = levelString;
                            }
                        }
                        
                        break;
                    default:
                        //zoneInfo = null;
                        break;
                }

            }

            ParseStatus parseStatus = ParseStatus.NotPresent;

            if(zoneInfo.monsterLevel != null) {

                if(zoneInfo.monsterLevelNumeric != null) {
                    parseStatus = ParseStatus.Parsed;

                    if(zoneInfo.delveDepth != null) {
                        if(zoneInfo.delveDepthNumeric != null) {
                            parseStatus = ParseStatus.Parsed;
                        }
                        else {
                            parseStatus = ParseStatus.PresentNotParsedCorrectly;
                        }
                    }

                }
                else {
                    parseStatus = ParseStatus.PresentNotParsedCorrectly;
                }


            }
            
            return (zoneInfo, parseStatus);
        }

        public (List<MapMod>, ParseStatus, string, List<string>) GetMapMods(Bitmap bmp, string baseFileName, string currentDir) {
            
            string imgGrayScaleFilePath = currentDir + "\\tmp\\" + baseFileName + "_mapModsGrayscale.jpeg";
            string ocrGrayscaleOutputPath = currentDir + "\\tmp\\" + baseFileName + "_mapModsGrayscale_hocr";

            int MinHue = 220;
            int MaxHue = 260;
            float MinLumi = 0.05f;
            float MaxLumi = 1f;
            float MinSat = 0.05f;
            float MaxSat = 1f;

            originalScreenshotBmpMutex.WaitOne();
            Bitmap bmpDeepCopy = new Bitmap(bmp);
            originalScreenshotBmpMutex.ReleaseMutex();

            Rectangle MapModRectangle = FindMapModRectangle(bmpDeepCopy, new int[] { 18, 20 }, 20, 205, 1920, 0.15, minHue: MinHue, maxHue: MaxHue, minLumi: MinLumi, maxLumi: MaxLumi, minSat: MinSat, maxSat: MaxSat, Color.FromArgb(0, 0, 0));
            
            Bitmap MapModsBmp = bmpDeepCopy.Clone(MapModRectangle, bmpDeepCopy.PixelFormat);
            
            Bitmap MapModsBmpZoomed = new Bitmap(MapModsBmp, MapModsBmp.Width * 2, MapModsBmp.Height * 2);

            Bitmap MapModsBmpZoomedGrayscale = MakeGrayscale3(MapModsBmpZoomed);

            MapModsBmpZoomedGrayscale.Save(imgGrayScaleFilePath, ImageFormat.Jpeg);
            
            generateOCR(imgGrayScaleFilePath, ocrGrayscaleOutputPath);
            
            (List<MapMod> returnMods, ParseStatus modsCorrectlyParsed, List<string> NonParsedMapModLines) = ParseMapMods(ocrGrayscaleOutputPath);

            return (returnMods, modsCorrectlyParsed, imgGrayScaleFilePath, NonParsedMapModLines);

        }
        
        private (List<MapMod>, ParseStatus, List<string>) ParseMapMods(string ocrFile) {
            var modLines = File.ReadAllLines(ocrFile + ".txt");

            MapMods possibleMapMods = Resource.PossibleMapModsList;

            var possibleLines = Resource.PossibleMapModLines;
            var PossibleModsDict = new Dictionary<string, List<MapMod>>(Resource.LineToMapModsDict);

            Dictionary<string, MapMod> ChosenCandidateMods = new Dictionary<string, MapMod>();

            List<Tuple<string, string, int>> chosenLines = new List<Tuple<string, string, int>>();
            List<MapMod> ActualChosenMods = new List<MapMod>();
            List<string> NonParsedMapModLines = new List<string>();

            int LinesWithContent = 0;

            foreach (var kvp in PossibleModsDict) {
                foreach (var mod in kvp.Value) {
                    foreach (var modLine in mod.ModLines) {
                        modLine.IsFound = false;
                    }
                }
            }

            foreach (var actualLine in modLines) {

                var actualLineLower = actualLine.ToLower();

                if (actualLine.Length < 5) {
                    continue;
                }
                else {
                    LinesWithContent++;
                }

                List<Tuple<int, string>> distances = new List<Tuple<int, string>>();
                foreach (var possibleLine in possibleLines) {

                    var possibleLinesLower = possibleLine.ToLower();

                    int levDist = Toolbox.LevenshteinDistance(actualLineLower, possibleLinesLower);
                    distances.Add(new Tuple<int, string>(levDist, possibleLine));
                }

                int minDist = int.MaxValue;
                string chosenLine = null;
                foreach (var kvp in distances) {
                    if (kvp.Item1 < minDist && (
                        (kvp.Item2.Length < 15 && kvp.Item1 <= 4) ||
                        (15 <= kvp.Item2.Length && kvp.Item2.Length < 30 && kvp.Item1 <= 7) ||
                        (30 <= kvp.Item2.Length && kvp.Item1 <= 10)
                        )) {
                        minDist = kvp.Item1;
                        chosenLine = kvp.Item2;
                    }
                }

                if(chosenLine == null) {
                    NonParsedMapModLines.Add(actualLine);
                    continue;
                }

                var possibleMods = PossibleModsDict[chosenLine];

                foreach (var mod in possibleMods) {
                    var tempMod = MapMod.CopyObject(mod);
                    if (ChosenCandidateMods.ContainsKey(tempMod.Name)) {
                        MapMod currentMultiLineMod = ChosenCandidateMods[tempMod.Name];

                        foreach (var line in currentMultiLineMod.ModLines) {
                            if (line.LineText == chosenLine) {
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

            foreach (var kvp in ChosenCandidateMods) {
                bool allFound = true;
                foreach (var line in kvp.Value.ModLines) {
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

            return (ActualChosenMods, status, NonParsedMapModLines);
        }

        public ZoneProperty GetZoneProperties() {

            long unixTimestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            string baseFileName = zone.ZoneID + "_" + unixTimestamp;
            string currentDir = Directory.GetCurrentDirectory();

            ZoneProperty zoneProperty = new ZoneProperty();

            Bitmap bmp = GetOriginalScreenshot();

            string originalFilePath = currentDir + "\\tmp\\" + baseFileName + "_original.jpeg";
            bmp.Save(originalFilePath, ImageFormat.Jpeg);

            zoneProperty.originalScreenshotPath = originalFilePath;

            Task t = new Task(() => GetZoneInfo(bmp, baseFileName, currentDir));

            Task <(ZoneInfo, ParseStatus, string)> zoneInfoTask = Task<(ZoneInfo, ParseStatus, string)>.Factory.StartNew(() => GetZoneInfo(bmp, baseFileName, currentDir));
            
            Task<(List<MapMod>, ParseStatus, string, List<string>)> mapModsTask = Task<(List<MapMod>, ParseStatus, string, List<string>)>.Factory.StartNew(() => GetMapMods(bmp, baseFileName, currentDir));
            
            Task.WhenAll(zoneInfoTask, mapModsTask);

            zoneProperty.zoneInfo = zoneInfoTask.Result.Item1;
            zoneProperty.zoneInfoParseStatus = zoneInfoTask.Result.Item2;
            zoneProperty.zoneInfoScreenshotPath = zoneInfoTask.Result.Item3;

            zoneProperty.mapMods = mapModsTask.Result.Item1;
            zoneProperty.mapModsParseStatus = mapModsTask.Result.Item2;
            zoneProperty.mapModsScreenshotPath = mapModsTask.Result.Item3;
            zoneProperty.NonParsedMapMods = mapModsTask.Result.Item4;

            return zoneProperty;
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

            int maxTimeoutMs = 20000;
            
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

    }

    public enum ParseStatus { Parsed, PresentNotParsedCorrectly, NotPresent }
    
    public enum ZoneInfoLines { MonsterLevel, DelveDepth, League, None }
}
