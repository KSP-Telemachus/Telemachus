//Author: Richard Bunt
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System;
using System.Net;
using System.Collections.Generic;

namespace Servers
{
    namespace AsynchronousServer
    {
        public class Server
        {
            public delegate ClientConnection ClientConnectionBuilder(Socket handler, Server server);
            ClientConnectionBuilder clientConnectionBuilder = (handler, server) =>
            {
                return new ClientConnection(handler, server);
            };

            public delegate void Connection(ClientConnection cc);
            public delegate void ConnectionRead(ClientConnection cc);
            public delegate void ConnectionNotify(ClientConnection cc, String message);
            public delegate void ServerNotify(String message);
            public delegate void ServerShutdown();

            public event ServerNotify OnServerNotify;
            public event Connection OnConnection;
            public event ServerShutdown OnShutdown;

            List<Socket> listeners = new List<Socket>();
            public ServerConfiguration configuration { get; set; }

            const String LOCALHOST = "127.0.0.1";

            public Server(ServerConfiguration serverConfiguration)
            {
                this.configuration = serverConfiguration;
            }

            public Server(ServerConfiguration serverConfiguration, ClientConnectionBuilder clientConnectionBuilder)
                :this (serverConfiguration)
            {
                this.clientConnectionBuilder = clientConnectionBuilder;
            }

            ~Server()
            {
                stopListening();
            }

            public void startListening()
            {
                bool success = false;
                Socket listener = null;

                //Try to bind to the user specified address if it exists
                if (configuration.ipAddresses.Count > 0)
                {
                    try
                    {
                        serverOut("Trying user defined address.");
                        listener = buildNewListener();
                        tryBind(listener, configuration);
                        listeners.Add(listener);
                        success = true;
                    }
                    catch
                    {
                        configuration.ipAddresses.RemoveAt(0);
                        //Search for alternative address.
                    }
                }

                //Try to bind to all the available adaptors
                if (!success)
                {
                    IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (IPAddress ip in ipHostInfo.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            try
                            {
                                serverOut("Trying internal addresses.");
                                configuration.ipAddresses.Insert(0, ip);
                                listener = buildNewListener();
                                tryBind(listener, configuration);


                                listeners.Add(listener);
                                success = true;
                            }
                            catch
                            {
                                configuration.ipAddresses.RemoveAt(0);
                                //Search for alternative address.
                                continue;
                            }
                        }
                    }

                    //Try to bind to localhost.
                    try
                    {
                        serverOut("Trying localhost.");
                        configuration.ipAddresses.Insert(0, IPAddress.Parse(LOCALHOST));
                        listener = buildNewListener();
                        tryBind(listener, configuration);
                        listeners.Add(listener);
                        success = true;
                    }
                    catch
                    {
                        configuration.ipAddresses.RemoveAt(0);
                    }
                }

                if (!success)
                {
                    throw new Exception("Unable to find bind to address.");
                }
                else
                {
                    serverOut("Waiting for a connection.");
                }
            }

            public void stopListening()
            {
                foreach (Socket listener in listeners)
                {
                    listener.Close();
                }

                if (OnShutdown != null)
                {
                    OnShutdown();
                }
            }

            private void serverOut(String s)
            {
                if (OnServerNotify != null)
                {
                    OnServerNotify(s);
                }
            }

            private Socket buildNewListener()
            {
                return new Socket(AddressFamily.InterNetwork,
                       SocketType.Stream, ProtocolType.Tcp);
            }

            private void tryBind(Socket listener, ServerConfiguration configuration)
            {
                IPEndPoint localEndPoint = new IPEndPoint(configuration.ipAddresses[0], configuration.port);

                listener.Bind(localEndPoint);
                listener.Listen(configuration.backLog);
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            }

            private void AcceptCallback(IAsyncResult ar)
            {
                try
                {
                    Socket listener = (Socket)ar.AsyncState, handler = listener.EndAccept(ar);

                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    if (OnConnection != null)
                    {
                        OnConnection(clientConnectionBuilder(handler, this));
                    }
                }
                catch (Exception e)
                {
                    serverOut(e.ToString());
                }
            }
        }

        public class ClientConnection
        {
            public event Server.ConnectionNotify OnConnectionNotify;
            public event Server.ConnectionRead OnConnectionRead;

            Socket socket;
            // Buffer size should be at least as big as the largest protocol verb
            public const int BUFFER_SIZE = 512;
            public byte[] buffer { get; set; }
            public StringBuilder progressiveMessage { get; set; }

            private Server server = null;
            private Server.ServerShutdown shutdownCallback = null;

            public ClientConnection(Socket socket, Server server)
            {
                progressiveMessage = new StringBuilder();
                buffer = new byte[BUFFER_SIZE];

                this.socket = socket;
                this.server = server;
                shutdownCallback = new Server.ServerShutdown(requestShutdown);
                server.OnShutdown += shutdownCallback;
            }

            public void startConnection()
            {
                try
                {
                    socket.BeginReceive(buffer, 0, BUFFER_SIZE, 0,
                       new AsyncCallback(ReadCallback), this);
                }
                catch (Exception e)
                {
                    if (OnConnectionNotify != null)
                    {
                        OnConnectionNotify(this, e.ToString());
                    }
                }
            }

            private void requestShutdown()
            {
                socket.Close();
            }

            private void ReadCallback(IAsyncResult ar)
            {
                try
                {
                    String content = String.Empty;

                    int bytesRead = socket.EndReceive(ar);

                    if (bytesRead > 0)
                    {
                        progressiveMessage.Append(Encoding.ASCII.GetString(
                            buffer, 0, bytesRead));

                        if (OnConnectionRead != null)
                        {
                            OnConnectionRead(this);
                        }

                        socket.BeginReceive(buffer, 0, ClientConnection.BUFFER_SIZE, 0,
                        new AsyncCallback(ReadCallback), this);
                    }
                    else
                    {
                        tryShutdown();
                    }
                }
                catch (Exception e)
                {
                    if (OnConnectionNotify != null)
                    {
                        OnConnectionNotify(this, e.ToString());
                    }

                    tryShutdown();
                }
            }

            public virtual void Send(String data)
            {
                Send(Encoding.ASCII.GetBytes(data));
            }

            public virtual void Send(byte[] byteData)
            {
                try
                {
                    socket.BeginSend(byteData, 0, byteData.Length, 0,
                        new AsyncCallback(SendCallback), socket);
                }
                catch (Exception e)
                {
                    if (OnConnectionNotify != null)
                    {
                        OnConnectionNotify(this, e.ToString());
                    }
                }
            }

            private void SendCallback(IAsyncResult ar)
            {
                try
                {
                    int bytesSent = socket.EndSend(ar);
                }
                catch (Exception e)
                {
                    if (OnConnectionNotify != null)
                    {
                        OnConnectionNotify(this, e.ToString());
                    }
                }
            }

            private void tryShutdown()
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch(SocketException)
                {
                }

                socket.BeginDisconnect(false, new AsyncCallback(DisconnectCallback), this);
            }

            private void DisconnectCallback(IAsyncResult ar)
            {
                try
                {
                    socket.EndDisconnect(ar);
                }
                catch (Exception e)
                {
                    if (OnConnectionNotify != null)
                    {
                        OnConnectionNotify(this, e.ToString());
                    }
                }
                finally
                {
                    server.OnShutdown -= shutdownCallback;
                    socket.Close();
                }
            }
        }

        public class ServerConfiguration
        {
            public int port { get; set; }
            public int backLog { get; set; }
            public List<IPAddress> ipAddresses { get; set; }

            public ServerConfiguration()
            {
                ipAddresses = new List<IPAddress>();
                port = 8080;
                backLog = 100;
            }

            public String getIPsAsString()
            {
                const String CONCAT = " and ";
                StringBuilder sb = new StringBuilder();

                foreach (IPAddress ip in ipAddresses)
                {
                    sb.Append(ip.ToString());
                    sb.Append(":");
                    sb.Append(port);
                    sb.Append(CONCAT);
                }

                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - CONCAT.Length, CONCAT.Length);
                }

                return sb.ToString();
            }

            public void addIPAddressAsString(String ip)
            {
                ipAddresses.Insert(0, IPAddress.Parse(ip));
            }
        }
    }
}
