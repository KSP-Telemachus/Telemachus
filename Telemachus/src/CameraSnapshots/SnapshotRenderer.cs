using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;

namespace Telemachus.CameraSnapshots
{
    class SnapshotRenderer : MonoBehaviour
    {
        public static byte[] renderSnaphot(List<Camera> cameras, int width, int height)
        {
            PluginLogger.debug("RENDERING SNAPSHOT");
            /*RenderTexture rt = new RenderTexture(width, height, 24);

            foreach(Camera camera in cameras) {
                camera.targetTexture = rt;
                //camera.Render();
            } */

            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            //RenderTexture backupRenderTexture = RenderTexture.active;
            //RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);

            foreach (Camera camera in cameras)
            {
                camera.targetTexture = null;
            }

            //RenderTexture.active = backupRenderTexture;

            byte[] result = screenShot.EncodeToJPG();
            Destroy(screenShot);
            //Destroy(rt);

            return result;
        }
    }
}
