//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using Telemachus;

namespace Telemachus
{
    public class VesselChangeDetector
    {
        #region Fields

        Vessel previousVessel = null;
        public delegate void VesselChange(Vessel vessel);
        private event VesselChange vesselChangeEvent;
        public static bool hasTelemachusPart = false;

        #endregion

        #region Events

        public void update(Vessel vessel)
        {

            if (vessel != null)
            {
                updateHasTelemachusPart(vessel);

                if (previousVessel != null)
                {
                    if (vessel.id != previousVessel.id || vessel.parts.Count != previousVessel.parts.Count)
                    {
                        fireVesselChanged(vessel);
                    }
                }
                else
                {
                    fireVesselChanged(vessel);
                }
            }

            previousVessel = vessel;
        }

        public void suscribe(VesselChange vc)
        {
            vesselChangeEvent += vc;
        }

        private void fireVesselChanged(Vessel vessel)
        {
            if (vesselChangeEvent != null)
            {
                vesselChangeEvent(vessel);
            }
        }

        private void updateHasTelemachusPart(Vessel vessel)
        {
            try
            {

                hasTelemachusPart = vessel.parts.FindAll(p => p.Modules.Contains("TelemachusDataLink")).Count > 0;
            }
            catch (Exception e)
            {
                PluginLogger.debug(e.Message + " " + e.StackTrace);
            }
        }

        #endregion
    }
}
