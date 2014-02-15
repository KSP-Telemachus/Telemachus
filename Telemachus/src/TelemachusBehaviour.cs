//Author: Richard Bunt
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
                    server.ServerNotify += HTTPServerNotify;
                    server.addHTTPResponsibility(new ElseResponsibility());
                    ioPageResponsibility = new IOPageResponsibility();
                    server.addHTTPResponsibility(ioPageResponsibility);

                    DataSources dataSources = new DataSources();
                    VesselChangeDetector vesselChangeDetector = new VesselChangeDetector();
                    IKSPAPI kspAPI = new KSPAPI(JSONFormatterProvider.Instance, vesselChangeDetector, serverConfig);
                    dataLinkResponsibility = new DataLinkResponsibility(serverConfig, kspAPI, dataSources);
                    server.addHTTPResponsibility(dataLinkResponsibility);

                    Servers.MinimalWebSocketServer.ServerConfiguration webSocketconfig = new Servers.MinimalWebSocketServer.ServerConfiguration();
                    webSocketconfig.bufferSize = 300;
                    Servers.MinimalWebSocketServer.Server webSocketServer = new Servers.MinimalWebSocketServer.Server(webSocketconfig);
                    webSocketServer.ServerNotify += WebSocketServerNotify;
                    webSocketServer.addWebSocketService("/datalink", new KSPWebSocketService(kspAPI, dataSources));
                    webSocketServer.subscribeToHTTPForStealing(server);

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
            config.SetValue("PORT", 8085);
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
            serverConfig.backLog = 1000;
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

        private static void HTTPServerNotify(object sender, Servers.NotifyEventArgs e)
        {
            PluginLogger.debug(e.message);
        }

        private static void WebSocketServerNotify(object sender, Servers.NotifyEventArgs e)
        {
            PluginLogger.print(e.message);
        }

        #endregion

        #region Behaviour Events

        public void Awake()
        {
            DontDestroyOnLoad(this);
            startDataLink();
        }

        public void OnDestroy()
        {
            stopDataLink();
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

        #region Lock

        readonly private System.Object queueLock = new System.Object();

        #endregion

        #region Methods

        public void execute()
        {
            lock (queueLock)
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
            lock (queueLock)
            {
                actionQueue.Add(delayedAPIEntry);
            }
        }

        #endregion
    }

    public class KSPAPI : IKSPAPI
    {
        public KSPAPI(FormatterProvider formatters, VesselChangeDetector vesselChangeDetector,
            Servers.AsynchronousServer.ServerConfiguration serverConfiguration)
        {
            APIHandlers.Add(new PausedDataLinkHandler(formatters));
            APIHandlers.Add(new FlyByWireDataLinkHandler(formatters));
            APIHandlers.Add(new FlightDataLinkHandler(formatters));
            APIHandlers.Add(new MechJebDataLinkHandler(formatters));
            APIHandlers.Add(new TimeWarpDataLinkHandler(formatters));
            APIHandlers.Add(new TargetDataLinkHandler(formatters));

            APIHandlers.Add(new CompoundDataLinkHandler(
                new List<DataLinkHandler>() { 
                    new OrbitDataLinkHandler(formatters),
                    new SensorDataLinkHandler(vesselChangeDetector, formatters),
                    new VesselDataLinkHandler(formatters),
                    new BodyDataLinkHandler(formatters),
                    new ResourceDataLinkHandler(vesselChangeDetector, formatters),
                    new APIDataLinkHandler(this, formatters, serverConfiguration),
                    new NavBallDataLinkHandler(formatters),
                    new MapViewDataLinkHandler(formatters),
                    new DockingDataLinkHandler(formatters)
                    }, formatters
                ));

            APIHandlers.Add(new DefaultDataLinkHandler(formatters));
        }
    }

    public abstract class IKSPAPI
    {
        protected List<DataLinkHandler> APIHandlers = new List<DataLinkHandler>();

        public void getAPIList(ref List<APIEntry> APIList)
        {
            foreach (DataLinkHandler APIHandler in APIHandlers)
            {
                APIHandler.appendAPIList(ref APIList);
            }

        }

        public void getAPIEntry(string APIString, ref List<APIEntry> APIList)
        {
            APIEntry result = null;

            foreach (DataLinkHandler APIHandler in APIHandlers)
            {
                if (APIHandler.process(APIString, out result))
                {
                    break;
                }
            }

            APIList.Add(result);
        }

        public void process(String API, out APIEntry apiEntry)
        {
            APIEntry result = null;
            foreach (DataLinkHandler APIHandler in APIHandlers)
            {
                if (APIHandler.process(API, out result))
                {
                    break;
                }
            }

            apiEntry = result;
        }
    }
}
