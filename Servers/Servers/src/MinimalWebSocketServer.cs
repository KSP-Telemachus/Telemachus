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
                foreach (IWebsSocketRequestResponsibility responsibility in requestChainOfResponsibility)
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

                ConnectionRead += StatefulWebSocketHandShake;
            }

            public enum OpCode
            {
                Continue = 0x0,
                Text = 0x1,
                Binary = 0x2,
                Close = 0x8,
                Ping = 0x9,
                Pong = 0xA
            }

            protected virtual void StatefulWebsocketHandler(object sender, ConnectionEventArgs e)
            {
                ClientConnection clientConnection = (ClientConnection)e.clientConnection;

                try
                {
                    byte[] b = clientConnection.progressiveMessage[0].Array.Take(2).ToArray<byte>();
                    IEnumerable<byte> position = clientConnection.progressiveMessage[0].Array.Skip(2);

                    bool fin = (b[0] & 0x80) == 0x80;
                    bool rsv1 = (b[0] & 0x40) == 0x40;
                    bool rsv2 = (b[0] & 0x20) == 0x20;
                    bool rsv3 = (b[0] & 0x10) == 0x10;

                    OpCode opCode = (OpCode)((b[0] & 0x8) 
                        | (b[0] & 0x4) | (b[0] & 0x2) | (b[0] & 0x1));

                    bool mask = (b[1] & 0x80) == 0x80;

                    byte payload = (byte)((clientConnection.progressiveMessage[0].Array[1] & 0x40)
                        | (b[1] & 0x20)
                        | (b[1] & 0x10) | (b[1] & 0x8)
                        | (b[1] & 0x4) | (b[1] & 0x2)
                        | (b[1] & 0x1));

                    ulong length = 0;

                    switch (payload)
                    {
                        case 126:
                            byte[] bytesUShort = position.Take(2).ToArray<byte>();
                            if (bytesUShort != null)
                            {
                                length = BitConverter.ToUInt16(bytesUShort.Reverse().ToArray(), 0);
                            }
                            position = position.Skip(2);
                            break;
                        case 127:
                            byte[] bytesULong = position.Take(8).ToArray<byte>();
                            if (bytesULong != null)
                            {
                                length = BitConverter.ToUInt16(bytesULong.Reverse().ToArray(), 0);
                            }
                            position = position.Skip(8);
                            break;
                        default:
                            length = payload;
                            break;
                    }
                    
                    byte[] maskKeys = null;
                    if (mask)
                    {
                        maskKeys = position.Take(4).ToArray<byte>();
                        position = position.Skip(4);
                    }

                    byte[] data = position.Take((int)length).ToArray<byte>();

                    if (mask)
                    {
                        for (int i = 0; i < data.Length; ++i)
                        {
                            data[i] = (byte)(data[i] ^ maskKeys[i % 4]);
                        }
                    }

                    ushort closeCode = 0;
                    if (opCode == OpCode.Close && data.Length == 2)
                    {
                        closeCode = BitConverter.ToUInt16(((byte[])data.Clone()).Reverse().ToArray(), 0);
                    }

                    progressiveMessage.RemoveAt(0);

                    Logger.debug("Message:" + Encoding.UTF8.GetString(data));
                    Logger.debug("Payload length: " + length);
                    Logger.debug("Mask: " + mask);
                    Logger.debug("Fin: " + fin);
                    Logger.debug("RSV1: " + rsv1);
                    Logger.debug("RSV2: " + rsv2);
                    Logger.debug("RSV3: " + rsv3);

                }
                catch (Exception ex)
                {
                    Logger.debug("Websocket Read failed: " + ex.ToString());
                }

            }

            protected virtual void StatefulWebSocketHandShake(object sender, ConnectionEventArgs e)
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
                    if (requestString.StartsWith(Servers.MinimalHTTPServer.Server.GET))
                    {
                        if (requestString.EndsWith(Servers.MinimalHTTPServer.Server.HEADER_END))
                        {
                            HTTPRequest request = new HTTPRequest();
                            Logger.debug(e.clientConnection.progressiveMessage.ToString());
                            request.parse(requestString);
                            HTTPResponse response = new WebsocketUpgrade(request.getAttribute("Sec-WebSocket-Key"));
                            Logger.debug(response.ToString());
                            clientConnection.progressiveMessage.Clear();
                            upgradeConnectionTorfc6455();

                            clientConnection.Send(response.ToBytes());
                        }

                        if (clientConnection.progressiveMessage.Count > configuration.maxRequestLength)
                        {
                            Logger.debug("Request too long.");
                            throw new RequestEntityTooLargeResponsePage();
                        }
                    }
                    else
                    {
                        Logger.debug("Bad websocket handshake.");
                        throw new BadRequestResponsePage();
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
