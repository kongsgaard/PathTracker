using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PathTracker_Backend
{
    [Serializable]
    public class MapMod
    {
        [JsonProperty("Lines")]
        public List<MapModLine> ModLines;

        [JsonProperty("Name")]
        public string Name;

        [JsonProperty("Type")]
        public string Type;


        public static MapMod CopyObject(MapMod mod) {
            MapMod newMod = new MapMod();
            newMod.ModLines = new List<MapModLine>(mod.ModLines);
            newMod.Name = mod.Name;
            newMod.Type = mod.Type;
            return newMod; 
        }
        
    }

    [Serializable]
    public class MapMods {

        [JsonProperty("MapMods")]
        public List<MapMod> MapModsList;
    }
}
