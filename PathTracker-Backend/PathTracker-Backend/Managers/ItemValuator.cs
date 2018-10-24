using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracker_Backend
{
    public class ItemValuator
    {
        public ICurrencyRates currencyRates;
        Dictionary<int,IItemRate> orderedItemValuators = new Dictionary<int, IItemRate>();
        
        public ItemValuator() {
            currencyRates = new PoeNinjaCurrencyRates();
            currencyRates.Update();
            
            orderedItemValuators.Add(1, new ItemNoteValuator());
            orderedItemValuators.Add(2, new ItemRecipeValuator());
        }

        public Tuple<double, ItemValueMode> ItemChaosValue(Item item) {
            
            foreach(IItemRate r in orderedItemValuators.OrderBy(x => x.Key).Select(y => y.Value)) {
                var result = r.CalculateItemValue(item);

                if(result.Item2 > 0) {
                    return new Tuple<double, ItemValueMode>(CurrencyChaosValue(result.Item1, result.Item2), result.Item3);
                }
            }

            return new Tuple<double, ItemValueMode>(0, ItemValueMode.Confirmed);
        }

        public double CurrencyChaosValue(string name, double count) {

            double value = currencyRates.LookupChaosValue(name);

            if(value != -1) {
                return value * count;
            }
            else {
                throw new Exception("Could not find value of currency: " + name);
            }
        }

        
    }

    public enum ItemValueMode { Confirmed, Tentative}
}
