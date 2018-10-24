using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracker_Backend
{
    public class ItemRecipeValuator : IItemRate {

        public Tuple<string, double, ItemValueMode> CalculateItemValue(Item item) {

            //6-Link
            int CurrentGroupID = -1;
            bool AllLinked = true;
            if(item.Sockets != null) {
                foreach (var s in item.Sockets) {
                    if (CurrentGroupID == -1) {
                        CurrentGroupID = s.GroupId;
                    }
                    else if (CurrentGroupID == s.GroupId) {

                    }
                    else {
                        AllLinked = false;
                        break;
                    }
                }
                if (AllLinked && item.Sockets.Count == 6) {
                    return new Tuple<string, double, ItemValueMode>("Divine Orb", 1, ItemValueMode.Confirmed);
                }
            }
            
            

            //6-Socket
            if(item.Sockets != null) {
                if (item.Sockets.Count == 6) {
                    return new Tuple<string, double, ItemValueMode>("Jeweller's Orb", 1, ItemValueMode.Confirmed);
                }
            }
            

            //GCP
            if(item.Category.Gems.Count > 0) {
                foreach(var prop in item.Properties.Where(x => x.Name== "Quality")) {

                    char[] digits = new char[2];
                    int pos = 0;
                    foreach(var ch in prop.Values[0][0]) {
                        if (char.IsDigit(ch)) {
                            digits[pos] = ch;
                            pos++;
                        }
                    }

                    string values = new String(digits);
                    double valint = -1;
                    if(double.TryParse(values, out valint)) {
                        if(valint == 0) {
                            throw new Exception("Quality of gem not calculated properly");
                        }
                        else {
                            if(valint == 20) {
                                return new Tuple<string, double, ItemValueMode>("Gemcutter's Prism", 1, ItemValueMode.Confirmed);
                            }
                            else {
                                return new Tuple<string, double, ItemValueMode>("Gemcutter's Prism", valint / 40, ItemValueMode.Confirmed);
                            }
                        }
                    }
                    else if(valint == -1) {
                        throw new Exception("Quality of gem not calculated properly");
                    }
                    else {
                        throw new Exception("Could not parse quality on gem!");
                    }
                }
            }

            //Chrome
            CurrentGroupID = -1;
            bool BlueFound = false;
            bool GreenFound = false;
            bool RedFound = false;

            foreach (var s in item.Sockets) {
                if (CurrentGroupID != s.GroupId) {
                    BlueFound = false;
                    GreenFound = false;
                    RedFound = false;
                    CurrentGroupID = s.GroupId;

                    if (s.Type == SocketType.Str) {
                        RedFound = true;
                    }
                    else if(s.Type == SocketType.Int) {
                        BlueFound = true;
                    }
                    else if(s.Type == SocketType.Dex) {
                        GreenFound = true;
                    }
                }
                else if (CurrentGroupID == s.GroupId) {
                    if (s.Type == SocketType.Str) {
                        RedFound = true;
                    }
                    else if (s.Type == SocketType.Int) {
                        BlueFound = true;
                    }
                    else if (s.Type == SocketType.Dex) {
                        GreenFound = true;
                    }
                }

                if(BlueFound && RedFound && GreenFound) {
                    return new Tuple<string, double, ItemValueMode>("Chromatic Orb", 1, ItemValueMode.Confirmed);
                }
            }
            return new Tuple<string, double, ItemValueMode>("",0, ItemValueMode.Confirmed);
        }
    }
}
