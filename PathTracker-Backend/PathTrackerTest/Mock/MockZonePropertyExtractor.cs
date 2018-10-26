using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PathTracker_Backend;

namespace PathTrackerTest {
    public class MockZonePropertyExtractor : IZonePropertyExtractor {


        private bool keepWatching = true;
        private Zone zone;

        public bool GetkeepWatching() {
            return keepWatching;
        }

        public Zone GetZone() {
            return zone;
        }

        public void setKeepWatching(bool val) {
            keepWatching = val;
        }

        public void SetZone(Zone _zone) {
            zone = _zone;
        }

        public void WatchForMinimapTab() {
            zone.mapMods = QueueMapMods.Dequeue();
        }

        private Queue<List<MapMod>> QueueMapMods = new Queue<List<MapMod>>();

        public void AddMapModsToQueue(List<MapMod> mods) {
            QueueMapMods.Enqueue(mods);
        }
    }
}
