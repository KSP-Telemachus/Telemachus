//Author: Richard Bunt
using KSP.IO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Timers;
using UnityEngine;
using WebSocketSharp.Server;

namespace Telemachus
{
    class TelemachusBehaviour : MonoBehaviour
    {
        #region Fields

        public static GameObject instance;
        private DelayedAPIRunner delayedAPIRunner = new DelayedAPIRunner();

        #endregion

        #region Data Link

        private static HttpServer webServer = null;
        private static KSPWebServerDispatcher webDispatcher = null;
        private static IKSPAPI apiInstance = null;

        private static PluginConfiguration config = PluginConfiguration.CreateForType<TelemachusBehaviour>();
        private static ServerConfiguration serverConfig = new ServerConfiguration();
        private static VesselChangeDetector vesselChangeDetector = null;

        // Create a default plugin manager to handle registrations
        private static PluginManager pluginManager = new PluginManager();

        // Keep a list of handlers of the data uplink/downlink rate
        private static UpLinkDownLinkRate rateTracker = new UpLinkDownLinkRate();

        private static bool isPartless = false;

        static public string getServerPrimaryIPAddress()
        {
            return serverConfig.ValidIpAddresses.First().ToString();
        }

        static public string getServerPort()
        {
            return serverConfig.port.ToString();
        }

        static private void startDataLink()
        {
            if (webServer == null)
            {
                try
                {
                    PluginLogger.print("Telemachus data link starting");

                    readConfiguration();

                    // Data access tools
                    vesselChangeDetector = new VesselChangeDetector(isPartless);
                    apiInstance = new KSPAPI(JSONFormatterProvider.Instance, vesselChangeDetector, serverConfig, pluginManager);

                    // Create the dispatcher and handlers. Handlers added in reverse priority order so that new ones are not ignored.
                    webDispatcher = new KSPWebServerDispatcher();
                    webDispatcher.AddResponder(new ElseResponsibility());
                    webDispatcher.AddResponder(new IOPageResponsibility());
                    var cameraLink = new CameraResponsibility(apiInstance, rateTracker);
                    webDispatcher.AddResponder(cameraLink);
                    var dataLink = new DataLinkResponsibility(apiInstance, rateTracker);
                    webDispatcher.AddResponder(dataLink);

                    // Create the server and associate the dispatcher
                    webServer = new HttpServer(serverConfig.ipAddress, serverConfig.port);
                    webServer.OnGet += webDispatcher.DispatchRequest;
                    webServer.OnPost += webDispatcher.DispatchRequest;

                    // Create the websocket server and attach to the web server
                    webServer.AddWebSocketService("/datalink", () => new KSPWebSocketService(apiInstance, rateTracker));

                    // Finally, start serving requests!
                    try {
                        webServer.Start();
                    } catch (Exception ex)
                    {
                        PluginLogger.print("Error starting web server: " + ex.ToString());
                        throw;
                    }

                    PluginLogger.print("Telemachus data link listening for requests on the following addresses: ("
                        + string.Join(", ", serverConfig.ValidIpAddresses.Select(x => x.ToString() + ":" + serverConfig.port.ToString()).ToArray())
                        + "). Try putting them into your web browser, some of them might not work.");
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

            // Read the port out of the config file
            int port = config.GetValue<int>("PORT");
            if (port != 0 && port.IsPortNumber()) {
                serverConfig.port = port;
            } else if (!port.IsPortNumber()) {
                PluginLogger.print("Port specified in configuration file '" + serverConfig.port + "' must be a value between 1 and 65535 inclusive");
            } else {
                PluginLogger.print("No port in configuration file - using default of " + serverConfig.port.ToString());
            }

            // Read a specific IP address to bind to
            string ip = config.GetValue<String>("IPADDRESS");
            if (ip != null)
            {
                IPAddress ipAddress = null;
                if (IPAddress.TryParse(ip, out ipAddress))
                {
                    serverConfig.ipAddress = ipAddress;
                } else {
                    PluginLogger.print("Invalid IP address in configuration file, falling back to default");
                }
            }
            else
            {
                PluginLogger.print("No IP address in configuration file.");
            }

            // Fill the serverconfig list of addresses.... if IPAddress.Any, then enumerate them
            if (serverConfig.ipAddress == IPAddress.Any)
            {
                // Build a list of addresses we will be able to recieve at
                serverConfig.ValidIpAddresses.Add(IPAddress.Loopback);
                serverConfig.ValidIpAddresses.AddRange(Dns.GetHostAddresses(Dns.GetHostName()));
            } else
            {
                serverConfig.ValidIpAddresses.Add(serverConfig.ipAddress);
            }

            serverConfig.version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            serverConfig.name = "Telemachus";

            isPartless = config.GetValue<int>("PARTLESS") == 0 ? false : true;
            PluginLogger.print("Partless:" + isPartless);
        }

        static private void stopDataLink()
        {
            if (webServer != null)
            {
                PluginLogger.print("Telemachus data link shutting down.");
                webServer.Stop();
                webServer = null;
            }
        }

        #endregion

        #region Behaviour Events

        public void Awake()
        {
            SimpleJson.SimpleJson.CurrentJsonSerializerStrategy = new SimpleJson.InfinityAsStringJsonSerializerStrategy();
            
            LookForModsToInject();
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

            if (FlightGlobals.fetch != null)
            {
                vesselChangeDetector.update(FlightGlobals.ActiveVessel);

                foreach (var client in webServer.WebSocketServices["/datalink"].Sessions.Sessions.OfType<KSPWebSocketService>()) 
                {
                    if (client.UpdateRequired(Time.time))
                    {
                        client.SendDataUpdate();
                    }
                }
            }
            else
            {
                PluginLogger.debug("Flight globals was null during start up; skipping update of vessel change.");
            }
        }


        void LookForModsToInject()
        {
            string foundMods = "Loading; Looking for compatible mods to inject registration....\nTelemachus compatible modules Found:\n";
            int found = 0;
            foreach (var asm in AssemblyLoader.loadedAssemblies)
            {
                foreach (var type in asm.assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(MonoBehaviour)))
                    {
                        // Does this have a static property named "Func<string> TelemachusPluginRegister { get; set; }?
                        var prop = type.GetProperty("TelemachusPluginRegister", BindingFlags.Static | BindingFlags.Public);
                        if (prop == null) continue;
                        found += 1;
                        foundMods += "  - " + type.ToString() + " ";
                        if (prop.PropertyType != typeof(Action<object>))
                        {
                            foundMods += "(Fail - Invalid property type)\n";
                            continue;
                        }

                        if (!prop.CanWrite)
                        {
                            foundMods += "(Fail - Property not writeable)\n";
                            continue;
                        }
                        // Can we read it - if so, only write if it is not null.
                        if (prop.CanRead)
                        {
                            if (prop.GetValue(null, null) != null)
                            {
                                foundMods += "(Fail - Property not null)\n";
                                continue;
                            }
                        }
                        // Write the value here
                        Action<object> pluginRegister = PluginRegistration.Register;
                        prop.SetValue(null, pluginRegister, null);
                        foundMods += "(Success)\n";
                    }
                }
            }
            if (found == 0) foundMods += "  None\n";

            foundMods += "Internal plugins loaded:\n";
            found = 0;
            // Look for any mods in THIS assembly that inherit ITelemachusMinimalPlugin...
            foreach (var typ in Assembly.GetExecutingAssembly().GetTypes())
            {
                try {
                    if (!typeof(IMinimalTelemachusPlugin).IsAssignableFrom(typ)) continue;
                    // Make sure we have a default constructor
                    if (typ.GetConstructor(Type.EmptyTypes) == null) continue;
                    // We have found a plugin internally. Instantiate it 
                    PluginRegistration.Register(Activator.CreateInstance(typ));

                    foundMods += "  - " + typ.ToString() + "\n";
                    found += 1;
                } catch (Exception ex)
                {
                    PluginLogger.print("Exception caught whilst loading internal plugin " + typ.ToString() + "; " + ex.ToString());
                }
            }
            if (found == 0) foundMods += "  None";
            PluginLogger.print(foundMods);
        }
        #endregion

        #region DataRate

        static public double getDownLinkRate()
        {
            return rateTracker.getDownLinkRate();
        }

        static public double getUpLinkRate()
        {
            return rateTracker.getUpLinkRate();
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
        private PluginManager _manager;

        public KSPAPI(FormatterProvider formatters, VesselChangeDetector vesselChangeDetector,
            ServerConfiguration serverConfiguration, PluginManager manager)
        {
            _manager = manager;

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
        }

        public override Vessel getVessel()
        {
            return FlightGlobals.ActiveVessel;
        }

        public override object ProcessAPIString(string apistring)
        {
            var data = new DataSources() { vessel = getVessel() };
            // Extract any arguments/parameters in this API string
            var name = apistring;
            parseParams(ref name, ref data);

            // Are we in flight mode, with a vessel?
            var cleanFlightMode = HighLogic.LoadedSceneIsFlight && data.vessel != null;

            try {
                // Get the API entry
                APIEntry apiEntry = null;
                process(name, out apiEntry);
                if (apiEntry == null) return null;

                // Can we run this variable at the moment?
                if (!apiEntry.alwaysEvaluable && !cleanFlightMode) {
                    if (data.vessel == null) throw new VariableNotEvaluable(apistring, "No vessel!");
                    throw new VariableNotEvaluable(apistring, "Not in flight mode");
                }

                // run the API entry
                var result = apiEntry.function(data);
                // And return the serialization-ready value
                return apiEntry.formatter.prepareForSerialization(result);
            } catch (UnknownAPIException)
            {
                if (!cleanFlightMode) throw new VariableNotEvaluable(apistring, "Plugin variables not evaluable outside flight scene with vessel");

                // Try looking in the pluginManager
                var pluginAPI = _manager.GetAPIDelegate(name);
                // If no entry, just continue the throwing of the exception
                if (pluginAPI == null) throw;

                // We found an API entry! Let's use that.
                return pluginAPI(data.vessel, data.args.ToArray());
            }
        }
    }

    public abstract class IKSPAPI
    {
        public class UnknownAPIException : ArgumentException
        {
            public string apiString = "";

            public UnknownAPIException(string apiString = "")
            {
                this.apiString = apiString;
            }

            public UnknownAPIException(string message, string apiString = "")
                : base(message)
            {
                this.apiString = apiString;
            }

            public UnknownAPIException(string message, string apiString, Exception inner)
                : base(message, inner)
            {
                this.apiString = apiString;
            }
        }

        public class VariableNotEvaluable : Exception
        {
            public VariableNotEvaluable(string apiString, string reason)
                : base("Cannot run " + apiString + ";" + reason)
            {

            }
        }
        
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
            if (result == null) throw new UnknownAPIException("Could not find API entry named " + API, API);
            apiEntry = result;
        }

        abstract public Vessel getVessel();

        public void parseParams(ref String arg, ref DataSources dataSources)
        {
            dataSources.args.Clear();

            try
            {
                if (arg.Contains("["))
                {
                    String[] argsSplit = arg.Split('[');
                    argsSplit[1] = argsSplit[1].Substring(0, argsSplit[1].Length - 1);
                    arg = argsSplit[0];
                    String[] paramSplit = argsSplit[1].Split(',');

                    for (int i = 0; i < paramSplit.Length; i++)
                    {
                        dataSources.args.Add(paramSplit[i]);
                    }
                }
            }
            catch (Exception e)
            {
                PluginLogger.debug(e.Message + " " + e.StackTrace);
            }
        }

        /// <summary>
        /// Accepts a string, and does any API processing (with the current vessel), returning the result.
        /// </summary>
        /// <remarks>This take responsibility for the whole chain of parsing, splitting and searching for the API</remarks>
        /// <param name="apistring"></param>
        /// <returns></returns>
        public abstract object ProcessAPIString(string apistring);
    }
}
