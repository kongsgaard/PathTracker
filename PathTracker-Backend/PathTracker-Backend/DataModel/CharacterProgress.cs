using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace PathTracker_Backend
{
    public class CharacterProgress
    {
        [JsonProperty("EquippedItems")]
        public List<Item> EquippedItems = new List<Item>();

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("League")]
        public string League { get; set; }

        [JsonProperty("AscendencyClass")]
        public int AscendencyClass { get; set; }

        [JsonProperty("LevelProgress")]
        public Dictionary<int, double> LevelProgress = new Dictionary<int, double>();

        [JsonProperty("ExperienceProgress")]
        public long ExperienceProgress { get; set; }

        [JsonProperty("NonpenalizedExperience")]
        public long TotalExperienceNonPenalized { get; set; }

    }
}
