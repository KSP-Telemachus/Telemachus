//Author: Richard Bunt
using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.Net.Sockets;
using Servers.MinimalHTTPServer;
using System.Linq;
using System.IO;

namespace Servers
{
    namespace MinimalWebSocketServer
    {
        public class Server
        {
            public event EventHandler<NotifyEventArgs> ServerNotify;

            protected virtual void OnServerNotify(object sender, NotifyEventArgs e)
            {
                EventHandler<NotifyEventArgs> handler = ServerNotify;

                if (handler != null)
                {
                    ServerNotify(this, e);
                }
            }

            AsynchronousServer.Server server = null;
            ServerConfiguration configuration = null;
            Dictionary<string, IWebSocketService> webSocketServices = new Dictionary<string, IWebSocketService>();

            public Server(ServerConfiguration configuration)
            {
                this.configuration = configuration;

                server = new AsynchronousServer.Server(configuration,
                    (handler, s) =>
                    {
                        return new ClientConnection(handler, s, configuration, webSocketServices);
                    });
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

            public void subscribeToHTTPForStealing(MinimalHTTPServer.Server server)
            {
                server.HTTPRequestArrived += HTTPRequestArrived;
            }

            public String getIPsAsString()
            {
                return server.configuration.getIPsAsString();
            }

            public void addWebSocketService(string prefix, IWebSocketService service)
            {
                webSocketServices[prefix] = service;
            }

            protected void ConnectionReceived(object sender, ConnectionEventArgs e)
            {
                //Subscribe to connection events before before commencing.
                e.clientConnection.ConnectionNotify += ConnectionNotify;
                e.clientConnection.startConnection();
            }

            private void ConnectionNotify(object sender, ConnectionNotifyEventArgs e)
            {
                OnServerNotify(this, new NotifyEventArgs(e.message + "\n" +
                    "Client Connection: " + e.clientConnection.ToString()));
            }

            protected void HTTPRequestArrived(object sender, HTTPRequestEventArgs e)
            {
                if (isWebSocketRequest(e.request))
                {
                    e.cancel = true;

                    ClientConnection upgradedClientConnection = new ClientConnection(e.clientConnection.stealSocket(),
                            server, this.configuration, webSocketServices, e.request);

                    upgradedClientConnection.ConnectionNotify += ConnectionNotify;
                    upgradedClientConnection.startConnectionWithHandShakeResponse();
                }
            }

            public static bool isWebSocketRequest(HTTPRequest request)
            {
                try
                {
                    request.getAttribute("Upgrade").Equals("websocket");
                    return true;
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
            }
        }

        public class ClientConnection : AsynchronousServer.ClientConnection
        {
            ServerConfiguration configuration = null;
            HTTPRequest request = null;
            WebSocketFrame frame = null;
            AsynchronousServer.Server server = null;
            IWebSocketService service = null;
            List<byte> data = null;
            Dictionary<string, IWebSocketService> webSocketServices = null;

            public ClientConnection(Socket socket, AsynchronousServer.Server server,
                ServerConfiguration configuration, Dictionary<string, IWebSocketService> webSocketServices)
                : base(socket, server)
            {
                this.configuration = configuration;
                this.server = server;
                this.webSocketServices = webSocketServices;

                data = new List<byte>();
                frame = new WebSocketFrame();
                request = new HTTPRequest();

                ConnectionRead += StatefulWebSocketHandShake;
            }

            public ClientConnection(Socket socket, AsynchronousServer.Server server,
                ServerConfiguration configuration, Dictionary<string, IWebSocketService> webSocketServices,
                HTTPRequest request)
                : this(socket, server, configuration, webSocketServices)
            {
                this.request = request;
            }

            public void startConnectionWithHandShakeResponse()
            {
                upgradeConnectionTorfc6455();
                base.startConnection();
            }

            protected virtual void StatefulWebsocketHandler(object sender, ConnectionEventArgs e)
            {
                ClientConnection clientConnection = (ClientConnection)e.clientConnection;

                int oldSize = data.Count;
                data.AddRange(clientConnection.message.Array);
                data.RemoveRange(oldSize + clientConnection.message.Count, clientConnection.message.Array.Length - clientConnection.message.Count);

                try
                {
                    while (true)
                    {
                        int frameSize = frame.parse(data, configuration);
                        data.RemoveRange(0, frameSize);

                        switch (frame.header.opCode)
                        {
                            case OpCode.Ping:
                                OpCodePing(this, new FrameEventArgs(frame, e.clientConnection));
                                break;
                            case OpCode.Pong:
                                OpCodePong(this, new FrameEventArgs(frame, e.clientConnection));
                                break;
                            case OpCode.Text:
                                OpCodeText(this, new FrameEventArgs(frame, e.clientConnection));
                                break;
                            case OpCode.Binary:
                                OpCodeBinary(this, new FrameEventArgs(frame, e.clientConnection));
                                break;
                            case OpCode.Close:
                                OpCodeClose(this, new FrameEventArgs(frame, e.clientConnection));
                                break;
                            default:
                                Default(this, new FrameEventArgs(frame, e.clientConnection));
                                break;
                        }

                        frame = new WebSocketFrame();
                    }
                }
                catch (InsufficientDataToParseFrameException)
                {
                    // exit this function and read more data.
                }
            }

            private void OpCodePing(object sender, FrameEventArgs e)
            {
                e.clientConnection.Send((new WebSocketPingFrame()).AsBytes());
                service.OpCodePing(this, e);
                OnConnectionNotify(new ConnectionNotifyEventArgs("ping received.", this));
            }

            private void OpCodePong(ClientConnection clientConnection, FrameEventArgs frameEventArgs)
            {
                service.OpCodePong(this, frameEventArgs);
                OnConnectionNotify(new ConnectionNotifyEventArgs("pong received.", this));
            }

            private void OpCodeText(object sender, FrameEventArgs frameEventArgs)
            {
                service.OpCodeText(this, frameEventArgs);
            }

            private void OpCodeBinary(object sender, FrameEventArgs frameEventArgs)
            {
                service.OpCodeBinary(sender, frameEventArgs);
            }

            private void OpCodeClose(object sender, FrameEventArgs frameEventArgs)
            {
                service.OpCodeClose(this, frameEventArgs);
            }

            private void Default(object sender, FrameEventArgs frameEventArgs)
            {

            }

            protected virtual void StatefulWebSocketHandShake(object sender, ConnectionEventArgs e)
            {
                ClientConnection clientConnection = (ClientConnection)e.clientConnection;

                try
                {
                    if (request.tryParseAppend(e.clientConnection.message, configuration.maxRequestLength))
                    {
                        if (MinimalWebSocketServer.Server.isWebSocketRequest(request))
                        {
                            if (!webSocketServices.ContainsKey(request.path))
                            {
                                throw new PageNotFoundResponsePage();
                            }

                            upgradeConnectionTorfc6455();
                        }
                    }
                }
                catch (HTTPResponse r)
                {
                    clientConnection.Send(r);
                    OnConnectionNotify(new ConnectionNotifyEventArgs(r.ToString(), this));
                }
                catch (Exception ex)
                {
                    clientConnection.Send(new ExceptionResponsePage(ex.Message + " " + ex.StackTrace));
                    OnConnectionNotify(new ConnectionNotifyEventArgs(ex.Message + ex.StackTrace, this));
                }
            }

            protected override void OnShutdown(EventArgs e)
            {
                base.OnShutdown(e);

                service.Shutdown(e);
            }

            private void upgradeConnectionTorfc6455()
            {
                service = webSocketServices[request.path].buildService(this);
                ConnectionRead -= StatefulWebSocketHandShake;
                ConnectionRead += StatefulWebsocketHandler;
                Send(new WebsocketUpgrade(request.getAttribute("Sec-WebSocket-Key")).ToBytes());
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
            public string name { get; set; }
            public string version { get; set; }
            public int maxRequestLength { get; set; }
            public ulong maxMessageSize { get; set; }

            public ServerConfiguration()
            {
                maxRequestLength = 8000;
                maxMessageSize = 4000;
            }
        }
    }

    public class FrameEventArgs
    {
        public WebSocketFrame frame { get; set; }
        public Servers.AsynchronousServer.ClientConnection clientConnection { get; set; }

        public FrameEventArgs(WebSocketFrame frame, Servers.AsynchronousServer.ClientConnection clientConnection)
        {
            this.frame = frame;
            this.clientConnection = clientConnection;
        }
    }

    public interface IWebSocketService
    {
        void OpCodePing(object sender, FrameEventArgs e);
        void OpCodePong(object sender, FrameEventArgs e);
        void OpCodeText(object sender, FrameEventArgs e);
        void OpCodeBinary(object sender, FrameEventArgs frameEventArgs);
        void OpCodeClose(object sender, FrameEventArgs frameEventArgs);
        void Shutdown(EventArgs e);
        IWebSocketService buildService(Servers.AsynchronousServer.ClientConnection clientConnection);
    }
}
