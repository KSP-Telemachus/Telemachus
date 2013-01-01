//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;

namespace Telemachus
{
    public class FlightControlDataLink
    {
        TelemachusDataLink p = null;
        public FlightControlDataLink(TelemachusDataLink p)
        {
            this.p = p;
        }

        public Boolean stage { get { checkAndInvoke(p, "activateNextStage"); return true; } set { } }
        public Boolean throttleUp { get { checkAndInvoke(p, "throttleUp"); return true; } set { } }
        public Boolean throttleDown { get { checkAndInvoke(p, "throttleDown"); return true; } set { } }


        private void checkAndInvoke(TelemachusDataLink p, string methodName)
        {
            if (!p.IsInvoking(methodName))
            {
                p.Invoke(methodName, 0);
            }
        }
    }
}
