using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Telemachus.CameraSnapshots
{
    public class CurrentFlightCameraCapture : CameraCapture
    {
        public override string cameraManagerName()
        {
            return "TelemachusFlightCamera";
        }

        public override string cameraType()
        {
            return "FlightCamera";
        }

        

        public override void BeforeRenderNewScreenshot()
        {
            //UpdateCameras();
            //base.BeforeRenderNewScreenshot();
        }

        
    }
}