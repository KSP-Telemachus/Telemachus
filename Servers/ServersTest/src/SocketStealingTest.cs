//Author: Richard Bunt
using Servers.AsynchronousServer;
using Servers.MinimalHTTPServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServersTest
{
    class SocketStealingTest : ITest
    {
        public void run()
        {
            // configure http server but do not start running yet
            Servers.MinimalHTTPServer.ServerConfiguration config = new Servers.MinimalHTTPServer.ServerConfiguration();
            Servers.MinimalHTTPServer.Server server = new Servers.MinimalHTTPServer.Server(config);
            server.ServerNotify += server_ServerNotify;
            server.addHTTPResponsibility(new ElseResponsibility());
            server.addHTTPResponsibility(new RunningResponsibility());
            
            // start the HTTP server
            server.startServing();
            Console.Read();
            server.stopServing();
        }

        static void server_ServerNotify(object sender, Servers.NotifyEventArgs e)
        {
            Console.WriteLine(e.message);
        }
    }
}
