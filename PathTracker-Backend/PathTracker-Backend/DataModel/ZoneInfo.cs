using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace PathTracker_Backend {

    [Serializable]
    public class ZoneInfo {
        
        [BsonElement(elementName: "monsterLevel")]
        [JsonProperty("monsterLevel")]
        public string monsterLevel { get; set; }

        [BsonElement(elementName: "delveDepth")]
        [JsonProperty("delveDepth")]
        public string delveDepth { get; set; }

    }
    
}

