using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Telemachus.CameraSnapshots
{
    class RasterPropMonitorCameraCapture : CameraCapture
    {
        public RasterPropMonitorCamera rpmCamera;
        protected static string cameraManagerNamePrefix = "RPMCamera-";
        protected static readonly string[] camerasToSkipPositionTransform = { "GalaxyCamera", "Camera ScaledSpace", "Camera VE Underlay" };
        protected Regex _cameraSkipRegex;
        protected Regex cameraSkipRegex
        {
            get
            {
                if(_cameraSkipRegex == null)
                {
                    _cameraSkipRegex = new Regex("(" + String.Join("|", camerasToSkipPositionTransform) + ")$");
                }

                return _cameraSkipRegex;
            }
        }

        public override string cameraManagerName()
        {
            return buildCameraManagerName(rpmCamera.cameraName);
        }

        public override string cameraType()
        {
            return "RasterPropMonitor";
        }

        protected bool builtCameraDuplicates = false;

        public static string buildCameraManagerName(string name)
        {
            return cameraManagerNamePrefix + name;
        }

        public override void additionalCameraUpdates(Camera dupliateCam, Camera gameCamera)
        {
            base.additionalCameraUpdates(dupliateCam, gameCamera);
            if (!cameraSkipRegex.IsMatch(gameCamera.name))
            {
                dupliateCam.transform.position = rpmCamera.part.transform.position;
            }

            // Just in case to support JSITransparentPod.
            //cam.cullingMask &= ~(1 << 16 | 1 << 20);

            dupliateCam.transform.rotation = rpmCamera.part.transform.rotation;
            dupliateCam.transform.Rotate(rpmCamera.rotateCamera);
            dupliateCam.transform.position += rpmCamera.translateCamera;
        }

        /*public override void additionalCameraUpdates(Camera cam)
        {
            if (!cameraSkipRegex.IsMatch(cam.name))
            {
                cam.transform.position = rpmCamera.part.transform.position;
            }

            // Just in case to support JSITransparentPod.
            //cam.cullingMask &= ~(1 << 16 | 1 << 20);

            cam.transform.rotation = rpmCamera.part.transform.rotation;
            cam.transform.Rotate(rpmCamera.rotateCamera);
            cam.transform.position += rpmCamera.translateCamera;
            
            base.additionalCameraUpdates(cam);
        }*/
    }
}
