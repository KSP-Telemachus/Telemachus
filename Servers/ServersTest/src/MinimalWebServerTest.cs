//Author: Richard Bunt
using Servers.AsynchronousServer;
using Servers.MinimalHTTPServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServersTest
{
    class MinimalWebServerTest : ITest
    {
        public void run()
        {
            Servers.MinimalHTTPServer.ServerConfiguration config = new Servers.MinimalHTTPServer.ServerConfiguration();
            Servers.MinimalHTTPServer.Server server = new Servers.MinimalHTTPServer.Server(config);
            server.ServerNotify += server_ServerNotify;
            server.addHTTPResponsibility(new ElseResponsibility());
            server.addHTTPResponsibility(new RunningResponsibility());

            server.startServing();
            Console.Read();
            server.stopServing();
        }

        static void server_ServerNotify(object sender, Servers.NotifyEventArgs e)
        {
            Console.WriteLine(e.message);
        }
    }

    public class DiskLessTestResponse : HTTPResponse
    {
        public DiskLessTestResponse()
        {
            protocol = "HTTP/1.0";
            responseType = "Not Found";
            responseCode = "404";
            attributes.Add("Content-Type", "text / html");
            attributes.Add("Content-Length", "0");
            content = "<html><body><h1>Not Found</h1></body></html>";
        }

        public override String ToString()
        {
            StringBuilder response = new StringBuilder(base.ToString());

            return response.ToString();
        }
    }

    public class RunningResponsibility : IHTTPRequestResponsibility
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

    public class ElseResponsibility : IHTTPRequestResponsibility
    {
        public bool process(Servers.AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            cc.Send(new DiskLessTestResponse().ToString());
            return true;
        }
    }
}
