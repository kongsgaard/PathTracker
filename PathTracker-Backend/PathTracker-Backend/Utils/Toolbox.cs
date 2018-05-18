using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;

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
        
    }


}
