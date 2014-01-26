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
            List<IHTTPRequestResponsibility> requestChainOfResponsibility = new List<IHTTPRequestResponsibility>();

            public const String GET = "GET";
            public const String POST = "POST";
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

            public void addHTTPResponsibility(IHTTPRequestResponsibility responsibility)
            {
                requestChainOfResponsibility.Insert(0, responsibility);
            }

            protected void ConnectionReceived(object sender, ConnectionEventArgs e)
            {
                //Subscribe to connection events before before commencing.
                e.clientConnection.ConnectionNotify += ConnectionNotify;
                e.clientConnection.ConnectionRead += ConnectionRead;
                e.clientConnection.ConnectionEmptyRead += ConnectionEmptyRead;
                e.clientConnection.startConnection();
            }

            private void ConnectionEmptyRead(object sender, ConnectionEventArgs e)
            {
                e.clientConnection.tryShutdown();
            }

            private void ConnectionRead(object sender, ConnectionEventArgs e)
            {
                ClientConnection clientConnection = (ClientConnection)e.clientConnection;

                String requestString = "";

                foreach (ArraySegment<byte> byteMessage in clientConnection.progressiveMessage)
                {
                    requestString += Encoding.ASCII.GetString(byteMessage.Array, 0, byteMessage.Count);
                }

                Logger.debug("Request String:" + requestString);
                try
                {
                    if (requestString.StartsWith(GET))
                    {
                        if (requestString.EndsWith(HEADER_END))
                        {
                            HTTPRequest request = new HTTPRequest();
                            request.parse(requestString);
                            processRequest(clientConnection, request);
                        }

                        if (requestString.Length > configuration.maxRequestLength)
                        {
                            throw new RequestEntityTooLargeResponsePage();
                        }
                    }
                    else if (requestString.StartsWith(POST))
                    {
                        HTTPRequest request = new HTTPRequest();

                        if (request.tryParse(requestString))
                        {
                            processRequest(clientConnection, request);
                        }
                    }
                    else
                    {
                        throw new BadRequestResponsePage();
                    }
                }
                catch (HTTPResponse r)
                {
                    clientConnection.Send(r);
                }
                catch (Exception ex)
                {
                    clientConnection.Send(new ExceptionResponsePage(ex.Message + " " + ex.StackTrace));
                }
            }

            private void ConnectionNotify(object sender, ConnectionNotifyEventArgs e)
            {
                OnServerNotify(new NotifyEventArgs(e.message + "\n" + 
                    "Client Connection: " + e.clientConnection.ToString()));
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
            private const int LARGEST_PROTOCOL_LENGTH = 4; 
            
            public string name { get; set; }
            public string version { get; set; }
            public int maxRequestLength { get; set; }

            public override int bufferSize
            {
                get
                {
                     return base.bufferSize;
                }
                set
                {
                    if (value >= LARGEST_PROTOCOL_LENGTH)
                    {
                        base.bufferSize = value;
                    }
                    else
                    {
                        throw new Exception("Server read buffer size is less than the length of the largest HTTP verb");
                    }
                }
            }

            public ServerConfiguration()
            {
                maxRequestLength = 8000;
            }
        }

        class FallBackRequestResponsibility : IHTTPRequestResponsibility
        {
            public bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request)
            {
                cc.Send(new PageNotFoundResponsePage().ToString());
                return true;
            }
        }

        public interface IHTTPRequestResponsibility
        {
            bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request);
        }
    }
}
