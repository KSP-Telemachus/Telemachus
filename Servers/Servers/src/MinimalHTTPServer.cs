//Author: Richard Bunt
using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.Net.Sockets;

namespace Servers
{
    namespace MinimalHTTPServer
    {
        public class Server
        {
            public delegate void ServerNotify(String message);

            public event ServerNotify OnServerNotify;

            AsynchronousServer.Server server = null;
            ServerConfiguration configuration = null;
            List<IHTTPRequestResponsibility> requestChainOfResponsibility = new List<IHTTPRequestResponsibility>();

            public const String GET = "GET";
            public const String HEADER_END = "\r\n\r\n";

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
                server.OnConnection += new AsynchronousServer.Server.Connection(connection);
                server.OnServerNotify += new AsynchronousServer.Server.ServerNotify(serverOut);
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

            public void addHTTPResponsibility(IHTTPRequestResponsibility responsibility)
            {
                requestChainOfResponsibility.Insert(0, responsibility);
            }

            private void connection(AsynchronousServer.ClientConnection cc)
            {
                //Register events before commencing the connection.
                cc.OnConnectionNotify += new AsynchronousServer.Server.ConnectionNotify(connectionOut);
                cc.OnConnectionRead += new AsynchronousServer.Server.ConnectionRead(connectionRead);
                cc.startConnection();
            }
            
            private void connectionRead(AsynchronousServer.ClientConnection clientConnection)
            {
                ClientConnection cc = (ClientConnection)clientConnection;

                try
                {
                    if (cc.progressiveMessage.ToString().StartsWith(GET))
                    {
                        if (cc.progressiveMessage.ToString().EndsWith(HEADER_END))
                        {
                            HTTPRequest request = new HTTPRequest();
                            request.parse(cc.progressiveMessage.ToString());
                            processRequest(cc, request);
                        }

                        if (cc.progressiveMessage.Length > configuration.maxRequestLength)
                        {
                            throw new RequestEntityTooLargeResponsePage();
                        }
                    }
                    else
                    {
                        throw new BadRequestResponsePage();
                    }
                }
                catch (HTTPResponse r)
                {
                    cc.Send(r);
                }
                catch (Exception e)
                {
                    cc.Send(new ExceptionResponsePage(e.Message + " " + e.StackTrace));
                }
            }

            private void processRequest(AsynchronousServer.ClientConnection cc, HTTPRequest request)
            {
                foreach (IHTTPRequestResponsibility responsibility in requestChainOfResponsibility)
                {
                    if (responsibility.process(cc, request))
                    {
                        break;
                    }
                }
            }

            private void serverOut(String message)
            {
                if (OnServerNotify != null)
                {
                    OnServerNotify(message);
                }
            }

            private void connectionOut(AsynchronousServer.ClientConnection cc, String message)
            {
                if (OnServerNotify != null)
                {
                    OnServerNotify(message);
                }
            } 
        }

        public class ClientConnection : AsynchronousServer.ClientConnection
        {
            ServerConfiguration configuration = null;

            public ClientConnection(Socket socket, AsynchronousServer.Server server, 
                ServerConfiguration configuration): base(socket, server)
            {
                this.configuration = configuration;
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
        }

        public interface IHTTPRequestResponsibility
        {
            bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request);
        }

        class FallBackRequestResponsibility : IHTTPRequestResponsibility
        {
            public bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request)
            {
                cc.Send(new PageNotFoundResponsePage().ToString());
                return true;
            }
        }
    }
}
