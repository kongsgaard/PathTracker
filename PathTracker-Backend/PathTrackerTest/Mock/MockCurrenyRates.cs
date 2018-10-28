using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PathTracker_Backend;

namespace PathTrackerTest {
    public class MockCurrenyRates : ICurrencyRates {

        public double LookupChaosValue(string itemName) {
            throw new NotImplementedException();
        }

        public Task Update() {
            return null;
        }

    }
}
