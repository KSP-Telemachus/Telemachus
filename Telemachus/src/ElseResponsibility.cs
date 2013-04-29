using Servers.MinimalHTTPServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Telemachus
{
    class ElseResponsibility : IHTTPRequestResponsibility
    {
        #region IHTTPRequestResponsibility

        public bool process(Servers.AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            IOLessDataLinkRedirectToApplication page = new IOLessDataLinkRedirectToApplication();
            page.setServerName("Telemachus " +
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            cc.Send(page.ToString());
            return true;
        }

        #endregion
    }

    class IOLessDataLinkRedirectToApplication : HTTPResponse
    {
        #region Constants

        const String HTML_FILE_SUFFIX = ".html";

        #endregion

        #region Constructors

        public IOLessDataLinkRedirectToApplication()
        {
            protocol = "HTTP/1.0";
            responseType = "Moved Permanently";
            responseCode = "301";
            attributes.Add("Location", "/telemachus/information.html");

            if (!KSP.IO.FileInfo.CreateForType<TelemachusDataLink>("information.html").Exists)
            {
                throw new SoftException("Unable to find the Telemachus index page. Is it installed in the PluginData folder?");
            }
        }

        #endregion

        #region Page Generation

        public override String ToString()
        {
            StringBuilder response = new StringBuilder(base.ToString());

            return response.ToString();
        }

        #endregion
    }  
}
