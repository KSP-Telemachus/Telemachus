using KSP.IO;
using MinimalHTTPServer;
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
        private VesselListChangeDetector vesselListChangeDetector = new VesselListChangeDetector();
        private VesselCacheMonitor vesselCacheMonitor = new VesselCacheMonitor();
        #endregion

        #region Data Link

        private static Server server = null;
        private static PluginConfiguration config = PluginConfiguration.CreateForType<TelemachusBehaviour>();
        private static ServerConfiguration serverConfig = new ServerConfiguration();

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
                    PluginLogger.Out("Telemachus data link starting");

                    readConfiguration();

                    server = new Server(serverConfig);
                    server.OnServerNotify += new Server.ServerNotify(serverOut);
                    server.addHTTPResponsibility(new ElseResponsibility());
                    server.addHTTPResponsibility(new IOPageResponsibility());
 
                    //server.addHTTPResponsibility(dataLinkResponsibility);
                    server.addHTTPResponsibility(new InformationResponsibility());
                    server.startServing();
                 
                    PluginLogger.Out("Telemachus data link listening for requests on the following addresses: ("
                        + server.getIPsAsString() +
                        "). Try putting them into your web browser, some of them might not work.");
                }
                catch (Exception e)
                {
                    PluginLogger.Out(e.Message);
                    PluginLogger.Out(e.StackTrace);
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
                PluginLogger.Log("No port in configuration file.");
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
                    PluginLogger.Log("Invalid IP address in configuration file, falling back to find.");
                }
            }
            else
            {
                PluginLogger.Log("No IP address in configuration file.");
            }
        }

        static private void stopDataLink()
        {
            if (server != null)
            {
                PluginLogger.Out("Telemachus data link shutting down.");
                server.stopServing();
                server = null;
            }
        }

        static private void serverOut(String message)
        {
            PluginLogger.Log(message);
        }

        #endregion

        #region Vessel List Changed Detector

        void vesselListChangeDetector_ColdChanged(int count)
        {
            PluginLogger.Log("Vessel List Changed Cold: " + count.ToString());
            vesselCacheMonitor.reBuildCache();
        }

        void vesselListChangeDetector_WarmChanged(int count)
        {
            PluginLogger.Log("Vessel List Changed Warm: " + count.ToString());
            vesselCacheMonitor.reBuildCache();
        }

        #endregion

        #region Behaviour Events

        public void Awake()
        {
            DontDestroyOnLoad(this);
            vesselListChangeDetector.ColdChanged += vesselListChangeDetector_ColdChanged;
            vesselListChangeDetector.WarmChanged += vesselListChangeDetector_WarmChanged;
            startDataLink();
        }

       

        public void Update()
        {
            vesselListChangeDetector.Update();

            vesselCacheMonitor.debugContents();
        }

        #endregion

        #region DataRate

        static public double getDownLinkRate()
        {
            return 0;
        }

        static public double getUpLinkRate()
        {
            return 0;
        }

        #endregion
    }

    class VesselCacheMonitor
    {
        #region Fields
        
        Dictionary<uint, Part> partCache =
           new Dictionary<uint, Part>();

        #endregion

        #region Cache Access

        public Vessel getVesselFromPartUid(uint id)
        {
            Part part = null;
           
            partCache.TryGetValue(id, out part);

            if (part == null)
            {
                return null;
            }
            else
            {
                return part.vessel;
            }
        }

        public void reBuildCache()
        {
            partCache.Clear();
            PluginLogger.Log("Building cache");
            
            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                vessel.i
                PluginLogger.Log("Number of parts: " + vessel.parts.Count);
                foreach (Part part in vessel.Parts)
                {
                    PluginLogger.Log("part: " + part.partInfo.name);
                    if (part.partInfo.name.StartsWith("Telemachus"))
                    {
                        PluginLogger.Log("Part Cache: " + part.uid);
                        partCache.Add(part.uid, part);
                    }
                }
            }
        }

        private bool containsTelemachusModule(Part part)
        {
            foreach (PartModule module in part.Modules)
            {
                if(module.GetType().Equals(typeof(TelemachusDataLink)))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        public void debugContents()
        {
            Dictionary<uint, Part>.Enumerator e = partCache.GetEnumerator();
            while (e.MoveNext())
            {
                Part p = e.Current.Value;
                if (p != null)
                {
                    //PluginLogger.Out(p.vessel.id + " " + p.uid + " " + p.vessel.missionTime.ToString() + " " + p.vessel.altitude.ToString());
                }
                else
                {
                    //PluginLogger.Out(e.Current.Key + " Part does not exist");
                }
            }
        }
    }

    class VesselListChangeDetector
    {
        #region Delegates

        public delegate void VesselListChange(int count);

        #endregion

        #region Events

        public event VesselListChange ColdChanged;
        public event VesselListChange WarmChanged;

        #endregion

        #region Fields

        private int previousSize = 0;

        #endregion

        #region Fire Event

        public void Update()
        {
            int nextSize = 0;

            if (FlightGlobals.fetch != null)
            {
                nextSize = FlightGlobals.Vessels.Count;
            }

            if (nextSize != previousSize)
            {
                if (previousSize == 0)
                {
                    if (ColdChanged != null)
                    {
                        ColdChanged(nextSize);
                    }
                }
                else
                {
                    if (WarmChanged != null)
                    {
                        WarmChanged(nextSize);
                    }
                }
            }

            previousSize = nextSize;
        }

        #endregion
    }
}
