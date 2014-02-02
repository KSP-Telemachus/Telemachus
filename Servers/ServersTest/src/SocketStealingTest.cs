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

            // configure web socket server but do not start running
            Servers.MinimalWebSocketServer.ServerConfiguration webSocketconfig = new Servers.MinimalWebSocketServer.ServerConfiguration();
            webSocketconfig.bufferSize = 100;
            Servers.MinimalWebSocketServer.Server webSocketServer = new Servers.MinimalWebSocketServer.Server(webSocketconfig);
            webSocketServer.ServerNotify += server_ServerNotify;
            webSocketServer.addWebSocketService("/server", new WebSocketEchoService());
            webSocketServer.subscribeToHTTPForStealing(server);

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
