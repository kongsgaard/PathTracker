using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace PathTracker_Backend {

    [Serializable]
    public class ZoneProperty {

        [BsonElement(elementName: "zoneInfo")]
        [JsonProperty("zoneInfo")]
        public ZoneInfo zoneInfo { get; set; }

        [BsonElement(elementName: "zoneInfoParseStatus")]
        [JsonProperty("zoneInfoParseStatus")]
        public ParseStatus zoneInfoParseStatus { get; set; }

        [BsonElement(elementName: "zoneInfoScreenshotPath")]
        [JsonProperty("zoneInfoScreenshotPath")]
        public string zoneInfoScreenshotPath { get; set; }


        [BsonElement(elementName: "mapMods")]
        [JsonProperty("mapMods")]
        public List<MapMod> mapMods { get; set; }

        [BsonElement(elementName: "NonParsedMapMods")]
        [JsonProperty("NonParsedMapMods")]
        public List<string> NonParsedMapMods { get; set; }


        [BsonElement(elementName: "mapModsParseStatus")]
        [JsonProperty("mapModsParseStatus")]
        public ParseStatus mapModsParseStatus { get; set; }

        [BsonElement(elementName: "mapModsScreenshotPath")]
        [JsonProperty("mapModsScreenshotPath")]
        public string mapModsScreenshotPath { get; set; }

        [BsonElement(elementName: "originalScreenshotPath")]
        [JsonProperty("originalScreenshotPath")]
        public string originalScreenshotPath { get; set; }


    }

}

