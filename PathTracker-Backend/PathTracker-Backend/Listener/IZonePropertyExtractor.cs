using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracker_Backend
{
    public interface IZonePropertyExtractor
    {
        Zone GetZone();

        void SetZone(Zone _zone);

        bool GetkeepWatching();

        void setKeepWatching(bool val);

        void WatchForMinimapTab();
    }
}
