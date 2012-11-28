using System;
using System.Text;
using RedCorona.Net;
using System.Collections;
using KSP.IO;

namespace Telemachus
{
    class DataLocator : IHttpHandler {
		
        private static string ROOT = "telemachus";
        TelemachusDataLink telemachusDataLink = null;

        public DataLocator(TelemachusDataLink telemachusDataLink)
        {
            this.telemachusDataLink = telemachusDataLink;
        }
        int i = 0;
		public virtual bool Process(HttpServer server, HttpRequest request, HttpResponse response){

            string mime = "text / html";
            response.ContentType = mime;
            if (request.Url.Contains(ROOT))
            {
                SensorBuffer sb = telemachusDataLink.ContainsBuffer(getSensorName(request.Url));

                if (sb != null)
                {
                    if (request.Url.Contains("data"))
                    {
                        response.Content = sb.ToJavaScript();
                        sb.Update();
                    }
                    else
                    {
                        TextReader index = TextReader.CreateForType<TelemachusDataLink>("telemachus.html");
                        response.Content = index.ReadToEnd();
                        response.Content = response.Content.Replace("@DATALOCATION", "http://192.168.1.64:8080/" 
                            + ROOT + "/data/" + getSensorName(request.Url));
                        response.Content = response.Content.Replace("@TITLE", getSensorName(request.Url));
                        index.Close();
                    }
                }
                else
                {
                    response.Content = "Nothing to see here.";
                }
            }
            else
            {
                response.Content = "Nothing to see here.";
            }
           
			return true;
		}

        private String getSensorName(String url)
        {
            String[] com = url.Split('/');

            return com[com.Length - 1];
        }
		
		public virtual string GetValue(HttpRequest req, string tag){
			return "<span class=error>Unknown substitution: " + tag + "</span>";
		}
	}
}
