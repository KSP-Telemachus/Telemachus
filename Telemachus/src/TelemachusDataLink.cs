using System;
using UnityEngine;
using KSP.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using MinimalHTTPServer;
using System.Reflection;

namespace Telemachus
{
    public class TelemachusDataLink : Part
    {
        static Server server = null;
        static DataLinkResponsibility dataLinkResponsibility = null;
        PluginConfiguration config = PluginConfiguration.CreateForType<TelemachusDataLink>();
        ServerConfiguration serverConfig = new ServerConfiguration();

        protected override void onFlightStart()
        {
            startDataLink(); 
            base.onFlightStart();
        }

        protected override void onDisconnect()
        {
            stopDataLink();
            base.onDisconnect();
        }

        protected override void onPartDestroy()
        {
            stopDataLink();
            base.onPartDestroy();
        }

        private void startDataLink()
        {
            if (server == null)
            {
                try
                {
                    Logger.Out("Telemachus data link starting");
                    
                    readConfiguration();
                 
                    server = new Server(serverConfig);
                    server.OnServerNotify += new Server.ServerNotify(serverOut);
                    server.addHTTPResponsibility(new ElseResponsibility());
                    server.addHTTPResponsibility(new IOPageResponsibility());
                    DataLink dataLinks = new DataLink();
                    dataLinks.vessel = this.vessel;
                    dataLinks.orbit = this.vessel.orbit;
                    dataLinks.pdl = new PausedDataLink();
                    dataLinks.fcdl = new FlightControlDataLink(this);
                    dataLinkResponsibility = new DataLinkResponsibility(dataLinks);
                    server.addHTTPResponsibility(dataLinkResponsibility);
                    server.addHTTPResponsibility(new InformationResponsibility(dataLinkResponsibility));
                    server.startServing();

                    Logger.Out("Telemachus data link listening for requests on the following addresses: (" 
                        + server.getIPsAsString() + 
                        "). Try putting them into your web browser, some of them might not work.");
    
                }
                catch (Exception e)
                {
                    Logger.Out(e.Message);
                    Logger.Out(e.StackTrace);
                }
            }
        }

        private void writeDefaultConfig()
        {
            config.SetValue("PORT", 8080);
            config.SetValue("IPADDRESS", "127.0.0.1");
            config.save();
        }

        private void readConfiguration()
        {
            config.load();

            int port = config.GetValue<int>("PORT");

            if (port != 0)
            {
                serverConfig.port = port;
            }
            else
            {
                Logger.Log("No port in configuration file.");
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
                    Logger.Log("Invalid IP address in configuration file, falling back to find.");
                }
            }
            else
            {
                Logger.Log("No IP address in configuration file.");
            }
        }

        private void stopDataLink()
        {
            if (server != null)
            {
                Logger.Out("Telemachus data link shutting down.");
                server.stopServing();
                server = null;
                dataLinkResponsibility.clearCache();
            }
        }

        private void serverOut(String message)
        {
            Logger.Log(message);
        }
    }

    public class DataLink
    {
        public Vessel vessel;
        public Orbit orbit;
        public PausedDataLink pdl;
        public FlightControlDataLink fcdl;
    }
}

