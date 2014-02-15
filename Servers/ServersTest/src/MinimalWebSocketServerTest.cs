//Author: Richard Bunt
using Servers;
using Servers.AsynchronousServer;
using Servers.MinimalWebSocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ServersTest
{
    class MinimalWebSocketServerTest : ITest
    {
        public void run()
        {
            Servers.MinimalWebSocketServer.ServerConfiguration config = new Servers.MinimalWebSocketServer.ServerConfiguration();
            config.bufferSize = 100;
            Servers.MinimalWebSocketServer.Server server = new Servers.MinimalWebSocketServer.Server(config);
            server.ServerNotify += server_ServerNotify;
            server.addWebSocketService("/server", new WebSocketEchoService());
            server.startServing();

            Console.Read();
            server.stopServing();
        }

        static void server_ServerNotify(object sender, Servers.NotifyEventArgs e)
        {
            Console.WriteLine(e.message);
        }
    }

    class WebSocketEchoService : IWebSocketService
    {

        public void OpCodePing(object sender, FrameEventArgs e)
        {

        }

        public void OpCodePong(object sender, FrameEventArgs e)
        {

        }

        public void OpCodeText(object sender, FrameEventArgs e)
        {
            WebSocketFrame frame = new WebSocketFrame(ASCIIEncoding.UTF8.GetBytes("Echo: " + e.frame.PayloadAsUTF8()));
            e.clientConnection.Send(frame.AsBytes());
        }

        public void OpCodeBinary(object sender, FrameEventArgs frameEventArgs)
        {

        }

        public void OpCodeClose(object sender, FrameEventArgs frameEventArgs)
        {

        }

        public IWebSocketService buildService(Servers.AsynchronousServer.ClientConnection clientConnection)
        {
            return new WebSocketEchoService();
        }

        public void Shutdown(EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
