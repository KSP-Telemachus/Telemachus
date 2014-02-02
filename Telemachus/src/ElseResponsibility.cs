//Author: Richard Bunt
using Servers.MinimalHTTPServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Telemachus
{
    class ElseResponsibility : IHTTPRequestResponsibility
    {
        #region Constants

        protected string INDEX_PAGE = "telemachus/information.html";

        #endregion

        #region IHTTPRequestResponsibility

        public bool process(Servers.AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            if (!KSP.IO.FileInfo.CreateForType<TelemachusDataLink>(INDEX_PAGE).Exists)
            {
                throw new PageNotFoundResponsePage("Unable to find the Telemachus index page. Is it installed in the PluginData folder?");
            }
            else
            {
                throw new PageNotFoundResponsePage(
                    "Did you mean to visit the <a href=\"/" + INDEX_PAGE + "\">index page</a>?");
            }
        }

        #endregion
    }
}
