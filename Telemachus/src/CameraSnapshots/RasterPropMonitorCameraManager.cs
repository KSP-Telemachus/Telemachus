using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Telemachus.CameraSnapshots
{
    class RasterPropMonitorCameraManager : MonoBehaviour
    {
        #region Singleton management
        public static GameObject instance;

        private RasterPropMonitorCameraManager() { }

        public static GameObject Instance
        {
            get
            {
                if (RasterPropMonitorCameraManager.instance == null)
                {
                    instance = GameObject.Find("RasterPropMonitorCameraManager")
                        ?? new GameObject("RasterPropMonitorCameraManager", typeof(RasterPropMonitorCameraManager));
                }
                
                return instance;
            }
        }
        #endregion

        public Dictionary<string, RasterPropMonitorCameraCapture> cameras = new Dictionary<string, RasterPropMonitorCameraCapture>();

        public void addCamera(RasterPropMonitorCamera camera)
        {
            if(camera == null)
            {
                return;
            }

            GameObject container = new GameObject("RasterPropMonitorCameraCapture:" + camera.cameraName, typeof(RasterPropMonitorCameraCapture));
            cameras[camera.cameraName] = (RasterPropMonitorCameraCapture)container.GetComponent(typeof(RasterPropMonitorCameraCapture));
            cameras[camera.cameraName].rpmCamera = camera;
        }
    }
}
