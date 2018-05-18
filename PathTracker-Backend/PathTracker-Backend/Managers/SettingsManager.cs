using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.IO;
using System.Threading;

namespace PathTracker_Backend
{
    public class SettingsManager
    {
        private static Mutex SettingsMutex = new Mutex();

        private SettingsManager(string file) {
            Configuration = BuildJsonConfiguration(file);
        }
        
        private IConfiguration Configuration;
        
        private IConfiguration BuildJsonConfiguration(string jsonFile) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(jsonFile);
            return builder.Build();
        }

        /// <summary>
        /// Return the current value for a Key
        /// </summary>
        /// <param name="key">Key to return value for</param>
        /// <returns></returns>
        public string GetValue(string key) {
            SettingsMutex.WaitOne();
            string Value = Configuration[key];
            SettingsMutex.ReleaseMutex();
            return Value;
        }

        /// <summary>
        /// Function to set Key to Value
        /// </summary>
        /// <param name="key">Key identifying the setting</param>
        /// <param name="value">Value to update the key</param>
        public void SetValue(string key, string value) {
            SettingsMutex.WaitOne();
            Configuration[key] = value;
            SettingsMutex.ReleaseMutex();
        }
        
        private static SettingsManager Manager = new SettingsManager("Settings.json");
        
        /// <summary>
        /// Gets the singleton instance of SettingsManager
        /// </summary>
        public static  SettingsManager Instance {
            get {
                return Manager;
            }
        }
    }
}
