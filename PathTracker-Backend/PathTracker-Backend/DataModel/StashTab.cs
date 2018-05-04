using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace PathTracker_Backend {
    public class StashTab {

        [JsonProperty("hidden")]
        bool Hidden { get; set; }

        [JsonProperty("selected")]
        bool Selected { get; set; }

        [JsonProperty("i")]
        public int Index { get; set; }

        [JsonProperty("n")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public TabType Type { get; set; }

        public List<Item> Items { get; set; }
    }

    public enum TabType { NormalStash, PremiumStash, QuadStash, DivinationCardStash, CurrencyStash, EssenceStash, MapStash }
}
