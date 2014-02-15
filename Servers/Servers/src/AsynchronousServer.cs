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

            public event EventHandler<NotifyEventArgs> ServerNotify;
            public event EventHandler<EventArgs> ServerShutdown;

            protected virtual void OnServerNotify(NotifyEventArgs e)
            {
                EventHandler<NotifyEventArgs> handler = ServerNotify;

                if(handler != null)
                {
                    ServerNotify(this, e);
                }
            }

            protected virtual void OnServerShutdown(EventArgs e)
            {
                EventHandler<EventArgs> handler = ServerShutdown;

                if (handler != null)
                {
                    ServerShutdown(this, e);
                }
            }

            public event EventHandler<ConnectionEventArgs> ConnectionReceived;

            protected virtual void OnConnectionReceived(ConnectionEventArgs e)
            {
                EventHandler<ConnectionEventArgs> handler = ConnectionReceived;

                if (handler != null)
                {
                    ConnectionReceived(this, e);
                }
            }

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
                        OnServerNotify(new NotifyEventArgs("Trying user defined address."));
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
                                OnServerNotify(new NotifyEventArgs("Trying internal addresses."));
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
                        OnServerNotify(new NotifyEventArgs("Trying localhost."));
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
                    OnServerNotify(new NotifyEventArgs("Waiting for a connection."));
                }
            }

            public void stopListening()
            {
                foreach (Socket listener in listeners)
                {
                    listener.Close();
                }

                OnServerShutdown(new EventArgs());
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

                        OnConnectionReceived(new ConnectionEventArgs(clientConnectionBuilder(handler, this)));
                }
                catch (Exception e)
                {
                    OnServerNotify(new NotifyEventArgs(e.ToString()));
                }
            }
        }

        public class ClientConnection
        {
            public event EventHandler<ConnectionNotifyEventArgs> ConnectionNotify;
            public event EventHandler<ConnectionEventArgs> ConnectionRead;
            public event EventHandler<ConnectionEventArgs> ConnectionEmptyRead;

            protected virtual void OnConnectionNotify(ConnectionNotifyEventArgs e)
            {
                EventHandler<ConnectionNotifyEventArgs> handler = ConnectionNotify;

                if (handler != null)
                {
                    ConnectionNotify(this, e);
                }
            }

            protected virtual void OnConnectionRead(ConnectionEventArgs e)
            {
                EventHandler<ConnectionEventArgs> handler = ConnectionRead;

                if (handler != null)
                {
                    ConnectionRead(this, e);
                }
            }

            protected virtual void OnConnectionEmptyRead(ConnectionEventArgs e)
            {
                EventHandler<ConnectionEventArgs> handler = ConnectionEmptyRead;

                if (handler != null)
                {
                    ConnectionEmptyRead(this, e);
                }
            }

            Socket socket = null;
            
            public ArraySegment<byte> message { get; set; }
            private byte[] buffer = null;

            private Server server = null;
            private EventHandler<EventArgs> shutdownCallback = null;

            public ClientConnection(Socket socket, Server server)
            {
                this.socket = socket;
                this.server = server;

                buffer = new byte[server.configuration.bufferSize];

                shutdownCallback = requestShutdown;
                server.ServerShutdown += shutdownCallback;
            }

            ~ClientConnection()
            {
                OnConnectionNotify(new ConnectionNotifyEventArgs("Cleaning up client connection.", this));
            }

            public void startConnection()
            {
                try
                {
                    socket.BeginReceive(buffer, 0, server.configuration.bufferSize, 0,
                       new AsyncCallback(ReadCallback), this);
                }
                catch (Exception e)
                {
                    OnConnectionNotify(new ConnectionNotifyEventArgs(e.ToString(), this));
                }
            }

            private void requestShutdown(Object sender, EventArgs e)
            {
                OnShutdown(e);
            }

            protected virtual void OnShutdown(EventArgs e)
            {
                if (socket != null)
                {
                    socket.Close();
                }
            }

            private void ReadCallback(IAsyncResult ar)
            {
                try
                {
                    String content = String.Empty;

                    int bytesRead = socket.EndReceive(ar);

                    if (bytesRead > 0)
                    {
                        OnConnectionNotify(new ConnectionNotifyEventArgs(bytesRead.ToString(), this));

                        message = new ArraySegment<byte>(buffer, 0, bytesRead);
                        buffer = new byte[server.configuration.bufferSize];
                        OnConnectionRead(new ConnectionEventArgs(this));

                        if (socket != null)
                        {
                            socket.BeginReceive(buffer, 0, server.configuration.bufferSize, 0,
                            new AsyncCallback(ReadCallback), this);
                        }
                    }
                    else
                    {
                        OnConnectionEmptyRead(new ConnectionEventArgs(this));
                    }
                }
                catch (Exception e)
                {
                    OnConnectionNotify(new ConnectionNotifyEventArgs(e.ToString() + "\n" + e.StackTrace, this));
                    tryShutdown();
                }
            }

            public virtual void Send(String data)
            {
                Send(Encoding.ASCII.GetBytes(data));
            }

            public virtual void Send(byte[] byteData)
            {
                if (socket != null)
                {
                    try
                    {
                        socket.BeginSend(byteData, 0, byteData.Length, 0,
                            new AsyncCallback(SendCallback), socket);
                    }
                    catch (Exception ex)
                    {
                        // Let the original caller decide how to handle the error.
                        OnConnectionNotify(new ConnectionNotifyEventArgs(ex.Message, this)); 
                        throw;
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
                    OnConnectionNotify(new ConnectionNotifyEventArgs(e.ToString(), this));
                }
            }

            public void tryShutdown()
            {
                if (socket != null)
                {
                    try
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }
                    catch (SocketException ex)
                    {
                        OnConnectionNotify(new ConnectionNotifyEventArgs(ex.Message, this)); 
                    }
                    catch (Exception ex)
                    {
                        OnConnectionNotify(new ConnectionNotifyEventArgs(ex.Message, this));
                    }

                    try
                    {
                        socket.BeginDisconnect(false, new AsyncCallback(DisconnectCallback), this);
                    }
                    catch(Exception ex)
                    {
                        OnConnectionNotify(new ConnectionNotifyEventArgs(ex.Message, this)); 
                    }
                }
            }

            private void DisconnectCallback(IAsyncResult ar)
            {
                try
                {
                    socket.EndDisconnect(ar);
                }
                catch (Exception e)
                {
                    OnConnectionNotify(new ConnectionNotifyEventArgs(e.ToString(), this));
                }
                finally
                {
                    server.ServerShutdown -= shutdownCallback;
                    socket.Close();
                }
            }

            public Socket stealSocket()
            {
                Socket temp = socket;
                socket = null;
                server.ServerShutdown -= shutdownCallback;
                return temp;
            }
        }

        public class ServerConfiguration
        {
            public int port { get; set; }
            public int backLog { get; set; }
            public virtual int bufferSize { get; set; }
            public List<IPAddress> ipAddresses { get; set; }

            public ServerConfiguration()
            {
                ipAddresses = new List<IPAddress>();
                port = 8085;
                backLog = 100;
                bufferSize = 512;
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

    public class ConnectionEventArgs : EventArgs
    {
        public AsynchronousServer.ClientConnection clientConnection {get; set;}

        public ConnectionEventArgs(AsynchronousServer.ClientConnection clientConnection)
        {
            this.clientConnection = clientConnection;
        }
    }

    public class NotifyEventArgs : EventArgs
    {
        public String message {get; set;}

        public NotifyEventArgs(String message)
        {
            this.message = message;
        }
    }

    public class ConnectionNotifyEventArgs : ConnectionEventArgs
    {
        public String message { get; set; }

        public ConnectionNotifyEventArgs(String message,
            AsynchronousServer.ClientConnection clientConnection)
            : base(clientConnection)
        {
            this.message = message;
        }
    }
}
