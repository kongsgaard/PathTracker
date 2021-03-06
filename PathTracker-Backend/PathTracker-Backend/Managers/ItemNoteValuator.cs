﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace PathTracker_Backend
{
    public class ItemNoteValuator : IItemRate {

        ResourceManager Resource;

        public ItemNoteValuator(ResourceManager resource) {
            Resource = resource;
        }

        public Tuple<string, double, ItemValueMode> CalculateItemValue(Item item) {

            string pattern = "^~price ([0-9.]+) ([a-zA-Z]+)";

            if(item.note == null) {
                return new Tuple<string, double, ItemValueMode>("", 0, ItemValueMode.Tentative);
            }

            Match m = Regex.Match(item.note, pattern);

            if (m.Success) {
                if (m.Groups.Count > 0) {
                    double num = 0;
                    if (double.TryParse(m.Groups[1].Value.Replace('.', ','), NumberStyles.Any, CultureInfo.InvariantCulture, out num)) {

                    }
                    else {
                        throw new Exception("Could not parse item price correctly");
                    }

                    string name = "";
                    if (Resource.CurrencyTagLookup.ContainsKey(m.Groups[2].Value)) {
                        name = Resource.CurrencyTagLookup[m.Groups[2].Value];
                    }
                    else {
                        Console.WriteLine("Parsed sale string, but failed to find correct itemtag in item note with note: " + item.note);
                    }

                    return new Tuple<string, double, ItemValueMode>(name, num, ItemValueMode.Tentative);
                }
            }
            

            return new Tuple<string, double, ItemValueMode>("", 0, ItemValueMode.Tentative);
        }
    }
}
