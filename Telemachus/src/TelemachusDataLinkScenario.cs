using KSP.IO;
using MinimalHTTPServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Telemachus
{
    public class TelemachusDataLinkScenario : ScenarioModule
    {
        #region Fields

        private static Server server = null;
        private static DataLinkResponsibility dataLinkResponsibility = null;
        private static PluginConfiguration config = PluginConfiguration.CreateForType<TelemachusDataLinkScenario>();
        private static ServerConfiguration serverConfig = new ServerConfiguration();

        public static UpLinkDownLinkRate dataRates
        {
            get
            {
                return dataLinkResponsibility.dataRates;
            }
        }

        #endregion

        public TelemachusDataLinkScenario()
        {
            startDataLink();
        }

        #region Data Link

        private void startDataLink()
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
                    DataLink dataLinks = new DataLink();
                    dataLinks.vessel = FlightGlobals.ActiveVessel;
                    dataLinks.orbit = FlightGlobals.ActiveVessel.orbit;
                    dataLinks.pdl = new PausedDataLink();
                    dataLinks.fcdl = new FlightControlDataLink(this);
                    dataLinkResponsibility = new DataLinkResponsibility(dataLinks);
                    server.addHTTPResponsibility(dataLinkResponsibility);
                    server.addHTTPResponsibility(new InformationResponsibility(dataLinkResponsibility));
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

        private void stopDataLink()
        {
            if (server != null)
            {
                PluginLogger.Out("Telemachus data link shutting down.");
                server.stopServing();
                server = null;
                dataLinkResponsibility.clearCache();
            }
        }

        private void serverOut(String message)
        {
            PluginLogger.Log(message);
        }

        #endregion

        #region Flight Control Invoke

        public void activateNextStage()
        {
            Staging.ActivateNextStage();
        }

        public void throttleUp()
        {

            FlightInputHandler.state.mainThrottle += 0.1f;

            if (FlightInputHandler.state.mainThrottle > 1)
            {
                FlightInputHandler.state.mainThrottle = 1;
            }
        }

        public void throttleDown()
        {
            FlightInputHandler.state.mainThrottle -= 0.1f;

            if (FlightInputHandler.state.mainThrottle < 0)
            {
                FlightInputHandler.state.mainThrottle = 0;
            }
        }

        #endregion

        ~TelemachusDataLinkScenario()
        {
            stopDataLink();
        }
    }
}
