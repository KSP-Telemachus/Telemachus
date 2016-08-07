using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Telemachus.CameraSnapshots
{
    public class CurrentFlightCameraCapture : CameraCapture
    {
        protected bool builtCameraDuplicates = false;

        public override string cameraManagerName()
        {
            return "TelemachusFlightCamera";
        }

        public override string cameraType()
        {
            return "FlightCamera";
        }

        protected override void LateUpdate()
        {
            if (CameraManager.Instance != null && HighLogic.LoadedSceneIsFlight && !builtCameraDuplicates)
            {
                UpdateCameras();
                builtCameraDuplicates = true;
            }

            foreach(KeyValuePair<string, Camera> KVP in cameraDuplicates)
            {
                debugCameraDetails(KVP.Value);
            }

            //base.LateUpdate();
        }

        public override void BeforeRenderNewScreenshot()
        {
            //UpdateCameras();
            base.BeforeRenderNewScreenshot();
        }
        
    }
}