using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PathTracker_Backend;

namespace PathTrackerTest {
    public class MockSettings : ISettings {
        public string GetValue(string key) {
            if (keyValuePairs.ContainsKey(key)) {
                return keyValuePairs[key];
            }
            else {
                throw new Exception("Mock settings did not contain info for key:" + key);
            }
        }

        public void SetValue(string key, string value) {
            if (keyValuePairs.ContainsKey(key)) {
                keyValuePairs[key] = value;
            }
            else {
                keyValuePairs.Add(key, value);
            }
        }

        private Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
    }
}
