//Author: Richard Bunt
using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.Net.Sockets;
using Servers.MinimalHTTPServer;
using System.Linq;

namespace Servers
{
    namespace MinimalWebSocketServer
    {
        public class Server
        {
            public event EventHandler<NotifyEventArgs> ServerNotify;

            protected virtual void OnServerNotify(NotifyEventArgs e)
            {
                EventHandler<NotifyEventArgs> handler = ServerNotify;

                if (handler != null)
                {
                    ServerNotify(this, e);
                }
            }

            AsynchronousServer.Server server = null;
            ServerConfiguration configuration = null;
            List<IWebsSocketRequestResponsibility> requestChainOfResponsibility = new List<IWebsSocketRequestResponsibility>();

            public Server(ServerConfiguration configuration)
            {
                this.configuration = configuration;

                server = new AsynchronousServer.Server(configuration,
                    (handler, s) =>
                {
                    return new ClientConnection(handler, s, configuration);
                });

                requestChainOfResponsibility.Add(new FallBackRequestResponsibility());
            }

            ~Server()
            {
                server.stopListening();
            }

            public void startServing()
            {
                server.ConnectionReceived += ConnectionReceived;
                server.ServerNotify += ServerNotify;
                server.startListening();
            }

            public void stopServing()
            {
                server.stopListening();
            }

            public String getIPsAsString()
            {
                return server.configuration.getIPsAsString();
            }

            public void addHTTPResponsibility(IWebsSocketRequestResponsibility responsibility)
            {
                requestChainOfResponsibility.Insert(0, responsibility);
            }

            protected void ConnectionReceived(object sender, ConnectionEventArgs e)
            {
                //Subscribe to connection events before before commencing.
                e.clientConnection.ConnectionNotify += ConnectionNotify;
                e.clientConnection.startConnection();
            }

            private void ConnectionNotify(object sender, ConnectionNotifyEventArgs e)
            {
                OnServerNotify(new NotifyEventArgs(e.message + "\n" +
                    "Client Connection: " + e.clientConnection.ToString()));
            }
            
            private void processRequest(AsynchronousServer.ClientConnection cc, HTTPRequest request)
            {
               
            } 
        }

        public class ClientConnection : AsynchronousServer.ClientConnection
        {
            ServerConfiguration configuration = null;
            HTTPRequest request = null;
            WebSocketFrame frame = new WebSocketFrame();

            public ClientConnection(Socket socket, AsynchronousServer.Server server, 
                ServerConfiguration configuration): base(socket, server)
            {
                this.configuration = configuration;

                ConnectionRead += StatefulWebSocketHandShake;
            }

            protected virtual void StatefulWebsocketHandler(object sender, ConnectionEventArgs e)
            {
                ClientConnection clientConnection = (ClientConnection)e.clientConnection;

                frame.parse(e.clientConnection.message);

                WebSocketFrame f = new WebSocketFrame(ASCIIEncoding.UTF8.GetBytes("message from socket server"));

                clientConnection.Send(f.AsFrame().Array);
            }

            protected virtual void StatefulWebSocketHandShake(object sender, ConnectionEventArgs e)
            {
                ClientConnection clientConnection = (ClientConnection)e.clientConnection;

                try
                {
                    if (request == null)
                    {
                        request = new HTTPRequest();
                    }

                    if (request.tryParseAppend(e.clientConnection.message, 10000))
                    {
                        upgradeConnectionTorfc6455();
                        clientConnection.Send(new WebsocketUpgrade(request.getAttribute("Sec-WebSocket-Key")).ToBytes());
                        request = null;
                    }
                }
                catch (HTTPResponse r)
                {
                    clientConnection.Send(r);
                    Logger.debug(r.ToString());
                }
                catch (Exception ex)
                {
                    clientConnection.Send(new ExceptionResponsePage(ex.Message + " " + ex.StackTrace));
                    Logger.debug(ex.Message + ex.StackTrace);
                }
            }

            private void upgradeConnectionTorfc6455()
            {
                ConnectionRead -= StatefulWebSocketHandShake;
                ConnectionRead += StatefulWebsocketHandler;
            }

            public virtual int Send(HTTPResponse r)
            {
                r.setAttribute("Server", configuration.name + " " +  configuration.version);
                byte[] toSend = r.ToBytes();
                Send(toSend);
                return toSend.Length;
            }
        }

        public class ServerConfiguration : AsynchronousServer.ServerConfiguration
        {
            public string name { get; set; }
            public string version { get; set; }
            public int maxRequestLength { get; set; }

            public ServerConfiguration()
            {
                maxRequestLength = 8000;
            }
        }

        public interface IWebsSocketRequestResponsibility
        {
            bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request);
        }

        class FallBackRequestResponsibility : IWebsSocketRequestResponsibility
        {
            public bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request)
            {
                cc.Send(new PageNotFoundResponsePage().ToString());
                return true;
            }
        }
    }
}
