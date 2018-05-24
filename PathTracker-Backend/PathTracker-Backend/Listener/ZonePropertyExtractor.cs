using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.IO;

namespace PathTracker_Backend {
    public class ZonePropertyExtractor {
        public Zone zone { get; set; }

        public ZonePropertyExtractor() {
            Program.keyboardHook.OnKeyPressed += kbh_OnKeyPressed;
            Program.keyboardHook.OnKeyUnpressed += kbh_OnKeyUnPressed;

            zone = new Zone("Testzone");
            zone.ZoneID = "SOMETESTZONE";
            
        }

        private bool MapmodsShown = false;
        private bool MapmodChangeQueued = false;
        private bool LMenuDown = false;
        private bool InventoryOpen = false;

        public void kbh_OnKeyPressed(object sender, Keys e) {

            if (e == Keys.LMenu) {
                LMenuDown = true;
            }
            else if (e == Keys.Tab) {
                if (LMenuDown == false) {
                    MapmodChangeQueued = true;
                }
            }
        }

        public void kbh_OnKeyUnPressed(object sender, Keys e) {

            if (e == Keys.LMenu) {
                LMenuDown = false;
            }
            else if (e == Keys.Tab) {
                if (MapmodChangeQueued == true) {
                    MapmodsShown = !MapmodsShown;
                    Console.WriteLine("MapmodShown changed to:" + MapmodsShown);
                    GetMapMods(MapmodsShown);
                }
                MapmodChangeQueued = false;
            }
        }

        public void GetMapMods(bool mapmodsShown) {

            //Give the minimap a little time to set
            System.Threading.Thread.Sleep(1000);
            
            var procs = Process.GetProcessesByName("PathOfExile_x64");

            string s = User32Wrapper.GetActiveWindowTitle();

            int k = 0;

            var rect = new User32Wrapper.Rect();
            int width = 0;
            int height = 0;
            foreach (Process proc in procs) {
                User32Wrapper.GetWindowRect(proc.MainWindowHandle, ref rect);
                width = rect.right - rect.left;
                height = rect.bottom - rect.top;

                // break foreach if an realistic rectangle found => main process found
                if (width != 0 && height != 0) {
                    break;
                }
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();

            var bmp = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(bmp);
            graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
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

            long unixTimestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            string baseFileName = zone.ZoneID + "_" + unixTimestamp;

            Rectangle mapModRect = new Rectangle(1280, 0, 640, 880);
            System.Drawing.Imaging.PixelFormat format = bmp.PixelFormat;

            watch.Restart();
            Bitmap mapMods = bmp.Clone(mapModRect, format);
            Console.WriteLine("Clone ms:" + watch.ElapsedMilliseconds);

            watch.Restart();
            Bitmap zoomed = new Bitmap(mapMods, mapMods.Width * 2, mapMods.Height * 2);
            Console.WriteLine("zoomed ms:" + watch.ElapsedMilliseconds);

            //bmp.Save(currentDir + "\\tmp\\" + unixTimestamp + ".png");
            //mapMods.Save(currentDir + "\\tmp\\" + unixTimestamp + "MAPMOD.png");
            //zoomed.Save(currentDir + "\\tmp\\" + unixTimestamp + "MAPMODzoomed.png");

            watch.Restart();
            Bitmap colored = ReplaceColor(zoomed, minHue: 240, maxHue: 240, minLumi: 0.25f, maxLumi: 1f, minSat: 0.25f, maxSat: 1f, Color.FromArgb(0, 0, 0), Color.FromArgb(255, 255, 255));
            Console.WriteLine("white colored ms:" + watch.ElapsedMilliseconds);

            //colored.Save(currentDir + "\\tmp\\" + unixTimestamp + "MAPMODColored.png");

            

            //Console.WriteLine("Took ms :" + watch.ElapsedMilliseconds);

            watch.Restart();
            bmp.Save(currentDir + "\\tmp\\" + baseFileName + "_original.jpeg", ImageFormat.Jpeg);
            Console.WriteLine("zoomed written ms:" + watch.ElapsedMilliseconds);


            watch.Restart();
            colored.Save(currentDir + "\\tmp\\" + baseFileName + "_filteredZoomed.jpeg", ImageFormat.Jpeg);
            Console.WriteLine("white written ms:" + watch.ElapsedMilliseconds);

            //bmp.Save(currentDir + "\\tmp\\" + unixTimestamp + "png.png", ImageFormat.Png);

            string hocrFile = generateOCR(currentDir, baseFileName);

            ParseOCRFile(hocrFile);
        }

        private static unsafe Bitmap ReplaceColor(Bitmap source,
                                  float minHue, float maxHue, float minLumi, float maxLumi, float minSat, float maxSat,
                                  Color replacementWithinThreshold, Color replacementOutsideThreshold) {
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

                        
                        
                        if (hue >= minHue && hue <= maxHue && lumi >= minLumi && lumi <= maxLumi && sat <= maxSat && sat >= minSat) {
                            //r = replacementWithinThreshold.R;
                            //g = replacementWithinThreshold.G;
                            //b = replacementWithinThreshold.B;
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

        private string generateOCR(string currentDir, string baseFileName) {

            int maxTimeoutMs = 60000;

            string generatedOCRFile = currentDir + "\\tmp\\" + baseFileName + "_hocrOutput";

            string tesseractDict = SettingsManager.Instance.GetValue("TesseractDict");

            if(tesseractDict == null) {
                throw new Exception("Tried to OCR with tesseract when TesseractDict was not set in the settings");
            }

            Process p = new Process();
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = "/K " + tesseractDict + "\\tesseract " + currentDir +"\\tmp\\"+baseFileName + "_filteredZoomed.jpeg " + generatedOCRFile + " & exit";
            //psi.CreateNoWindow = true;

            p.StartInfo = psi;

            p.Start();
            p.WaitForExit(maxTimeoutMs);

            Console.WriteLine("Writeing hocr to " + generatedOCRFile);

            return generatedOCRFile + ".txt";
        }

        private void ParseOCRFile(string ocrFile) {
            var modLines = File.ReadAllLines(ocrFile);

            int k = 0;
        }
    }
}
