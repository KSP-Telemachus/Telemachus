using KSP.IO;
using Servers.MinimalHTTPServer;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Telemachus
{
    class TelemachusBehaviour : MonoBehaviour
    {
        #region Fields

        public static GameObject instance;
        private DelayedAPIRunner delayedAPIRunner = new DelayedAPIRunner();

        #endregion

        #region Data Link

        private static Server server = null;
        private static PluginConfiguration config = PluginConfiguration.CreateForType<TelemachusBehaviour>();
        private static ServerConfiguration serverConfig = new ServerConfiguration();
        private static DataLinkResponsibility dataLinkResponsibility = null;
        private static IOPageResponsibility ioPageResponsibility = null;

        static public string getServerPrimaryIPAddress()
        {
            return serverConfig.ipAddresses[0].ToString();
        }

        static public string getServerPort()
        {
            return serverConfig.port.ToString();
        }

        static private void startDataLink()
        {
            if (server == null)
            {
                try
                {
                    PluginLogger.print("Telemachus data link starting");

                    readConfiguration();

                    server = new Server(serverConfig);
                    server.OnServerNotify += new Server.ServerNotify(serverOut);
                    server.addHTTPResponsibility(new ElseResponsibility());
                    ioPageResponsibility = new IOPageResponsibility();
                    server.addHTTPResponsibility(ioPageResponsibility);
                    dataLinkResponsibility = new DataLinkResponsibility();
                    server.addHTTPResponsibility(dataLinkResponsibility);
                    server.startServing();
                 
                    PluginLogger.print("Telemachus data link listening for requests on the following addresses: ("
                        + server.getIPsAsString() +
                        "). Try putting them into your web browser, some of them might not work.");
                }
                catch (Exception e)
                {
                    PluginLogger.print(e.Message);
                    PluginLogger.print(e.StackTrace);
                }
            }
        }

        static private void writeDefaultConfig()
        {
            config.SetValue("PORT", 8080);
            config.SetValue("IPADDRESS", "127.0.0.1");
            config.save();
        }

        static private void readConfiguration()
        {
            config.load();

            int port = config.GetValue<int>("PORT");

            if (port != 0)
            {
                serverConfig.port = port;
            }
            else
            {
                PluginLogger.print("No port in configuration file.");
            }

            String ip = config.GetValue<String>("IPADDRESS");

            if (ip != null)
            {
                try
                {
                    serverConfig.addIPAddressAsString(ip);
                }
                catch
                {
                    PluginLogger.print("Invalid IP address in configuration file, falling back to find.");
                }
            }
            else
            {
                PluginLogger.print("No IP address in configuration file.");
            }

            serverConfig.maxRequestLength = 8000;
            serverConfig.version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            serverConfig.name = "Telemachus";
        }

        static private void stopDataLink()
        {
            if (server != null)
            {
                PluginLogger.print("Telemachus data link shutting down.");
                server.stopServing();
                server = null;
            }
        }

        static private void serverOut(String message)
        {
            PluginLogger.debug(message);
        }

        #endregion

        #region Behaviour Events

        public void Awake()
        {
            DontDestroyOnLoad(this);

            startDataLink();
        }

        public void Update()
        {
            delayedAPIRunner.execute();
        }

        #endregion

        #region DataRate

        static public double getDownLinkRate()
        {
            return dataLinkResponsibility.dataRates.getDownLinkRate();
        }

        static public double getUpLinkRate()
        {
            return dataLinkResponsibility.dataRates.getUpLinkRate();
        }

        #endregion

        #region Delayed API Runner

        public void queueDelayedAPI(DelayedAPIEntry entry)
        {
            delayedAPIRunner.queue(entry);
        }

        #endregion
    }

    public class DelayedAPIRunner
    {
        #region Fields

        List<DelayedAPIEntry> actionQueue = new List<DelayedAPIEntry>();

        #endregion

        #region Methods

        public void execute()
        {
            lock (actionQueue)
            {
                foreach (DelayedAPIEntry entry in actionQueue)
                {
                    entry.call();
                }

                actionQueue.Clear();
            }
        }

        public void queue(DelayedAPIEntry delayedAPIEntry)
        {
             lock (actionQueue)
            {
                actionQueue.Add(delayedAPIEntry);
            }
        }

        #endregion
    }
}
