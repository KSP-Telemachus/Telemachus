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
using Telemachus.CameraSnapshots;
using System.Text.RegularExpressions;

namespace Telemachus
{
    public class CameraResponsibility : IHTTPRequestResponder
    {
        /// The page prefix that this class handles
        public const String PAGE_PREFIX = "/telemachus/cameras";
        public const String CAMERA_LIST_ENDPOINT = PAGE_PREFIX;
        public const String NGROK_ORIGINAL_HOST_HEADER = "X-Original-Host";
        protected Regex _cameraNameEndpointRegex;
        protected Regex cameraNameEndpointRegex
        {
            get
            {
                if (_cameraNameEndpointRegex == null)
                {
                    _cameraNameEndpointRegex = new Regex(Regex.Escape(PAGE_PREFIX) + "\\/(.+)");
                }

                return _cameraNameEndpointRegex;
            }
        }

        /// The KSP API to use to access variable data
        private IKSPAPI kspAPI = null;

        private UpLinkDownLinkRate dataRates = null;

        

        #region Initialisation

        public CameraResponsibility(IKSPAPI kspAPI, UpLinkDownLinkRate rateTracker)
        {
            this.kspAPI = kspAPI;
            dataRates = rateTracker;
        }

        public void setCameraCapture()
        {
            //PluginLogger.debug("START CAMERA CATPURE");
            
            //PluginLogger.debug("CAM CAMPTURE CREATED");
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

        public string cameraURL(HttpListenerRequest request, CameraCapture camera)
        {
            String hostname = "";
            if (request.Headers.Contains(NGROK_ORIGINAL_HOST_HEADER))
            {
                hostname = request.Headers[NGROK_ORIGINAL_HOST_HEADER];
            }
            else
            {
                hostname = request.UserHostName;
            }

            return request.Url.Scheme + "://" + hostname + PAGE_PREFIX + "/" + UnityEngine.WWW.EscapeURL(camera.cameraManagerName());
        }

        public bool processCameraManagerIndex(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (GameObject.Find("CurrentFlightCameraCapture") == null)
            {
                //PluginLogger.debug("REBUILDING CAMERA CAPTURE");
                this.setCameraCapture();
            }

            var jsonObject = new List<Dictionary<string, object>>();

            foreach(KeyValuePair<string, CameraCapture> cameraKVP in CameraCaptureManager.classedInstance.cameras)
            {
                var jsonData = new Dictionary<string, object>();
                jsonData["name"] = cameraKVP.Value.cameraManagerName();
                jsonData["type"] = cameraKVP.Value.cameraType();
                jsonData["url"] = cameraURL(request, cameraKVP.Value);

                jsonObject.Add(jsonData);
            }

            byte[] jsonBytes = Encoding.UTF8.GetBytes(SimpleJson.SimpleJson.SerializeObject(jsonObject));

            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = "application/json";
            response.WriteContent(jsonBytes);
            dataRates.SendDataToClient(jsonBytes.Length);

            return true;
        }

        public bool processCameraImageRequest(string cameraName, HttpListenerRequest request, HttpListenerResponse response)
        {
            cameraName = cameraName.ToLower();
            if (!CameraCaptureManager.classedInstance.cameras.ContainsKey(cameraName))
            {
                response.StatusCode = 404;
                return true;
            }

            CameraCapture camera = CameraCaptureManager.classedInstance.cameras[cameraName];
            //PluginLogger.debug("RENDERING SAVED CAMERA: "+ camera.cameraManagerName());
            if (camera.didRender)
            {
                response.ContentEncoding = Encoding.UTF8;
                response.ContentType = "image/jpeg";
                response.WriteContent(camera.imageBytes);
                dataRates.SendDataToClient(camera.imageBytes.Length);
            }
            else
            {
                response.StatusCode = 503;
            }

            return true;
        }

        public bool process(HttpListenerRequest request, HttpListenerResponse response)
        {
            //PluginLogger.debug(request.Url.AbsolutePath.TrimEnd('/'));
            //PluginLogger.debug(String.Join(",", CameraCaptureManager.classedInstance.cameras.Keys.ToArray()));
            //PluginLogger.debug("FLIGHT CAMERA: " + this.cameraCaptureTest);
            if (request.Url.AbsolutePath.TrimEnd('/').ToLower() == CAMERA_LIST_ENDPOINT)
            {
                // Work out how big this request was
                long byteCount = request.RawUrl.Length + request.ContentLength64;
                // Don't count headers + request.Headers.AllKeys.Sum(x => x.Length + request.Headers[x].Length + 1);
                dataRates.RecieveDataFromClient(Convert.ToInt32(byteCount));

                return processCameraManagerIndex(request, response);
            } else if (cameraNameEndpointRegex.IsMatch(request.Url.AbsolutePath))
            {
                Match match = cameraNameEndpointRegex.Match(request.Url.AbsolutePath);
                string cameraName = UnityEngine.WWW.UnEscapeURL(match.Groups[1].Value);
                //PluginLogger.debug("GET CAMERA: " + cameraName);
                return processCameraImageRequest(cameraName, request, response);
            }
            
            return false;
        }
    }
}
