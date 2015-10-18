//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Reflection;
using WebSocketSharp.Net;
using WebSocketSharp;

namespace Telemachus
{
    public class DataLinkResponsibility : IHTTPRequestResponder
    {
        /// The page prefix that this class handles
        public const String PAGE_PREFIX = "/telemachus/datalink";
        /// The KSP API to use to access variable data
        private IKSPAPI kspAPI = null;

        private UpLinkDownLinkRate dataRates = null;

        #region Initialisation

        public DataLinkResponsibility(IKSPAPI kspAPI, UpLinkDownLinkRate rateTracker)
        {
            this.kspAPI = kspAPI;
            dataRates = rateTracker;
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

            // Now, serialize the dictionary and write to the response
            var returnData = Encoding.UTF8.GetBytes(SimpleJson.SimpleJson.SerializeObject(results));
            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = "application/json";
            response.WriteContent(returnData);
            dataRates.SendDataToClient(returnData.Length);
            return true;
        }
    }

    public class DataSources
    {
        #region Fields

        public Vessel vessel;
        public List<String> args = new List<String>();
        protected string varName;
        #endregion

        public DataSources Clone()
        {
            DataSources d = new DataSources();
            d.vessel = this.vessel;
            d.args = new List<string>(this.args);
            return d;
        }
    }
}
