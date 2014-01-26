using Servers.AsynchronousServer;
using Servers.MinimalWebSocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServersTest
{
    class MinimalWebSocketServerTest
    {
        public static int Main(String[] args)
        {
            Servers.MinimalWebSocketServer.ServerConfiguration config = new Servers.MinimalWebSocketServer.ServerConfiguration();
            Servers.MinimalWebSocketServer.Server server = new Servers.MinimalWebSocketServer.Server(config);
            server.ServerNotify += server_ServerNotify;

            server.startServing();

            Console.Read();
            server.stopServing();
            return 0;
        }

        static void server_ServerNotify(object sender, Servers.NotifyEventArgs e)
        {
            Console.WriteLine(e.message);
        }
    }
}
