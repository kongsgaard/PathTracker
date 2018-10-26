using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracker_Backend
{
    public interface IWebRequestManager
    {
        Inventory GetInventory(string currentChar="");

        StashApiRequest GetStashtab(string name, string league = "", bool initializeTabs = false);
    }
}
