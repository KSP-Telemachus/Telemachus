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
       
        #region Part Events

        protected override void onFlightStart()
        {
            ScenarioRunner.GetLoadedModules().Add(new TelemachusDataLinkScenario());
            //startDataLink();
            base.onFlightStart();
        }

        protected override void onDisconnect()
        { 
            base.onDisconnect();
        }

        protected override void onPartDestroy()
        {

            //stopDataLink();
            

            base.onPartDestroy();
        }

        #endregion

        private void getTelemachusScenarioModule()
        {
            foreach(ScenarioModule s in ScenarioRunner.GetLoadedModules())
            {

            }

        }
    }

    public class TelemachusPowerDrain : PartModule
    {

        static string[] dataUnits = new string[] { "Error", " bit/s", " kbit/s", " Mbit/s", "Gbit/s" };

        #region Fields

        static public bool isActive = true, activeToggle = true;

        static public float powerConsumption = 0f;

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
                    isActive = false;
                    telemachusInactive();
                }
                else
                {
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
            uplinkReading = formatBitRate(TelemachusDataLinkScenario.dataRates.getUpLinkRate());
            downlinkReading = formatBitRate(TelemachusDataLinkScenario.dataRates.getDownLinkRate());
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

