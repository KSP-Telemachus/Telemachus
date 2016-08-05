using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Telemachus.CameraSnapshots
{
    class RasterPropMonitorCameraCapture : CameraCapture
    {
        public RasterPropMonitorCamera rpmCamera;
        protected static string cameraManagerNamePrefix = "RPMCamera:";

        public override string cameraManagerName()
        {
            return cameraManagerNamePrefix + rpmCamera.cameraName;
        }

        protected bool builtCameraDuplicates = false;

        protected override void LateUpdate()
        {
            /*if (CameraManager.Instance != null && HighLogic.LoadedSceneIsFlight && rpmCamera != null && !builtCameraDuplicates)
            {
                UpdateCameras();
                builtCameraDuplicates = true;
            }*/

            base.LateUpdate();
        }

        public override void BeforeRenderNewScreenshot()
        {
            UpdateCameras();
            base.BeforeRenderNewScreenshot();
        }

        public override void additionalCameraUpdates(Camera cam)
        {
            PluginLogger.debug("ROTATION:" + rpmCamera.rotateCamera);
            cam.transform.position = rpmCamera.part.transform.position;
            base.additionalCameraUpdates(cam);
            PluginLogger.debug("PAST BASE");
            cam.transform.rotation = rpmCamera.part.transform.rotation;
            cam.transform.Rotate(rpmCamera.rotateCamera);
            //cam.transform.position += rpmCamera.translateCamera;
        }
    }
}
