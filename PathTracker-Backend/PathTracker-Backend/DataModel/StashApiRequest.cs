using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace PathTracker_Backend {
    public class StashApiRequest {
        [JsonProperty("numTabs")]
        public int NumTabs { get; set; }

        [JsonProperty("tabs")]
        public List<StashTab> StashTabs { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }
    }
}
