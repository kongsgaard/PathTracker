using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;


namespace PathTracker_Backend {
    public class InventoryListener {
        private int MsListenDelay;
        private Stopwatch ListenTimer = new Stopwatch();

        public InventoryListener(int msListenDelay) {
            MsListenDelay = msListenDelay;
        }

        public void StartListening() {

            ListenTimer.Start();

            while (true) {

                if(ListenTimer.ElapsedMilliseconds >= MsListenDelay) {
                    ListenTimer.Restart();
                }
                else {
                    System.Threading.Thread.Sleep(MsListenDelay - (int)ListenTimer.ElapsedMilliseconds);
                }
            }

        }

        
    }
}
