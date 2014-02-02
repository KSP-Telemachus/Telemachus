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
            public event EventHandler<HTTPRequestEventArgs> HTTPRequestArrived;

            protected virtual void OnServerNotify(object sender, NotifyEventArgs e)
            {
                EventHandler<NotifyEventArgs> handler = ServerNotify;

                if (handler != null)
                {
                    ServerNotify(this, e);
                }
            }

            protected virtual void OnHTTPRequestArrived(object sender, HTTPRequestEventArgs e)
            {
                EventHandler<HTTPRequestEventArgs> handler = HTTPRequestArrived;

                if (handler != null)
                {
                    HTTPRequestArrived(this, e);
                }
            }

            AsynchronousServer.Server server = null;
            ServerConfiguration configuration = null;
            List<IHTTPRequestResponsibility> requestChainOfResponsibility = new List<IHTTPRequestResponsibility>();

            public Server(ServerConfiguration configuration)
            {
                this.configuration = configuration;

                server = new AsynchronousServer.Server(configuration,
                    (handler, s) =>
                    {
                        return new ClientConnection(handler, this, configuration);
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
                server.ServerNotify += OnServerNotify;
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

            public AsynchronousServer.Server getServer()
            {
                return server;
            }

            protected void ConnectionReceived(object sender, ConnectionEventArgs e)
            {
                //Subscribe to connection events before before commencing.
                e.clientConnection.ConnectionNotify += ConnectionNotify;
                e.clientConnection.ConnectionEmptyRead += ConnectionEmptyRead;
                ((MinimalHTTPServer.ClientConnection)e.clientConnection).HTTPRequestArrived += OnHTTPRequestArrived;

                e.clientConnection.startConnection();
            }

            private void ConnectionEmptyRead(object sender, ConnectionEventArgs e)
            {
                e.clientConnection.tryShutdown();
            }

            private void ConnectionNotify(object sender, ConnectionNotifyEventArgs e)
            {
                OnServerNotify(this, new NotifyEventArgs(e.message + "\n" +
                    "Client Connection: " + e.clientConnection.ToString()));
            }

            public void processRequest(AsynchronousServer.ClientConnection cc, HTTPRequest request)
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
            public event EventHandler<HTTPRequestEventArgs> HTTPRequestArrived;

            protected virtual void OnHTTPRequestArrived(HTTPRequestEventArgs e)
            {
                EventHandler<HTTPRequestEventArgs> handler = HTTPRequestArrived;

                if (handler != null)
                {
                    HTTPRequestArrived(this, e);
                }
            }

            ServerConfiguration configuration = null;
            HTTPRequest request = null;
            MinimalHTTPServer.Server server = null;

            public ClientConnection(Socket socket, MinimalHTTPServer.Server server,
                ServerConfiguration configuration)
                : base(socket, server.getServer())
            {
                this.configuration = configuration;
                this.server = server;

                ConnectionRead += StatefulConnectionRead;
            }

            protected void StatefulConnectionRead(object sender, ConnectionEventArgs e)
            {
                ClientConnection clientConnection = (ClientConnection)e.clientConnection;

                try
                {
                    if (request == null)
                    {
                        request = new HTTPRequest();
                    }

                    if (request.tryParseAppend(e.clientConnection.message, configuration))
                    {
                        HTTPRequestEventArgs eventArgs = new HTTPRequestEventArgs(this, request);
                        OnHTTPRequestArrived(eventArgs);

                        if (!eventArgs.cancel)
                        {
                            server.processRequest(clientConnection, request);
                        }

                        request = null;
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

            public virtual int Send(HTTPResponse r)
            {
                r.setAttribute("Server", configuration.name + " " + configuration.version);
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

        public class HTTPRequestEventArgs : EventArgs
        {
            public AsynchronousServer.ClientConnection clientConnection { get; set; }
            public HTTPRequest request { get; set; }
            public bool cancel { get; set; }

            public HTTPRequestEventArgs(AsynchronousServer.ClientConnection clientConnection, HTTPRequest request)
            {
                this.clientConnection = clientConnection;
                this.request = request;
                cancel = false;
            }
        }
    }
}
