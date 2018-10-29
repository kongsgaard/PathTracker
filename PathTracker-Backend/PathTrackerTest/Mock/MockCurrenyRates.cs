using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using PathTracker_Backend;

namespace PathTrackerTest {
    public class MockCurrenyRates : ICurrencyRates {

        public List<CurrencyInformation> Currency { get; private set; }

        public Dictionary<string, double> TypelineChaosValue = new Dictionary<string, double>();


        public double LookupChaosValue(string itemName) {
            
            if (TypelineChaosValue.ContainsKey(itemName)) {
                return TypelineChaosValue[itemName];
            }
            else
                return -1;
        }

        public async Task Update() {

        }

        public async Task UpdateOnce() {
            Currency = await GetAndDeserialize<CurrencyInformation>();

            foreach (CurrencyInformation cinfo in Currency) {
                if (TypelineChaosValue.ContainsKey(cinfo.CurrencyTypeName)) {
                    TypelineChaosValue[cinfo.CurrencyTypeName] = cinfo.ChaosEquivalent;
                }
                else {
                    TypelineChaosValue.Add(cinfo.CurrencyTypeName, cinfo.ChaosEquivalent);
                }
            }


        }

        private async Task<List<T>> GetAndDeserialize<T>() {

            string currentDir = Directory.GetCurrentDirectory();
            
            return JsonConvert.DeserializeObject<PoeNinjaLines<T>>(File.ReadAllText(currentDir + "/TestData/TestDataPoENinjaCurrency.json")).Lines;
        }


        #region Internal classes
        internal class PoeNinjaLines<T> {
            public List<T> Lines { get; set; }
        }

        public class CurrencyInformation {
            public string CurrencyTypeName { get; set; }
            public ValueInfo Pay { get; set; }
            public ValueInfo Receive { get; set; }
            public object PaySparkLine { get; set; }
            public object ReceiveSparkLine { get; set; }
            public double ChaosEquivalent { get; set; }

            /// <inheritdoc />
            public override string ToString() {
                return $"{CurrencyTypeName}, {nameof(ChaosEquivalent)}: {ChaosEquivalent}";
            }

            public class ValueInfo {
                public int Id { get; set; }
                public int LeagueId { get; set; }
                public int PayCurrencyId { get; set; }
                public int GetCurrencyId { get; set; }
                public DateTime SampleTimeUtc { get; set; }
                public int Count { get; set; }
                public double Value { get; set; }
                public int DataPointCount { get; set; }
            }
        }

        public class ItemInformation {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Icon { get; set; }
            public int MapTier { get; set; }
            public int LevelRequired { get; set; }
            public string BaseType { get; set; }
            public int StackSize { get; set; }
            public string Variant { get; set; }
            public string ProphecyText { get; set; }
            public string ArtFilename { get; set; }
            public int Links { get; set; }
            public int ItemClass { get; set; }
            // Really only useful if you care about value changes up or down.
            public SparkLine Sparkline { get; set; }
            public List<ItemModifier> ImplicitModifiers { get; set; }
            public List<ItemModifier> ExplicitModifiers { get; set; }
            public string FlavourText { get; set; }
            public string ItemType { get; set; }
            public double ChaosValue { get; set; }
            public double ExaltedValue { get; set; }
            public int Count { get; set; }

            /// <inheritdoc />
            public override string ToString() {
                return $"{Name}, {nameof(ChaosValue)}: {ChaosValue}";
            }
        }

        public class ItemModifier {
            public string Text { get; set; }
            public bool Optional { get; set; }
        }

        public class SparkLine {
            public List<float?> Data { get; set; }
            public float TotalChange { get; set; }
        }
        #endregion

    }
}
