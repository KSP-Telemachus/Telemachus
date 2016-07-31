using UnityEngine;
using System.Collections;
using System;

namespace Telemachus
{
    public class CameraCapture : MonoBehaviour
    {
        public RenderTexture overviewTexture;
        Camera OVcamera = null;
        public string cameraName = "";
        public bool didRender;
        public byte[] imageBytes = null;
        public bool mutex = false;

        private Camera NearCamera;
        private Camera FarCamera;
        private Camera SkyboxCamera;
        private Camera GalaxyCamera;
        private const float fovAngle = 60f;
        private const float aspect = 1.0f;
        private  int camerares = 256;

        void Start()
        {
            PluginLogger.debug("CAMERA CAPTURE STARTED");
        }

        void OnDisable()
        {
            PluginLogger.debug("WHY DISABLED?");
        }

        void OnDestroy()
        {
            PluginLogger.debug("WHY DESTROY?");
        }

        void Update()
        {
            PluginLogger.debug("UPDATE CAMERA");
        }

        void LateUpdate()
        {
            PluginLogger.debug("LATEUPDATE FOR CAMERA");
            if (OVcamera == null)
            {
                PluginLogger.debug("NO CAMERA YET, FINDING: " + this.cameraName);
                if(FlightCamera.fetch != null)
                {
                    //OVcamera = FlightCamera.fetch.mainCamera;
                }

                
                foreach (Camera cam in Camera.allCameras)
                {
                    if (cam.name == this.cameraName) { OVcamera = cam; }
                }
            }

            if (OVcamera != null)
            {
                PluginLogger.debug("CAMERA FOUND, TAKING SCREENSHOT");
                StartCoroutine(NewScreenshot());
            }
        }

        public void UpdateCameras()
        {
            if (NearCamera == null)
            {
                var NearCameraGameObject = new GameObject("GATU Camera 00");
                NearCamera = NearCameraGameObject.AddComponent<Camera>();
            }

            if (FarCamera == null)
            {
                var FarCameraGameObject = new GameObject("GATU Camera 01");
                FarCamera = FarCameraGameObject.AddComponent<Camera>();
            }

            if (SkyboxCamera == null)
            {
                var SkyboxCameraGameObject = new GameObject("GATU Camera ScaledSpace");
                SkyboxCamera = SkyboxCameraGameObject.AddComponent<Camera>();
            }

            if (GalaxyCamera == null)
            {
                var GalaxyCameraGameObject = new GameObject("GATU GalaxyCamera");
                GalaxyCamera = GalaxyCameraGameObject.AddComponent<Camera>();
            }

            Camera sourceNearCam = null;
            Camera sourceFarCam = null;
            Camera sourceSkyCam = null;
            Camera sourceGalaxyCam = null;

            if (CameraManager.Instance != null)
            {
                PluginLogger.debug("CURRENT CAMERA MODE: " + CameraManager.Instance.currentCameraMode);
            }
            foreach (Camera cam in Camera.allCameras)
            {
                debugCameraDetails(cam);
                if (cam.name == "Camera 00") { sourceNearCam = cam; }
                else if (cam.name == "Camera 01") { sourceFarCam = cam; }
                else if (cam.name == "Camera ScaledSpace") { sourceSkyCam = cam; }
                else if (cam.name == "GalaxyCamera") { sourceGalaxyCam = cam; }
            }

            NearCamera.CopyFrom(sourceNearCam);
            NearCamera.enabled = false;
            NearCamera.fieldOfView = fovAngle;
            NearCamera.aspect = aspect;

            FarCamera.CopyFrom(sourceFarCam);
            FarCamera.enabled = false;
            FarCamera.fieldOfView = fovAngle;
            FarCamera.aspect = aspect;

            SkyboxCamera.transform.position = sourceSkyCam.transform.position;
            SkyboxCamera.CopyFrom(sourceSkyCam);
            SkyboxCamera.enabled = false;
            SkyboxCamera.fieldOfView = fovAngle;
            SkyboxCamera.aspect = aspect;

            GalaxyCamera.transform.position = sourceGalaxyCam.transform.position;
            GalaxyCamera.CopyFrom(sourceGalaxyCam);
            GalaxyCamera.enabled = false;
            GalaxyCamera.fieldOfView = fovAngle;
            GalaxyCamera.aspect = aspect;
        }

        public void debugCameraDetails(Camera cam)
        {
            PluginLogger.debug("CAMERA: " + cam.name + " ; FAR: " + cam.far + "; FAR CLIP PLANE: " + cam.farClipPlane);
        }
        
        public IEnumerator NewScreenshot()
        {
            if (mutex)
            {
                yield return true;
            }
            PluginLogger.debug("BYPASSED MUTEX");
            mutex = true;
            PluginLogger.debug("WAITING FOR END OF FRAME");
            yield return new WaitForEndOfFrame();

            UpdateCameras();

            RenderTexture rt = new RenderTexture(camerares, camerares, 24);

            NearCamera.targetTexture = rt;
            FarCamera.targetTexture = rt;
            SkyboxCamera.targetTexture = rt;
            GalaxyCamera.targetTexture = rt;

            GalaxyCamera.Render();
            SkyboxCamera.Render();
            FarCamera.Render();
            NearCamera.Render();

            Texture2D screenShot = new Texture2D(camerares, camerares, TextureFormat.RGB24, false);
            RenderTexture backupRenderTexture = RenderTexture.active;
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, camerares, camerares), 0, 0);

            NearCamera.targetTexture = null;
            FarCamera.targetTexture = null;
            SkyboxCamera.targetTexture = null;
            GalaxyCamera.targetTexture = null;

            RenderTexture.active = backupRenderTexture;


            this.imageBytes = screenShot.EncodeToPNG();
            this.didRender = true;
            mutex = false;
            Destroy(screenShot);
            Destroy(rt);
        }

    }
}