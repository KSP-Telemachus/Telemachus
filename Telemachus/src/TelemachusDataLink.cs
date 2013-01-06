//Author: Richard Bunt
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
        #region Fields

        static Server server = null;
        static DataLinkResponsibility dataLinkResponsibility = null;
        PluginConfiguration config = PluginConfiguration.CreateForType<TelemachusDataLink>();
        ServerConfiguration serverConfig = new ServerConfiguration();

        public UpLinkDownLinkRate dataRates 
        { 
            get 
            { 
                return dataLinkResponsibility.dataRates; 
            }
        }

        #endregion

        #region Part Events

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

        #endregion

        #region Data Link

        private void startDataLink()
        {
            if (server == null)
            {
                try
                {
                    PluginLogger.Out("Telemachus data link starting");
                    P
                    readConfiguration();

                    server = new Server(serverConfig);
                    server.OnServerNotify += new Server.ServerNotify(serverOut);
                    server.addHTTPResponsibility(new ElseResponsibility());
                    server.addHTTPResponsibility(new IOPageResponsibility());
                    DataLink dataLinks = new DataLink();
                    dataLinks.vessel = this.vessel;
                    dataLinks.orbit = this.vessel.orbit;
                    dataLinks.pdl = new PausedDataLink(getPowerDrainModule());
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

        public TelemachusPowerDrain getPowerDrainModule()
        {
            foreach (var o in this.Modules)
            {
                if (typeof(TelemachusPowerDrain) == (o.GetType()))
                {
                    return (TelemachusPowerDrain)o;
                }
            }
            
            return null;
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

            if(FlightInputHandler.state.mainThrottle > 1)
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
    }

    public class TelemachusPowerDrain : PartModule
    {

        string[] dataUnits = new string[] { "Error", " bit/s", " kbit/s", " Mbit/s", "Gbit/s" };

        #region Fields

        public bool active = true, activeToggle = true;

        public float powerConsumption = 0f;

        [KSPField]
        public float powerConsumptionIncrease = 0.02f;
        
        [KSPField]
        public float powerConsumptionBase = 0.02f;

        [KSPField(guiActive = true, guiName = "Power Consumption")]
        string activeReading = "";

        [KSPField(guiActive = true, guiName = "Uplink Rate")]
        string uplinkReading = "";

        [KSPField(guiActive = true, guiName = "Downlink Rate")]
        string downlinkReading = "";

        [KSPEvent(guiActive = true, guiName = "Enable/Disable Data Link")]
        public void togglePower()
        {
            if (activeToggle)
            {
                activeToggle = false;
            }
            else
            {
                activeToggle = true;
            }
        }

        #endregion

        #region Part Events

        public override void OnUpdate()
        {
            if (activeToggle)
            {
                float requiredPower = powerConsumption * TimeWarp.deltaTime;
                float availPower = part.RequestResource("ElectricCharge", requiredPower);

                if (availPower < requiredPower)
                {
                    active = false;
                    telemachusInactive();
                }
                else
                {
                    active = true;
                    telemachusActive();
                }
            }
            else
            {
                 telemachusInactive();
            }
        }

        #endregion

        #region GUI Update

        private void telemachusInactive()
        {
            activeReading = "0 units";
            uplinkReading = "0" + dataUnits[1];
            downlinkReading = "0" + dataUnits[1];
        }

        private void telemachusActive()
        {
            activeReading = powerConsumption + " units";

            TelemachusDataLink tdl = (TelemachusDataLink)part;
            uplinkReading = formatBitRate(tdl.dataRates.getUpLinkRate());
            downlinkReading = formatBitRate(tdl.dataRates.getDownLinkRate());
        }

        private string formatBitRate(double bitRate)
        {
            int index = 1;
            powerConsumption = powerConsumptionBase;

            while(bitRate > 1000)
            {
                bitRate = bitRate / 1000;
                index++;
                powerConsumption += powerConsumptionIncrease;
            }

            if(index >= dataUnits.Length)
            {
                index = 0;
            }

            return Math.Round(bitRate, 2) + dataUnits[index];
        }

        #endregion
    }

    public class DataLink
    {
        #region Fields

        public Vessel vessel;
        public Orbit orbit;
        public PausedDataLink pdl;
        public FlightControlDataLink fcdl;

        #endregion
    }
}

