using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracker_Backend
{
    public interface ISettings
    {
        string GetValue(string key);

        void SetValue(string key, string value);
    }
}
