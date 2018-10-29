using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace PathTracker_Backend {
    public class Inventory {
        [JsonProperty("character")]
        public Character Character { get; set; }

        public List<Item> Items { get; set; }

        private Inventory(Character chara, List<Item> it) {
            Character = chara;
            Items = it;
        }

        public Inventory Copy() {

            return new Inventory(Character, );
        }
    }

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

        private Character(string name, string lea, int classid, int ascendency, string clas, int level, long exp)
        {
            Name = name;
            League = lea;
            ClassId = classid;
            AscendencyClass = ascendency;
            Class = clas;
            Level = level;
            Experience = exp;
        }

        public Character Copy() {
            return new Character(Name, League, ClassId, AscendencyClass, Class, Level, Experience);
        }
        
    }
}
