using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace PathTracker_Backend {

    [Serializable]
    public class Inventory {
        [JsonProperty("character")]
        public Character Character { get; set; }

        public List<Item> Items { get; set; }
        
    }

    [Serializable]
    public class Character {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("league")]
        public string League { get; set; }

        [JsonProperty("classId")]
        public int ClassId { get; set; }

        [JsonProperty("ascendancyClass")]
        public int AscendencyClass { get; set; }

        [JsonProperty("class")]
        public string Class { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("experience")]
        public long Experience { get; set; }

    }
}
