using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Telemachus.CameraSnapshots
{
    public class CurrentFlightCameraCapture : CameraCapture
    {
        protected static new string cameraContainerNamePrefix = "Telemachus Camera Container - ";

        public override string cameraManagerName()
        {
            return "TelemachusFlightCamera";
        }

        public override void BeforeRenderNewScreenshot()
        {
            UpdateCameras();
            base.BeforeRenderNewScreenshot();
        }
        
    }
}