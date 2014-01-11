using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Servers.MinimalHTTPServer;


namespace ServersTest
{
    public class Program
    {
        public static int Main(String[] args)
        {
            ServerConfiguration config = new ServerConfiguration();
            Server server = new Server(config);
            server.OnServerNotify += new Server.ServerNotify(serverOut);
            server.addHTTPResponsibility(new ElseResponsibility());
            server.addHTTPResponsibility(new RunningResponsibility());

            server.startServing();

            Console.Read();
            server.stopServing();
            return 0;
        }

        private static void serverOut(String message)
        {
            Console.WriteLine(message);
        }
    }
}

