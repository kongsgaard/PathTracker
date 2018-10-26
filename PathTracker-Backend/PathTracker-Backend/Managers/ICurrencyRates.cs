using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracker_Backend
{
    public interface ICurrencyRates
    {
        double LookupChaosValue(Item item);

        Task Update(string league);
    }
}
