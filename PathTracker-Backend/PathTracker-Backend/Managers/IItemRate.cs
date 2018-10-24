using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracker_Backend
{
    public interface IItemRate
    {
        Tuple<string, double, ItemValueMode> CalculateItemValue(Item item);
    }
}
