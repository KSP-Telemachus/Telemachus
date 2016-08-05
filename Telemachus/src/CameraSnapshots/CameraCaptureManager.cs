using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Telemachus.CameraSnapshots
{
    class CameraCaptureManager : MonoBehaviour
    {
        #region Singleton management
        public static GameObject instance;

        private CameraCaptureManager() { }

        public static GameObject Instance
        {
            get
            {
                if (CameraCaptureManager.instance == null)
                {
                    instance = GameObject.Find("CameraCaptureManager")
                        ?? new GameObject("CameraCaptureManager", typeof(CameraCaptureManager));
                }
                
                return instance;
            }
        }

        public static CameraCaptureManager classedInstance
        {
            get {
                return (CameraCaptureManager)Instance.GetComponent(typeof(CameraCaptureManager));
            }
        }
        #endregion

        public Dictionary<string, CameraCapture> cameras = new Dictionary<string, CameraCapture>();

        public void addCamera(RasterPropMonitorCamera camera)
        {
            if(camera == null)
            {
                return;
            }

            GameObject container = new GameObject("RasterPropMonitorCameraCapture:" + camera.cameraName, typeof(RasterPropMonitorCameraCapture));
            RasterPropMonitorCameraCapture cameraCapture = (RasterPropMonitorCameraCapture)container.GetComponent(typeof(RasterPropMonitorCameraCapture));
            cameraCapture.rpmCamera = camera;

            cameras[cameraCapture.cameraManagerName()] = cameraCapture;
        }

        public void addCameraCapture(CameraCapture cameraCapture)
        {
            cameras[cameraCapture.cameraManagerName()] = cameraCapture;
        }

        public void removeCamera(string name)
        {
            cameras.Remove(name);
        }
    }
}
