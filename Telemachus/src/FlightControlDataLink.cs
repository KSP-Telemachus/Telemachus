using System;
using System.Collections.Generic;
using System.Text;

namespace Telemachus
{
    public class FlightControlDataLink
    {
        Part p = null;
        public FlightControlDataLink(Part p)
        {
            this.p = p;
        }
        public Boolean stage { get { p.Invoke("Staging.ActivateNextStage",1); return true; } set { } }
    }
}
