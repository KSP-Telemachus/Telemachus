using System;
using System.Collections.Generic;
using System.Text;

namespace Telemachus
{
    public class PausedDataLink
    {
        public Boolean paused { get { return FlightDriver.Pause; } set { } }
    }
}
