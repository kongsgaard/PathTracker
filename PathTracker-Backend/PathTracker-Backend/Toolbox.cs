using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PathTracker_Backend
{
    public static class Toolbox {

        public static (List<Item>, List<Item>) ItemDiffer(List<Item> previous, List<Item> current) {

            var added = current.Except(previous, new ItemComparer()).ToList();
            var removed = previous.Except(current, new ItemComparer()).ToList();
            
            return (added, removed);
        }

    }
}
