using System;
using System.Collections.Generic;
using System.Text;

public class VesselChangeDetector
{
    Vessel previousVessel = null;
    public delegate void VesselChange(Vessel vessel);
    private event VesselChange vesselChangeEvent;

    public void update(Vessel vessel){

        if (vessel != null)
        {
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
}

