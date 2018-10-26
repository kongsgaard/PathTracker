using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PathTracker_Backend {
    public class PoeNinjaCurrencyRates : ICurrencyRates {

        ISettings Settings;

        public PoeNinjaCurrencyRates(ISettings settings) {
            Settings = settings;
        }
        /// <summary>
        /// A list of all cached item information from poe.ninja (non currency and map fragments)
        /// </summary>
        public List<ItemInformation> ItemInformations { get; private set; }

        /// <summary>
        /// A list of all cached currency information from poe.ninja (conversion values between currency types)
        /// </summary>
        public List<CurrencyInformation> Currency { get; private set; }

        public Dictionary<string, double> TypelineChaosValue = new Dictionary<string, double>(); 

        /// <summary>
        /// A list of all map fragment information from poe.ninja (similar to currency)
        /// </summary>
        public List<CurrencyInformation> Fragments { get; private set; }

        /// <summary>
        /// The last time the cached information was updated (information may only be updated once every two hours to save on bandwidth)
        /// </summary>
        public DateTime LastUpdate { get; private set; }

        /// <summary>
        /// Returns the current chaos value of the supplied item, or -1 if the item is unknown. 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <remarks>
        /// Please note: poe.ninja *only* cares about the number of links on an item, not the number of sockets.
        /// It's possible you may want to check poe.trade as well for the socket count and see where you actually lie.
        /// 
        /// Eg; a 6 socket (but only 4 link) Doomfletch Prism could be ~250c on poe.trade, but poe.ninja will only show ~85c because the *links* are low.
        /// </remarks>
        public double LookupChaosValue(string itemName) {

            if (TypelineChaosValue.ContainsKey(itemName)) {
                return TypelineChaosValue[itemName];
            }
            else
                return -1;
        }

        /// <summary>
        /// Updates the current item information caches from poe.ninja.
        /// </summary>
        /// <param name="league"></param>
        /// <returns></returns>
        public async Task Update() {

            string league = Settings.GetValue("CurrentLeague");

            // Only run an update every 2 hours, thanks. :)
            if (LastUpdate.AddMinutes(30) > DateTime.Now)
                return;

            var dateSuffix = $"{DateTime.Now.Year}-{DateTime.Now.Day:D2}-{DateTime.Now.Month:D2}";
            
            using (var client = new WebClient()) {
                Currency = await GetAndDeserialize<CurrencyInformation>(client, $"http://poe.ninja/api/Data/CurrencyOverview?league={league}&date={dateSuffix}&type=Currency");
                Fragments = await GetAndDeserialize<CurrencyInformation>(client, $"http://poe.ninja/api/Data/CurrencyOverview?league={league}&date={dateSuffix}&type=Fragment");
            }
            
            foreach(CurrencyInformation cinfo in Currency) {
                if (TypelineChaosValue.ContainsKey(cinfo.CurrencyTypeName)) {
                    TypelineChaosValue[cinfo.CurrencyTypeName] = cinfo.ChaosEquivalent;
                }
                else {
                    TypelineChaosValue.Add(cinfo.CurrencyTypeName, cinfo.ChaosEquivalent);
                }
            }

            foreach(CurrencyInformation finfo in Fragments) {
                if (TypelineChaosValue.ContainsKey(finfo.CurrencyTypeName)) {
                    TypelineChaosValue[finfo.CurrencyTypeName] = finfo.ChaosEquivalent;
                }
                else {
                    TypelineChaosValue.Add(finfo.CurrencyTypeName, finfo.ChaosEquivalent);
                }
            }

            LastUpdate = DateTime.Now;
        }
        
        private async Task<List<T>> GetAndDeserialize<T>(WebClient c, string url) {
            return JsonConvert.DeserializeObject<PoeNinjaLines<T>>(await c.DownloadStringTaskAsync(url)).Lines;
        }

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
    }
}
