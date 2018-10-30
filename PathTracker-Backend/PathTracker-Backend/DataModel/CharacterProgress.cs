using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using MongoDB.Bson.Serialization.Attributes;

namespace PathTracker_Backend
{
    [Serializable]
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

        [BsonIgnore]
        [JsonProperty("LevelProgress")]
        public Dictionary<int, double> LevelProgress = new Dictionary<int, double>();

        [JsonProperty("ExperienceProgress")]
        public long ExperienceProgress { get; set; }

        [JsonProperty("NonpenalizedExperience")]
        public long TotalExperienceNonPenalized { get; set; }

    }
}
