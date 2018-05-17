using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.DrawingCore;
using System.Text;
using log4net;
using log4net.Core;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using log4net.Config;
using log4net.Layout;


namespace PathTracker_Backend {
    class Program {
        

        static void Main(string[] args) {

            //ZoneManager man = new ZoneManager();
            //
            //System.Threading.Thread.Sleep(5000);
            //var procs = Process.GetProcessesByName("PathOfExile_x64");
            //
            //string s = User32.GetActiveWindowTitle();
            //
            ////var lst = Process.GetProcessesByName("Discord");
            //int k = 0;
            //
            //var rect = new User32.Rect();
            //int width = 0;
            //int height = 0;
            //foreach (Process proc in procs) {
            //    User32.GetWindowRect(proc.MainWindowHandle, ref rect);
            //    width = rect.right - rect.left;
            //    height = rect.bottom - rect.top;
            //    
            //    // break foreach if an realistic rectangle found => main process found
            //    if (width != 0 && height != 0) {
            //        break;
            //    }
            //}
            //
            //Stopwatch watch = new Stopwatch();
            //watch.Start();
            //
            //for(int i = 0; i < 20; i++) {
            //    
            //    var bmp = new Bitmap(width, height);
            //    Graphics graphics = Graphics.FromImage(bmp);
            //    graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            //
            //    Console.WriteLine("Captured: " + watch.ElapsedMilliseconds);
            //
            //    bmp.Save("c:\\tmp\\poe"+i.ToString()+".png");
            //
            //    Console.WriteLine(i + ":" + watch.ElapsedMilliseconds);
            //    System.Threading.Thread.Sleep(1000);
            //    watch.Restart();
            //}


            //EventManager manager = new EventManager();
            //
            //Task t = new Task(manager.StartClientTxtListener);
            //t.Start();
            //System.Threading.Thread.Sleep(2000); //Wait for ClientTxtListenrer to start
            //manager.StartInventoryListener();
            //manager.StartStashtabListener("C");
            //
            //t.Wait();

            Console.WriteLine("Done!");
            Console.ReadLine();
        }




    }
}
