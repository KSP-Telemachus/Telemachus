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

                    ServerConfiguration config = new ServerConfiguration();
                    server = new Server(config);
                    server.OnServerNotify += new Server.ServerNotify(serverOut);
                    server.addHTTPResponsibility(new ElseResponsibility());
                    server.addHTTPResponsibility(new TelemachusResponsibility());
                    DataLinks dataLinks = new DataLinks();
                    dataLinks.vessel = this.vessel;
                    dataLinks.orbit = this.vessel.orbit;
                    dataLinkResponsibility = new DataLinkResponsibility(dataLinks);
                    server.addHTTPResponsibility(new DataLinkResponsibility(dataLinks));
                    server.addHTTPResponsibility(new InformationResponsibility());
                    server.startServing();

                    Logger.Out("Telemachus data link listening for requests on: " + server.getIP() + ":" + config.port.ToString());
    
                }
                catch (Exception e)
                {
                    Logger.Out(e.Message);
                    Logger.Out(e.StackTrace);
                }
            }
        }

        private void stopDataLink()
        {
            if (server != null)
            {
                Logger.Out("Telemachus data link shutting down.");
                server.stopServing();
                server = null;
                dataLinkResponsibility.clear();
            }
        }

        private void serverOut(String message)
        {
            Logger.Log(message);
        }
    }
}

