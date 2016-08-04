using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Telemachus.CameraSnapshots
{
    class RasterPropMonitorCameraCapture : MonoBehaviour
    {
        public RasterPropMonitorCamera rpmCamera;

        void LateUpdate()
        {
            PluginLogger.debug("LATEUPDATE FOR RPM Camera");
            if(rpmCamera != null)
            {
                PluginLogger.debug("LATEUPDATE FOR RPM CAMERA:" + rpmCamera.cameraName);
            }

            /*if (CameraManager.Instance != null)
            {
                PluginLogger.debug("CAMERA FOUND, TAKING SCREENSHOT");
                StartCoroutine(NewScreenshot());
            }*/
        }
    }
}
