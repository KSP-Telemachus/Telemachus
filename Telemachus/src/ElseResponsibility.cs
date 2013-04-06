using MinimalHTTPServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Telemachus
{
    class ElseResponsibility : IHTTPRequestResponsibility
    {
        #region IHTTPRequestResponsibility

        public bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            cc.Send(new IOLessDataLinkRedirectToApplication(IOPageResponsibility.files).ToString());
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

        public IOLessDataLinkRedirectToApplication(String[] files)
        {
            protocol = "HTTP/1.0";
            responseType = "Moved Permanently";
            responseCode = "301";
            attributes.Add("Location", "/telemachus/telemachus.html");
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
