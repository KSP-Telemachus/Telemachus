using System;
using Servers.MinimalHTTPServer;
using System.Collections.Generic;
using Servers.AsynchronousServer;

namespace ServersTest
{
    class RunningResponsibility : IHTTPRequestResponsibility
    {
        public bool process(Servers.AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            if (request.path.StartsWith("/server"))
            {
                try
                {
                    cc.Send(new OKResponsePage("Server Running.").ToString());
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
    }

    class ElseResponsibility : IHTTPRequestResponsibility
    {
        public bool process(Servers.AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            cc.Send(new IOLessDataLinkNotFound().ToString());
            return true;
        }
    }
}
