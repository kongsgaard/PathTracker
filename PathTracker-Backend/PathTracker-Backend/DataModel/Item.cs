using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace PathTracker_Backend {

    [Serializable]
    public class Item {

        //[BsonId]
        //public string _id { get; set; }

        [BsonElement(elementName: "abyssJewel")]
        [JsonProperty("abyssJewel")]
        public bool abyssJewel { get; set; }

        [BsonElement(elementName: "additionalProperties")]
        [JsonProperty("additionalProperties")]
        public List<Property> additionalProperties { get; set; }

        [BsonElement(elementName: "category")]
        [JsonProperty("category")]
        public Category category { get; set; }

        [BsonElement(elementName: "corrupted")]
        [JsonProperty("corrupted")]
        public bool corrupted { get; set; }

        [BsonElement(elementName: "cosmeticMods")]
        [JsonProperty("cosmeticMods")]
        public List<string> cosmeticMods { get; set; }

        [BsonElement(elementName: "craftedMods")]
        [JsonProperty("craftedMods")]
        public List<string> craftedMods { get; set; }

        [BsonElement(elementName: "descrText")]
        [JsonProperty("descrText")]
        public string descrText { get; set; }

        [BsonElement(elementName: "dubplicated")]
        [JsonProperty("dubplicated")]
        public bool dubplicated { get; set; }

        [BsonElement(elementName: "")]
        [JsonProperty("elder")]
        public bool elder { get; set; }

        [BsonElement(elementName: "enchantMods")]
        [JsonProperty("enchantMods")]
        public List<string> enchantMods { get; set; }

        [BsonElement(elementName: "explicitMods")]
        [JsonProperty("explicitMods")]
        public List<string> explicitMods { get; set; }

        [BsonElement(elementName: "flavourText")]
        [JsonProperty("flavourText")]
        public List<string> flavourText { get; set; }

        [BsonElement(elementName: "frameType")]
        [JsonProperty("frameType")]
        public int frameType { get; set; }

        [BsonElement(elementName: "h")]
        [JsonProperty("h")]
        public int h { get; set; }

        [BsonElement(elementName: "icon")]
        [JsonProperty("icon")]
        public string icon { get; set; }

        [BsonElement(elementName: "itemId")]
        [JsonProperty("id")]
        public string itemId { get; set; }

        [BsonElement(elementName: "identified")]
        [JsonProperty("identified")]
        public bool identified { get; set; }

        [BsonElement(elementName: "ilvl")]
        [JsonProperty("ilvl")]
        public int ilvl { get; set; }

        [BsonElement(elementName: "implicitMods")]
        [JsonProperty("implicitMods")]
        public List<string> implicitMods { get; set; }

        [BsonElement(elementName: "inventoryId")]
        [JsonProperty("inventoryId")]
        public string inventoryId { get; set; }

        [BsonElement(elementName: "isRelic")]
        [JsonProperty("isRelic")]
        public bool isRelic { get; set; }

        [BsonElement(elementName: "league")]
        [JsonProperty("league")]
        public string league { get; set; }

        [BsonElement(elementName: "lockedToCharacter")]
        [JsonProperty("lockedToCharacter")]
        public bool lockedToCharacter { get; set; }

        [BsonElement(elementName: "maxStackSize")]
        [DefaultValue(0)]
        [JsonProperty("maxStackSize", DefaultValueHandling = DefaultValueHandling.Populate)]
        public int maxStackSize { get; set; }

        [BsonElement(elementName: "name")]
        [JsonProperty("name")]
        public string name { get; set; }

        [BsonElement(elementName: "nextLevelRequierments")]
        [JsonProperty("nextLevelRequierments")]
        public List<Property> nextLevelRequierments { get; set; }

        [BsonElement(elementName: "note")]
        [JsonProperty("note")]
        public string note { get; set; }

        [BsonElement(elementName: "properties")]
        [JsonProperty("properties")]
        public List<Property> properties { get; set; }

        [BsonElement(elementName: "prophecyDiffText")]
        [JsonProperty("prophecyDiffText")]
        public string prophecyDiffText { get; set; }

        [BsonElement(elementName: "prophecyText")]
        [JsonProperty("prophecyText")]
        public string prophecyText { get; set; }

        [BsonElement(elementName: "requirements")]
        [JsonProperty("requirements")]
        public List<Property> requirements { get; set; }

        [BsonElement(elementName: "secDescrText")]
        [JsonProperty("secDescrText")]
        public string secDescrText { get; set; }

        [BsonElement(elementName: "shaper")]
        [JsonProperty("shaper")]
        public bool shaper { get; set; }

        [BsonElement(elementName: "socketedItems")]
        [JsonProperty("socketedItems")]
        public List<Item> socketedItems { get; set; }

        [BsonElement(elementName: "sockets")]
        [JsonProperty("sockets")]
        public List<Socket> sockets { get; set; }

        [BsonElement(elementName: "stackSize")]
        [JsonProperty("stackSize")]
        public int stackSize { get; set; }

        [BsonElement(elementName: "support")]
        [JsonProperty("support")]
        public bool support { get; set; }

        [BsonElement(elementName: "talismanTier")]
        [JsonProperty("talismanTier")]
        public int talismanTier { get; set; }

        [BsonElement(elementName: "typeLine")]
        [JsonProperty("typeLine")]
        public string typeLine { get; set; }

        [BsonElement(elementName: "utilityMods")]
        [JsonProperty("utilityMods")]
        public List<string> utilityMods { get; set; }

        [BsonElement(elementName: "verified")]
        [JsonProperty("verified")]
        public bool verified { get; set; }

        [BsonElement(elementName: "w")]
        [JsonProperty("w")]
        public int w { get; set; }

        [BsonElement(elementName: "x")]
        [JsonProperty("x")]
        public int x { get; set; }

        [BsonElement(elementName: "y")]
        [JsonProperty("y")]
        public int y { get; set; }

        [BsonElement(elementName: "itemValues")]
        [JsonProperty("itemValues")]
        public ItemValues itemValues = new ItemValues();

        [BsonElement(elementName: "CurrentZoneID")]
        [JsonProperty("CurrentZoneID")]
        public string CurrentZoneID { get; set; }
   
    }

    [Serializable]
    public class ItemValues {
        [BsonElement(elementName: "currentChaosValue")]
        [JsonProperty("currentChaosValue")]
        public double currentChaosValue { get; set; }

        [BsonElement(elementName: "valueMode")]
        [JsonProperty("valueMode")]
        public ItemValueMode valueMode = ItemValueMode.Unset;

        [BsonElement(elementName: "values")]
        [JsonProperty("values")]
        public List<ItemValue> values = new List<ItemValue>();
        
    }

    [Serializable]
    public class ItemValue {
        [BsonElement(elementName: "currentChaosValue")]
        [JsonProperty("currentChaosValue")]
        public double currentChaosValue { get; set; }

        [BsonElement(elementName: "valueMode")]
        [JsonProperty("valueMode")]
        public ItemValueMode valueMode = ItemValueMode.Unset;

        [BsonElement(elementName: "zoneID")]
        [JsonProperty("zoneID")]
        public string zoneID { get; set; }

        [BsonElement(elementName: "setAt")]
        [JsonProperty("setAt")]
        public DateTime setAt { get; set; }
        
    }

    /// <summary>
    /// Comparer for item, used when calculating Difference between lists of items
    /// </summary>
    public class ItemComparer : IEqualityComparer<Item> {
        public int GetHashCode(Item item) {
            if (item == null) {
                return 0;
            }
            return item.itemId.GetHashCode();
        }

        public bool Equals(Item i1, Item i2) {
            if (object.ReferenceEquals(i1, i2)) {
                return true;
            }
            if (object.ReferenceEquals(i1, null) ||
                object.ReferenceEquals(i2, null)) {
                return false;
            }
            return i1.itemId == i2.itemId;
        }
    }

    [Serializable]
    public class Property {
        [BsonElement(elementName: "name")]
        [JsonProperty("name")]
        public string name { get; set; }

        [BsonElement(elementName: "values")]
        [JsonProperty("values")]
        public List<List<string>> values { get; set; }

        [BsonElement(elementName: "displayMode")]
        [JsonProperty("displayMode")]
        public int displayMode { get; set; }

        [BsonElement(elementName: "type")]
        [JsonProperty("type")]
        public int type { get; set; }

        [BsonElement(elementName: "progress")]
        [JsonProperty("progress")]
        public double progress { get; set; }
        
    }

    [Serializable]
    public class Category {
        [BsonElement(elementName: "maps")]
        [JsonProperty("maps")]
        public List<string> maps { get; set; }

        [BsonElement(elementName: "currency")]
        [JsonProperty("currency")]
        public List<string> currency { get; set; }

        [BsonElement(elementName: "jewels")]
        [JsonProperty("jewels")]
        public List<string> jewels { get; set; }

        [BsonElement(elementName: "gems")]
        [JsonProperty("gems")]
        public List<string> gems { get; set; }

        [BsonElement(elementName: "cards")]
        [JsonProperty("cards")]
        public List<string> cards { get; set; }

        [BsonElement(elementName: "flasks")]
        [JsonProperty("flasks")]
        public List<string> flasks { get; set; }

        [BsonElement(elementName: "weapons")]
        [JsonProperty("weapons")]
        public List<string> weapons { get; set; }

        [BsonElement(elementName: "accessories")]
        [JsonProperty("accessories")]
        public List<string> accessories { get; set; }
        
        
    }

    [Serializable]
    public class Socket {
        [BsonElement(elementName: "group")]
        [JsonProperty("group")]
        public int group { get; set; }

        /// <summary>
        /// S=str, I=int, D=dex, G=white, false=abyss
        /// </summary>
        [BsonElement(elementName: "attr")]
        [JsonProperty("attr")]
        private string attr { get; set; }

        public SocketType Type {
            get {
                switch (attr) {
                    case "S":
                        return SocketType.Str;
                    case "I":
                        return SocketType.Int;
                    case "D":
                        return SocketType.Dex;
                    case "G":
                        return SocketType.White;
                    case "false":
                        return SocketType.Abyss;
                    default:
                        throw new Exception("Non-impletemend sockettype: " + attr + " encountered");
                }
            }

        }
        
    }

    public enum SocketType { Str, Int, Dex, White, Abyss }

    public enum ItemChangedTypes { ItemAdded, ItemNoteModified, ItemRemoved}

}

