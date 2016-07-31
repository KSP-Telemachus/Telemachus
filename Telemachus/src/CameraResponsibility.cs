//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Reflection;
using WebSocketSharp.Net;
using WebSocketSharp;
using UnityEngine;
using System.Collections;

namespace Telemachus
{
    public class CameraResponsibility : IHTTPRequestResponder
    {
        /// The page prefix that this class handles
        public const String PAGE_PREFIX = "/telemachus/cameras";
        /// The KSP API to use to access variable data
        private IKSPAPI kspAPI = null;

        private UpLinkDownLinkRate dataRates = null;

        private CameraCapture cameraCaptureTest = null;

        #region Initialisation

        public CameraResponsibility(IKSPAPI kspAPI, UpLinkDownLinkRate rateTracker)
        {
            this.kspAPI = kspAPI;
            dataRates = rateTracker;
            this.setCameraCapture();
        }

        public void setCameraCapture()
        {
            PluginLogger.debug("START CAMERA CATPURE");
            GameObject obj = new GameObject("CameraCapture", typeof(CameraCapture));
            this.cameraCaptureTest = (CameraCapture)obj.GetComponent(typeof(CameraCapture));
            PluginLogger.debug("CAM CAMPTURE CREATED");
        }

        #endregion

        private static Dictionary<string,object> splitArguments(string argstring)
        {
            var ret = new Dictionary<string, object>();
            if (argstring.StartsWith("?")) argstring = argstring.Substring(1);

            foreach (var part in argstring.Split('&'))
            {
                var subParts = part.Split('=');
                if (subParts.Length != 2) continue;
                var keyName = UnityEngine.WWW.UnEscapeURL(subParts[0]);
                var apiName = UnityEngine.WWW.UnEscapeURL(subParts[1]);
                ret[keyName] = apiName;
            }
            return ret;
        }

        private static IDictionary<string, object> parseJSONBody(string jsonBody)
        {
            return (IDictionary<string, object>)SimpleJson.SimpleJson.DeserializeObject(jsonBody);
        }

        public bool process(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (!request.RawUrl.StartsWith(PAGE_PREFIX)) return false;

            // Work out how big this request was
            long byteCount = request.RawUrl.Length + request.ContentLength64;
            // Don't count headers + request.Headers.AllKeys.Sum(x => x.Length + request.Headers[x].Length + 1);
            dataRates.RecieveDataFromClient(Convert.ToInt32(byteCount));

            /*
            IDictionary<string, object> apiRequests;
            if (request.HttpMethod.ToUpper() == "POST" && request.HasEntityBody)
            {
                System.IO.StreamReader streamReader = new System.IO.StreamReader(request.InputStream);
                apiRequests = parseJSONBody(streamReader.ReadToEnd());
            }
            else
            {
                apiRequests = splitArguments(request.Url.Query);
            }

            var results = new Dictionary<string, object>();
            var unknowns = new List<string>();
            var errors = new Dictionary<string, string>();
            
            foreach (var name in apiRequests.Keys)
            {
                try {
                    results[name] = kspAPI.ProcessAPIString(apiRequests[name].ToString());
                } catch (IKSPAPI.UnknownAPIException)
                {
                    unknowns.Add(apiRequests[name].ToString());
                } catch (Exception ex)
                {
                    errors[apiRequests[name].ToString()] = ex.ToString();
                }
            }
            // If we had any unrecognised API keys, let the user know
            if (unknowns.Count > 0) results["unknown"] = unknowns;
            if (errors.Count > 0)   results["errors"]  = errors;
            */

            /*
            
            Camera sourceNearCam = null;
            foreach (Camera cam in Camera.allCameras)
            {
                if (cam.name == "Camera 00") { sourceNearCam = cam; }
            }*/
            /*
            try
            {

                PluginLogger.debug("FINDING CAMERA");
                Camera sourceNearCam = null;

                foreach (Camera cam in Camera.allCameras)
                {
                    if (cam.name == "Camera 00") { sourceNearCam = cam; }
                }

                PluginLogger.debug("CONSTANTS");
                Camera NearCamera;
                float fovAngle = 60f;
                float aspect = 1.0f;
                int camerares = 512;
                Vector3 rotateConstant = new Vector3(-90, 0, 0);

                PluginLogger.debug("BUILD GATU CMERA");
                var NearCameraGameObject = new GameObject("GATU Camera 00");
                NearCamera = NearCameraGameObject.AddComponent<Camera>();
                NearCamera.CopyFrom(sourceNearCam);
                NearCamera.enabled = false;
                NearCamera.fieldOfView = fovAngle;
                NearCamera.aspect = aspect;

                PluginLogger.debug("BUILD CAMERA TEXTURE");
                RenderTexture rt = new RenderTexture(camerares, camerares, 24);
                rt.Create();

                NearCamera.targetTexture = rt;

                //NearCamera.transform.rotation = this.part.gameObject.transform.rotation;
                //NearCamera.transform.Rotate(rotateConstant);
                //NearCamera.transform.position = this.part.gameObject.transform.position;

                PluginLogger.debug("RENDER");
                NearCamera.Render();

                PluginLogger.debug("BUILD SCREENSHOT");
                Texture2D screenShot = new Texture2D(camerares, camerares, TextureFormat.RGB24, false);
                RenderTexture backupRenderTexture = RenderTexture.active;
                RenderTexture.active = rt;
                PluginLogger.debug("READ PIXELS");
                screenShot.ReadPixels(new Rect(0, 0, camerares, camerares), 0, 0);

                PluginLogger.debug("ENCODE");
                byte[] bytes = screenShot.EncodeToPNG();

                PluginLogger.debug("TRANSMIT");
                // Now, serialize the dictionary and write to the response
                //var returnData = Encoding.UTF8.GetBytes(bytes);
                response.ContentEncoding = Encoding.UTF8;
                response.ContentType = "image/png";
                response.WriteContent(bytes);
                dataRates.SendDataToClient(bytes.Length);

                PartModule.Destroy(rt);
                PartModule.Destroy(screenShot);
            } catch(Exception e)
            {
                PluginLogger.debug("BOOM");
                PluginLogger.debug(e.ToString());
            }
            return true;
            */

            if (GameObject.Find("CameraCapture") == null)
            {
                PluginLogger.debug("REBUILDING CAMERA CAPTURE");
                this.setCameraCapture();
            }


            if (this.cameraCaptureTest.didRender)
            {
                response.ContentEncoding = Encoding.UTF8;
                response.ContentType = "image/png";
                response.WriteContent(this.cameraCaptureTest.imageBytes);
                dataRates.SendDataToClient(this.cameraCaptureTest.imageBytes.Length);
            }
            else
            {
                string NAString = "NA";
                response.ContentEncoding = Encoding.UTF8;
                response.ContentType = "text/plain";
                response.WriteContent(Encoding.UTF8.GetBytes(NAString));
                dataRates.SendDataToClient(Encoding.UTF8.GetByteCount(NAString));
            }
            return true;
        }
    }
}
