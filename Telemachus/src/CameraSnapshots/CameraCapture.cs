using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Telemachus.CameraSnapshots
{
    public class CameraCapture : MonoBehaviour
    {
        public RenderTexture overviewTexture;
        public bool didRender;
        public byte[] imageBytes = null;
        public bool mutex = false;

        public virtual string cameraManagerName()
        {
            return "NA";
        }

        public virtual string cameraType()
        {
            return "NA";
        }

        protected Dictionary<string, Camera> cameraDuplicates = new Dictionary<string, Camera>();
        protected List<string> activeCameras;
        protected static readonly string[] skippedCameras = { "UIMainCamera", "UIVectorCamera", "velocity camera" };
        protected string cameraContainerNamePrefix {
            get
            {
                return "TelemachusCameraContainer:" + cameraManagerName();
            }
        }
        
        protected const float fovAngle = 60f;
        protected const float aspect = 1.0f;
        public  int cameraResolution = 300;

        protected virtual void LateUpdate()
        {
            //PluginLogger.debug("LATEUPDATE FOR CAMERA");
            if(CameraManager.Instance != null && HighLogic.LoadedSceneIsFlight)
            {
                //PluginLogger.debug("CAMERA FOUND, TAKING SCREENSHOT");
                StartCoroutine(NewScreenshot());
            }
        }

        public void UpdateCameras()
        {
            if (CameraManager.Instance != null)
            {
                //PluginLogger.debug("CURRENT CAMERA MODE: " + CameraManager.Instance.currentCameraMode);
            }

            activeCameras = new List<string>();

            foreach (Camera camera in Camera.allCameras)
            {
                // debugCameraDetails(camera);
                // Don't duplicate any cameras we're going to skip
                if (skippedCameras.IndexOf(camera.name) != -1)
                {
                    continue;
                }

                Camera cameraDuplicate;

                if (!cameraDuplicates.ContainsKey(camera.name))
                {
                    var cameraDuplicateGameObject = new GameObject(cameraContainerNamePrefix + camera.name);
                    cameraDuplicate = cameraDuplicateGameObject.AddComponent<Camera>();
                    cameraDuplicates[camera.name] = cameraDuplicate;
                }
                else
                {
                    cameraDuplicate = cameraDuplicates[camera.name];
                }

                cameraDuplicate.CopyFrom(camera);
                cameraDuplicate.enabled = false;
                cameraDuplicate.fieldOfView = fovAngle;
                cameraDuplicate.aspect = aspect;

                if (camera.name == "Camera 00" || camera.name == "FXCamera")
                {
                    //PluginLogger.debug("ADJUSTING NEAR CLIPPING PLANE FOR: " + camera.name + " : " + cameraDuplicate.farClipPlane / 8192.0f);
                    cameraDuplicate.nearClipPlane = cameraDuplicate.farClipPlane / 8192.0f;
                }

                additionalCameraUpdates(cameraDuplicate);

                //Now that the camera has been duplicated, add it to the list of active cameras
                activeCameras.Add(camera.name);
            }
        }

        public virtual void additionalCameraUpdates(Camera cam){ }

        public virtual void debugCameraDetails(Camera cam)
        {
            PluginLogger.debug("CAMERA: " + cam.name + " POS: " + cam.transform.position + "; ROT: " + cam.transform.rotation  + " ; NEAR:" + cam.nearClipPlane + "; FAR: " + cam.farClipPlane);
        }

        public virtual void BeforeRenderNewScreenshot(){ }
        
        public IEnumerator NewScreenshot()
        {
            if (mutex)
            {
                yield return true;
            }
            //PluginLogger.debug("BYPASSED MUTEX");
            mutex = true;
            //PluginLogger.debug("WAITING FOR END OF FRAME");
            yield return new WaitForEndOfFrame();

            BeforeRenderNewScreenshot();

            List<Camera> renderingCameras = new List<Camera>();
            foreach (string cameraName in activeCameras)
            {
                //PluginLogger.debug("[" + cameraManagerName() + "] GETTING CAMERA " + cameraName);
                renderingCameras.Add(cameraDuplicates[cameraName]);
            }

            this.imageBytes = SnapshotRenderer.renderSnaphot(renderingCameras, cameraResolution, cameraResolution);
            this.didRender = true;
            mutex = false;
        }

    }
}