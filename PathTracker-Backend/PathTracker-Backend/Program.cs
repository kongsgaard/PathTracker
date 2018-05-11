using System;
using System.Threading.Tasks;

namespace PathTracker_Backend {
    class Program {
        

        static void Main(string[] args) {

            EventManager manager = new EventManager();

            Task t = new Task(manager.StartClientTxtListener);
            t.Start();
            System.Threading.Thread.Sleep(2000); //Wait for ClientTxtListenrer to start
            manager.StartInventoryListener();
            manager.StartStashtabListener("C");

            t.Wait();
            Console.ReadLine();
        }


    }
}
