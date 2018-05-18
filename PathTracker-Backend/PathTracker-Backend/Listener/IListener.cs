using System;
using System.Collections.Generic;
using System.Text;

namespace PathTracker_Backend
{
    public interface IListener
    {
        void StartListening();

        void StopListening();
    }
}
