using System;
using System.Threading.Tasks;

namespace PathTracker_Backend {
    class Program {
        static void Main(string[] args) {

            InventoryListener inventoryListener = new InventoryListener(5000);

            Task tsk1 = new Task(inventoryListener.StartListening);

            tsk1.Start();


            Task.WaitAll(tsk1);

            Console.ReadLine();
        }
    }
}
