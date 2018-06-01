using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PathTracker_Backend
{
    public class MapModLine
    {
        [JsonProperty("Line")]
        public string LineText;

        [JsonProperty("MaxRoll")]
        public int? MaxRoll;

        [JsonProperty("MinRoll")]
        public int? MinRoll;

        public bool IsFound = false;
    }
}
