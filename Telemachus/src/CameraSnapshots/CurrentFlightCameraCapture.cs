using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Telemachus.CameraSnapshots
{
    public class CurrentFlightCameraCapture : CameraCapture
    {
        protected new List<string> activeCameras = new List<string>();
        private readonly string[] knownCameraNames =
        {
            "GalaxyCamera",
            "Camera ScaledSpace",
            "Camera VE Underlay", // Environmental Visual Enhancements plugin camera
            "Camera VE Overlay",  // Environmental Visual Enhancements plugin camera
            "Camera 01",
            "Camera 00",
            "InternalCamera",
            "FXCamera"
        };

        public Dictionary<string, Camera> gameCameraMapping = new Dictionary<string, Camera>();

        public override string cameraManagerName()
        {
            return "TelemachusFlightCamera";
        }

        public override string cameraType()
        {
            return "FlightCamera";
        }

        protected override void LateUpdate()
        {
            //PluginLogger.debug("LATE UPDATE FOR FLIGHT CAMERA");
            if (CameraManager.Instance != null && HighLogic.LoadedSceneIsFlight)
            {
                duplicateAnyNewCameras();
                repositionCamera();
                StartCoroutine(newRender());
            }

            //base.LateUpdate();
        }

        public IEnumerator newRender()
        {
            yield return true;
            //PluginLogger.debug(cameraManagerName() +  ": WAITING FOR END OF FRAME");
            //yield return new WaitForEndOfFrame();
            //PluginLogger.debug(cameraManagerName() + ": OUT OF FRAME");
            Texture2D texture = getTexture2DFromRenderTexture();
            this.imageBytes = texture.EncodeToJPG();
            this.didRender = true;
        }

        public Texture2D getTexture2DFromRenderTexture()
        {
            Texture2D texture2D = new Texture2D(overviewTexture.width, overviewTexture.height);
            RenderTexture.active = overviewTexture;
            texture2D.ReadPixels(new Rect(0, 0, overviewTexture.width, overviewTexture.height), 0, 0);
            texture2D.Apply();
            return texture2D;
        }

        public override void BeforeRenderNewScreenshot()
        {
            //UpdateCameras();
            //base.BeforeRenderNewScreenshot();
        }

        public void duplicateAnyNewCameras()
        {
            //PluginLogger.debug("BUILDING DUPLICATE CAMERAS");

            if(overviewTexture == null)
            {
                overviewTexture = new RenderTexture(cameraResolution, cameraResolution, 24);
            }

            List<string> currentlyActiveCameras = new List<string>();

            foreach (Camera camera in Camera.allCameras)
            {
                //PluginLogger.debug("CAMERA:" + camera.name + " TIMESTAMP: " + DateTime.UtcNow.ToShortTimeString());
                // debugCameraDetails(camera);
                // Don't duplicate any cameras we're going to skip
                if (skippedCameras.IndexOf(camera.name) != -1)
                {
                    continue;
                }

                // Don't duplicate cameras we don't know about
                if(knownCameraNames.IndexOf(camera.name) == -1)
                {
                    continue;
                }

                if (!cameraDuplicates.ContainsKey(camera.name))
                {
                    var cameraDuplicateGameObject = new GameObject(cameraContainerNamePrefix + camera.name);
                    Camera cameraDuplicate = cameraDuplicateGameObject.AddComponent<Camera>();
                    cameraDuplicates[camera.name] = cameraDuplicate;
                    cameraDuplicate.CopyFrom(camera);
                    cameraDuplicate.fieldOfView = fovAngle;
                    cameraDuplicate.aspect = aspect;
                    cameraDuplicate.targetTexture = this.overviewTexture;

                    if (camera.name == "Camera 00" || camera.name == "FXCamera")
                    {
                        cameraDuplicate.nearClipPlane = cameraDuplicate.farClipPlane / 8192.0f;
                    }

                    //Now that the camera has been duplicated, add it to the list of active cameras
                    activeCameras.Add(camera.name);

                    if (!gameCameraMapping.ContainsKey(camera.name))
                    {
                        gameCameraMapping[camera.name] = camera;
                    }
                }

                //Mark that the camera is currently active
                currentlyActiveCameras.Add(camera.name);
            }

            IEnumerable<string> disabledCameras = activeCameras.Except(currentlyActiveCameras);
            foreach(string disabledCamera in disabledCameras)
            {
                if (cameraDuplicates.ContainsKey(disabledCamera))
                {
                    Destroy(cameraDuplicates[disabledCamera]);
                    cameraDuplicates.Remove(disabledCamera);
                    activeCameras.Remove(disabledCamera);
                }
            }
        }


        public void repositionCamera()
        {
            foreach (KeyValuePair<string, Camera> KVP in cameraDuplicates)
            {
                Camera cameraDuplicate = KVP.Value;
                Camera gameCamera = gameCameraMapping[KVP.Key];

                cameraDuplicate.transform.position = gameCamera.transform.position;
                cameraDuplicate.transform.rotation = gameCamera.transform.rotation;

                additionalCameraUpdates(cameraDuplicate);
            }
        }

        public void verboseCameraDebug(Camera camera)
        {
            string[] debugProperties = {
                        "CAMERA INFO: " + camera.name,
                        "TARGET DISPLAY: " + camera.targetDisplay,
                        "TARGET TEXTURE: " + camera.targetTexture,
                        "RENDERING PATH: " + camera.renderingPath,
                        "ACTUAL RENDER PATH: " + camera.actualRenderingPath,
                        "CAMERA TYPE: " + camera.cameraType,
                        "GAME OBJECT: " + camera.gameObject
                    };
            PluginLogger.debug(String.Join("\n", debugProperties));
        }
    }
}