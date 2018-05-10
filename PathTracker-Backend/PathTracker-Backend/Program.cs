using System;
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
            
            //RequestCoordinator requestCoordinator = new RequestCoordinator();

            //StashtabListener inventoryListener = new StashtabListener("Ch",5000, requestCoordinator);
            //
            //Task tsk1 = new Task(inventoryListener.StartListening);
            //
            //tsk1.Start();
            //
            //tsk1.Wait();

            ClientTxtListener s = new ClientTxtListener(msListenDelay: default);


            ClientTxtListener clientTxtListener = new ClientTxtListener(1);

            clientTxtListener.StartListening();

            //FileSystemWatcher watcher = new FileSystemWatcher();
            //string fileMinimap = "C:\\Users\\emilk\\Documents\\My Games\\Path of Exile\\Minimap";
            //string filepath = "C:\\PoE\\logs\\Client.txt";


            


            



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


    }
}
