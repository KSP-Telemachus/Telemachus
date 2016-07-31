using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Telemachus
{
    public class CameraCapture : MonoBehaviour
    {
        public RenderTexture overviewTexture;
        public bool didRender;
        public byte[] imageBytes = null;
        public bool mutex = false;

        protected Dictionary<string, Camera> cameraDuplicates = new Dictionary<string, Camera>();
        protected List<string> activeCameras;
        protected static readonly string[] skippedCameras = { "UIMainCamera" };
        protected static string cameraContainerNamePrefix = "Telemachus Camera Container - ";
        
        private const float fovAngle = 60f;
        private const float aspect = 1.0f;
        private  int camerares = 300;

        void LateUpdate()
        {
            PluginLogger.debug("LATEUPDATE FOR CAMERA");
            if(CameraManager.Instance != null)
            {
                PluginLogger.debug("CAMERA FOUND, TAKING SCREENSHOT");
                StartCoroutine(NewScreenshot());
            }
        }

        public void UpdateCameras()
        {
            if (CameraManager.Instance != null)
            {
                PluginLogger.debug("CURRENT CAMERA MODE: " + CameraManager.Instance.currentCameraMode);
            }

            activeCameras = new List<string>();

            foreach (Camera camera in Camera.allCameras)
            {
                debugCameraDetails(camera);
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

                //Now that the camera has been duplicated, add it to the list of active cameras
                activeCameras.Add(camera.name);
            }
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

            foreach(string cameraName in activeCameras)
            {
                PluginLogger.debug("GETTING CAMERA" + cameraName);
                Camera activeCamera = cameraDuplicates[cameraName];
                activeCamera.targetTexture = rt;
            }

            foreach (string cameraName in activeCameras)
            {
                Camera activeCamera = cameraDuplicates[cameraName];
                activeCamera.Render();
            }

            Texture2D screenShot = new Texture2D(camerares, camerares, TextureFormat.RGB24, false);
            RenderTexture backupRenderTexture = RenderTexture.active;
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, camerares, camerares), 0, 0);

            foreach (string cameraName in activeCameras)
            {
                Camera activeCamera = cameraDuplicates[cameraName];
                activeCamera.targetTexture = null;
            }

            RenderTexture.active = backupRenderTexture;


            this.imageBytes = screenShot.EncodeToPNG();
            this.didRender = true;
            mutex = false;
            Destroy(screenShot);
            Destroy(rt);
        }

    }
}