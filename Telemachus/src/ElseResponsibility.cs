//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Net;

namespace Telemachus
{
    class ElseResponsibility : IHTTPRequestResponder
    {
        #region Constants

        protected string INDEX_PAGE = "telemachus/information.html";

        #endregion

        #region IHTTPRequestResponder

        public bool process(HttpListenerRequest request, HttpListenerResponse response)
        {
            PluginLogger.print("Falling back on default handler");

            // For now, copy the behaviour until we understand it more
            if (!KSP.IO.FileInfo.CreateForType<TelemachusDataLink>(INDEX_PAGE).Exists)
            {
                throw new FileNotFoundException("Unable to find the Telemachus index page. Is it installed in the PluginData folder?");
            }
            else if (request.RawUrl == "/" || request.RawUrl.ToLowerInvariant().StartsWith("/index"))
            {
                // Just redirect them
                var index = new Uri(request.Url, "/" + INDEX_PAGE);
                response.Redirect(index.ToString());
                return true;
            }
            return false;
        }
        #endregion
    }
}
