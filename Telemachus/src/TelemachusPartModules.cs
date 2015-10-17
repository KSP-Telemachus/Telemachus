//Author: Richard Bunt
using System;
using UnityEngine;
using KSP.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Collections;

namespace Telemachus
{
    public class TelemachusDataLink : PartModule
    {
        #region Fields

        [KSPEvent(guiActive = true, guiName = "Open Link")]
        public void openBrowser()
        {
            try
            {

                Application.OpenURL("http://" + TelemachusBehaviour.getServerPrimaryIPAddress() + ":"
                    + TelemachusBehaviour.getServerPort() + "/telemachus/information.html");

            }
            catch
            {
                PluginLogger.print(
                    "Unable to open the data link. Please try to edit the Telemachus configuration file manually");
            }
        }

        #endregion

        #region Part Events

        public override void OnAwake()
        {
            if (TelemachusBehaviour.instance == null)
            {
                TelemachusBehaviour.instance = GameObject.Find("TelemachusBehaviour")
                    ?? new GameObject("TelemachusBehaviour", typeof(TelemachusBehaviour));
            }

            base.OnAwake();
        }

        #endregion
    }

    public class TelemachusModuleAnimateGeneric : ModuleAnimateGeneric
    {
        #region Part Events

        public override void OnUpdate()
        {
            if (animSwitch == TelemachusPowerDrain.activeToggle)
            {
                if (status.Equals("Locked"))
                {
                    Toggle();
                }
            }

            foreach (BaseEvent theEvent in Events)
            {
                theEvent.guiActive = false;
            }

            base.OnUpdate();
        }

        #endregion
    }

    public class TelemachusPowerDrain : PartModule
    {
        #region Fields

        static string[] dataUnits = new string[] { "Error", " bit/s", " kbit/s", " Mbit/s", "Gbit/s" };

        static public bool isActive = true;

        //On by default
        [KSPField(isPersistant = true)]
        static public bool activeToggle = true;

        static public float powerConsumption = 0f;

        [KSPField]
        public float powerConsumptionIncrease = 0.02f;

        [KSPField]
        public float powerConsumptionBase = 0.02f;

        [KSPField(guiActive = true, guiName = "Data Link Status")]
        string statusString = "Disabled";

        [KSPField(guiActive = true, guiName = "Power Consumption")]
        string activeReading = "";

        [KSPField(guiActive = true, guiName = "Up Link Rate")]
        string uplinkReading = "";

        [KSPField(guiActive = true, guiName = "Down Link Rate")]
        string downlinkReading = "";

        [KSPEvent(guiActive = true, guiName = "Enable/Disable Data Link")]
        public void togglePower()
        {
            if (activeToggle)
            {
                statusString = "Disabled";
                activeToggle = false;
            }
            else
            {

                activeToggle = true;
            }
        }

        #endregion

        #region Part Events

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            if (!(state == StartState.Editor || state == StartState.None))
            {
                FlyByWireDataLinkHandler.reset();
                vessel.OnFlyByWire -= FlyByWireDataLinkHandler.onFlyByWire;
                vessel.OnFlyByWire += FlyByWireDataLinkHandler.onFlyByWire;
            }
        }

        public override void OnUpdate()
        {
            if (part.vessel != FlightGlobals.ActiveVessel) {
                return;
            }
            if (activeToggle)
            {
                float requiredPower = powerConsumption * TimeWarp.deltaTime;
                float availPower = part.RequestResource("ElectricCharge", requiredPower);

                if (availPower < requiredPower)
                {
                    statusString = "Insufficient power";
                    isActive = false;
                    telemachusInactive();
                }
                else
                {
                    statusString = "Enabled";
                    isActive = true;
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
            uplinkReading = formatBitRate(TelemachusBehaviour.getUpLinkRate());
            downlinkReading = formatBitRate(TelemachusBehaviour.getDownLinkRate());
        }

        private string formatBitRate(double bitRate)
        {
            int index = 1;
            powerConsumption = powerConsumptionBase;

            while (bitRate > 1000)
            {
                bitRate = bitRate / 1000;
                index++;
                powerConsumption += powerConsumptionIncrease;
            }

            if (index >= dataUnits.Length)
            {
                index = 0;
            }

            return Math.Round(bitRate, 2) + dataUnits[index];
        }

        #endregion
    }
}

