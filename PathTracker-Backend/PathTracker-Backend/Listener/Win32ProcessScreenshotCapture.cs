using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PathTracker_Backend
{
    public class Win32ProcessScreenshotCapture : IProcessWindowScreenshotCapture {

        public (Graphics, Bitmap) GetProcessScreenshot(string name) {

            //Give the minimap a little time to set
            System.Threading.Thread.Sleep(1000);

            var procs = Process.GetProcessesByName("PathOfExile_x64");

            string activeTitle = User32Wrapper.GetActiveWindowTitle();

            if (activeTitle != "Path of Exile") {
                throw new Exception("Process was not named Path of Exile, found name:" + activeTitle);
            }

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
            return (graphics, bmp);

        }
    }
}
