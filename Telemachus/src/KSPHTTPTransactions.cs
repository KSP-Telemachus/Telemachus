using System;
using MinimalHTTPServer;
using System.Text;

namespace Telemachus
{
    class IOLessDataLinkNotFound : HTTPResponse
    {
        public IOLessDataLinkNotFound()
        {
            protocol = "HTTP/1.0";
            responseType = "Not Found";
            responseCode = "404";
            attributes.Add("Content-Type", "text / html");
            attributes.Add("Content-Length", "0");
            content = "<html><body><h1>No datalink found at this address</h1></body></html>";
        }

        public override String ToString()
        {
            StringBuilder response = new StringBuilder(base.ToString());

            return response.ToString();
        }
    }  
}
