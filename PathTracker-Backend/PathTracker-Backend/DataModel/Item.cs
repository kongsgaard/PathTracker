using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace PathTracker_Backend {
    public class Item {
        [JsonProperty("abyssJewel")]
        public bool AbyssJewel { get; set; }

        [JsonProperty("additionalProperties")]
        public List<Property> AdditionalProperties { get; set; }

        [JsonProperty("category")]
        public Category Category { get; set; }

        [JsonProperty("corrupted")]
        public bool Corrupted { get; set; }

        [JsonProperty("cosmeticMods")]
        public List<string> CosmeticMods { get; set; }

        [JsonProperty("craftedMods")]
        public List<string> CraftedMods { get; set; }

        [JsonProperty("descrText")]
        public string Description { get; set; }

        [JsonProperty("dubplicated")]
        public bool Dubplicated { get; set; }

        [JsonProperty("elder")]
        public bool Elder { get; set; }

        [JsonProperty("enchantMods")]
        public List<string> EnchantMods { get; set; }

        [JsonProperty("explicitMods")]
        public List<string> ExplicitMods { get; set; }

        [JsonProperty("flavourText")]
        public List<string> FlavourText { get; set; }

        [JsonProperty("frameType")]
        public int FrameType { get; set; }

        [JsonProperty("h")]
        public int Height { get; set; }

        [JsonProperty("icon")]
        public string IconUrl { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("identified")]
        public bool Identified { get; set; }

        [JsonProperty("ilvl")]
        public int Ilvl { get; set; }

        [JsonProperty("implicitMods")]
        public List<string> ImplicitMods { get; set; }

        [JsonProperty("inventoryId")]
        public string InventoryId { get; set; }

        [JsonProperty("isRelic")]
        public bool IsRelic { get; set; }

        [JsonProperty("league")]
        public string League { get; set; }

        [JsonProperty("lockedToCharacter")]
        public bool LockedToCharacter { get; set; }

        [JsonProperty("maxStackSize")]
        public int MaxStackSize { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nextLevelRequierments")]
        public List<Property> NextLevelReqs { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("properties")]
        public List<Property> Properties { get; set; }

        [JsonProperty("prophecyDiffText")]
        public string ProphecyDiffText { get; set; }

        [JsonProperty("prophecyText")]
        public string ProphecyText { get; set; }

        [JsonProperty("requirements")]
        public List<Property> Requirements { get; set; }

        [JsonProperty("secDescrText")]
        public string SecDescrText { get; set; }

        [JsonProperty("shaper")]
        public bool Shaper { get; set; }

        [JsonProperty("socketedItems")]
        public List<Item> SocketedItems { get; set; }

        [JsonProperty("sockets")]
        public List<Socket> Sockets { get; set; }

        [JsonProperty("stackSize")]
        public int StackSize { get; set; }

        [JsonProperty("support")]
        public bool Support { get; set; }

        [JsonProperty("talismanTier")]
        public int TalismanTier { get; set; }

        [JsonProperty("typeLine")]
        public string TypeLine { get; set; }

        [JsonProperty("utilityMods")]
        public List<string> UtilityMods { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }

        [JsonProperty("w")]
        public int Width { get; set; }

        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }
    }

    /// <summary>
    /// Comparer for item, used when calculating Difference between lists of items
    /// </summary>
    public class ItemComparer : IEqualityComparer<Item> {
        public int GetHashCode(Item item) {
            if (item == null) {
                return 0;
            }
            return item.Id.GetHashCode();
        }

        public bool Equals(Item i1, Item i2) {
            if (object.ReferenceEquals(i1, i2)) {
                return true;
            }
            if (object.ReferenceEquals(i1, null) ||
                object.ReferenceEquals(i2, null)) {
                return false;
            }
            return i1.Id == i2.Id;
        }
    }

    public class Property {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("values")]
        public List<List<string>> Values { get; set; }

        [JsonProperty("displayMode")]
        public int DisplayMode { get; set; }

        [JsonProperty("type")]
        public int PropertyType { get; set; }

        [JsonProperty("progress")]
        public double XpProgress { get; set; }

    }

    public class Category {
        [JsonProperty("maps")]
        public List<string> Maps { get; set; }

        [JsonProperty("currency")]
        public List<string> Currency { get; set; }

        [JsonProperty("jewels")]
        public List<string> Jewels { get; set; }

        [JsonProperty("gems")]
        public List<string> Gems { get; set; }

        [JsonProperty("cards")]
        public List<string> Cards { get; set; }

        [JsonProperty("flasks")]
        public List<string> Flasks { get; set; }

        [JsonProperty("weapons")]
        public List<string> Weapons { get; set; }

        [JsonProperty("accessories")]
        public List<string> Accessories { get; set; }
    }

    public class Socket {
        [JsonProperty("group")]
        public int GroupId { get; set; }

        /// <summary>
        /// S=str, I=int, D=dex, G=white, false=abyss
        /// </summary>
        [JsonProperty("attr")]
        private string Attr { get; set; }

        public SocketType Type {
            get {
                switch (Attr) {
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
                        throw new Exception("Non-impletemend sockettype: " + Attr + " encountered");
                }
            }

        }
    }

    public enum SocketType { Str, Int, Dex, White, Abyss }

}

