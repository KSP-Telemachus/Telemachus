//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;

namespace Telemachus
{
    public class PausedDataLink
    {
        TelemachusPowerDrain tpd = null;

        public PausedDataLink(TelemachusPowerDrain tpd)
        {
            this.tpd = tpd;
        }

        public Boolean paused 
        { 
            get 
            {
                return FlightDriver.Pause || (tpd == null ? false : !tpd.active || !tpd.activeToggle); 
            } 
            set { } 
        }
    }
}
