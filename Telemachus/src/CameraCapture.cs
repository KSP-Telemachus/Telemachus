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
                StartCoroutine(TakeScreenShot());
            }
        }

        public void debugCameraDetails(Camera cam)
        {
            PluginLogger.debug("CAMERA: " + cam.name + " ; FAR: " + cam.far + "; FAR CLIP PLANE: " + cam.farClipPlane);
        }
        
        public IEnumerator TakeScreenShot()
        {
            if (mutex)
            {
                yield return true;
            }
            PluginLogger.debug("BYPASSED MUTEX");
            mutex = true;
            PluginLogger.debug("WAITING FOR END OF FRAME");
            yield return new WaitForEndOfFrame();

            PluginLogger.debug("GETTING CAMERA");
            Camera camOV = OVcamera;

            PluginLogger.debug("Swapping Textures");
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture currentCameraTexture = camOV.targetTexture;

            RenderTexture rt = new RenderTexture(256, 256, 24);
            rt.Create();

            float oldAspect = camOV.aspect;

            camOV.aspect = 1.0f;

            camOV.targetTexture = rt;

            foreach (Camera cam in Camera.allCameras)
            {
                debugCameraDetails(cam);
            }

            PluginLogger.debug("MYCAM BELOW");
            debugCameraDetails(camOV);

            RenderTexture.active = camOV.targetTexture;
            PluginLogger.debug("RENDERING");
            camOV.Render();

            PluginLogger.debug("READ APPLY 1");
            Texture2D imageOverview = new Texture2D(camOV.targetTexture.width, camOV.targetTexture.height, TextureFormat.RGB24, false);
            PluginLogger.debug("READ APPLY 2");
            imageOverview.ReadPixels(new Rect(0, 0, camOV.targetTexture.width, camOV.targetTexture.height), 0, 0);
            PluginLogger.debug("READ APPLY 3");
            imageOverview.Apply();

            PluginLogger.debug("RETURNING TEXTURE");
            RenderTexture.active = currentRT;
            camOV.targetTexture = currentCameraTexture;
            camOV.aspect = oldAspect;

            PluginLogger.debug("ENCODING TO PNG");
            // Encode texture into PNG
            this.imageBytes = imageOverview.EncodeToPNG();
            this.didRender = true;
            mutex = false;

            Destroy(imageOverview);
            Destroy(rt);
        }
    }
}