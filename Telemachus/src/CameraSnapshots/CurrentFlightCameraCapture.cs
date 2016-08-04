using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Telemachus.CameraSnapshots
{
    public class CurrentFlightCameraCapture : CameraCapture
    {
        public new string cameraManagerName = "TelemachusFlightCamera";
        protected static new string cameraContainerNamePrefix = "Telemachus Camera Container - ";

        public override void BeforeRenderNewScreenshot()
        {
            UpdateCameras();
            base.BeforeRenderNewScreenshot();
        }
        
    }
}