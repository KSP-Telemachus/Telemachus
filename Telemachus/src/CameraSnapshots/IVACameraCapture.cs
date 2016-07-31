using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Telemachus.CameraSnapshots
{
    public class IVACameraCapture : MonoBehaviour
    {
        public RenderTexture overviewTexture;
        public bool didRender;
        public byte[] imageBytes = null;
        public bool mutex = false;

        protected Dictionary<string, Camera> cameraDuplicates = new Dictionary<string, Camera>();
        protected List<string> activeCameras;
        protected static readonly string[] skippedCameras = { "UIMainCamera", "UIVectorCamera" };
        protected static string cameraContainerNamePrefix = "Telemachus Camera Container - ";

        protected float fovAngle = 60f;

        private const float defaultFovAngle = 60f;
        private const float aspect = 1.0f;
        private  int camerares = 300;

        void LateUpdate()
        {
            PluginLogger.debug("LATEUPDATE FOR CAMERA");
            if(InternalCamera.Instance != null)
            {
                PluginLogger.debug("INTERNAL CAMERA Position: " + InternalCamera.Instance.transform.position.ToString());
                PluginLogger.debug("INTERNAL CAMERA Rotation: " + InternalCamera.Instance.transform.rotation.ToString());
                
                if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA)
                {
                    foreach(Camera camera in Camera.allCameras)
                    {
                        if(camera.name == "InternalCamera")
                        {
                            fovAngle = camera.fieldOfView;
                        }
                    }
                }
                else
                {
                    fovAngle = defaultFovAngle;
                }

                PluginLogger.debug("INTERNAL CAMERA Zoom: " + fovAngle);
            }
            else
            {
                PluginLogger.debug("NO INTERNAL CAMERA");
            }
            
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
            PluginLogger.debug("CAMERA: " + cam.name + " ; NEAR CLIP PLANE: "+ cam.nearClipPlane +"; FAR CLIP PLANE: " + cam.farClipPlane + " FOV : " + cam.fieldOfView + " POSITION: " +  cam.transform.position.ToString() + " ROTATION: " + cam.transform.rotation.ToString());
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

            List<Camera> renderingCameras = new List<Camera>();
            foreach (string cameraName in activeCameras)
            {
                PluginLogger.debug("GETTING CAMERA" + cameraName);
                renderingCameras.Add(cameraDuplicates[cameraName]);
            }

            this.imageBytes = SnapshotRenderer.renderSnaphot(renderingCameras, camerares, camerares);
            this.didRender = true;
            mutex = false;
        }

    }
}