using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PathTracker_Backend;

namespace PathTrackerTest {
    class MockWebRequestManager : IWebRequestManager {

        private Dictionary<string,Queue<Inventory>> inventories = new Dictionary<string, Queue<Inventory>>();

        public void AddInventoryToQueue(Inventory inv, string character) {

            if (inventories.ContainsKey(character)) {
                inventories[character].Enqueue(inv);
            }
            else {
                Queue<Inventory> newQueue = new Queue<Inventory>();
                newQueue.Enqueue(inv);
                inventories.Add(character, newQueue);
            }
        }

        private Dictionary<string, Queue<StashApiRequest>> stashTabs = new Dictionary<string, Queue<StashApiRequest>>();

        public void AddStashtabToQueue(StashApiRequest stash, string league, string name) {

            string combo = name + "##" + league;
            if (stashTabs.ContainsKey(combo)) {
                stashTabs[combo].Enqueue(stash);
            }
            else {
                Queue<StashApiRequest> newQueue = new Queue<StashApiRequest>();
                newQueue.Enqueue(stash);
                stashTabs.Add(combo, newQueue);
            }
        }


        public Inventory GetInventory(string currentChar = "") {

            if (inventories.ContainsKey(currentChar)){
                return inventories[currentChar].Dequeue();
            }
            else {
                throw new Exception("Mock inventory did not contain inventory for character");
            }

        }

        public StashApiRequest GetStashtab(string name, string league = "", bool initializeTabs = false) {

            string combo = name + "##" + league;

            if (stashTabs.ContainsKey(combo)) {
                return stashTabs[combo].Dequeue();
            }
            else {
                throw new Exception("Mock inventory did not contain inventory for character");
            }

        }
    }
}
