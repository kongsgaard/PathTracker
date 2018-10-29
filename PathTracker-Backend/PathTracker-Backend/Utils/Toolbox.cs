using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

namespace PathTracker_Backend
{
    public static class Toolbox {

        public static Dictionary<T, double> AddDictionaries<T>(Dictionary<T, double> Dict1, Dictionary<T, double> Dict2) {

            foreach (var kvp in Dict2) {
                if (Dict1.ContainsKey(kvp.Key)) {
                    Dict1[kvp.Key] = Dict2[kvp.Key] + Dict1[kvp.Key];
                }
                else {
                    Dict1[kvp.Key] = Dict2[kvp.Key];
                }
            }

            return null;
        }

        public static Dictionary<T, int> AddDictionaries<T>(Dictionary<T, int> Dict1, Dictionary<T, int> Dict2) {

            foreach (var kvp in Dict2) {
                if (Dict1.ContainsKey(kvp.Key)) {
                    Dict1[kvp.Key] = Dict2[kvp.Key] + Dict1[kvp.Key];
                }
                else {
                    Dict1[kvp.Key] = Dict2[kvp.Key];
                }
            }

            return Dict1;
        }

        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public static int LevenshteinDistance(string s, string t) {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0) {
                return m;
            }

            if (m == 0) {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++) {
            }

            for (int j = 0; j <= m; d[0, j] = j++) {
            }

            // Step 3
            for (int i = 1; i <= n; i++) {
                //Step 4
                for (int j = 1; j <= m; j++) {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    
                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        public static T Clone<T>(this T source) {
            if (!typeof(T).IsSerializable) {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null)) {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream()) {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

    }


}
