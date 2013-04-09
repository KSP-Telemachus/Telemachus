//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MinimalHTTPServer;
using System.Threading;
using System.Collections;

namespace Telemachus
{
    public class VesselDataLinkHandler : DataLinkHandler
    {
        #region Initialisation
        
        public VesselDataLinkHandler()
        {
            buildAPI();
        }
        
        protected void buildAPI()
        {
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.altitude; }, 
                "v.altitude", "Altitude"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.heightFromTerrain; }, 
                "v.heightFromTerrain", "Height from Terrain"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.missionTime; }, 
                "v.missionTime", "Mission Time"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.srf_velocity.magnitude; },
                "v.surfaceVelocity", "Surface Velocity"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.angularVelocity.magnitude; },
                "v.angularVelocity", "Angular Velocity"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.obt_velocity.magnitude; },
                "v.orbitalVelocity", "Orbital Velocity"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.horizontalSrfSpeed; },
                "v.surfaceSpeed", "Surface Speed"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.verticalSpeed; },
                "v.verticalSpeed", "Vertical Speed"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.atmDensity; },
                "v.atmosphericDensity", "Atmospheric Density"));
        }

        #endregion
    }

    public class OrbitDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public OrbitDataLinkHandler()
        {
            buildAPI();
        }

        protected void buildAPI()
        {
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.PeA; },
                "o.PeA", "PeA"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.ApA; },
                "o.ApA", "ApA"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.timeToAp; },
                "o.timeToAp", "Time to Ap"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.timeToPe; },
                "o.timeToPe", "Time to Pe"));
        }

        #endregion
    }

    public class SensorDataLinkHandler : DataLinkHandler
    {

        SensorCache sensorCache = new SensorCache();

        #region Initialisation

        public SensorDataLinkHandler(VesselChangeDetector vesselChangeDetector)
        {
            buildAPI();
            vesselChangeDetector.suscribe(new VesselChangeDetector.VesselChange(vesselChanged));
        }

        protected void buildAPI()
        {
            registerAPI(new APIEntry(
                dataSources => { return getsSensorValues(dataSources, "TEMP"); },
                "s.temperature", "Temperature"));
            registerAPI(new APIEntry(
                dataSources => { return getsSensorValues(dataSources, "ACC"); },
                "s.acceleration", "Acceleration"));
            registerAPI(new APIEntry(
                dataSources => { return getsSensorValues(dataSources, "GRAV"); },
                "s.gravity", "Gravity"));
            registerAPI(new APIEntry(
                dataSources => { return getsSensorValues(dataSources, "PRES"); },
                "s.pressure", "Pressure"));
        }

        #endregion

        #region Sensors

        protected List<ModuleEnviroSensor> getsSensorValues(DataSources datasources, string ID)
        {
            return sensorCache.get(ID);
        }

        #endregion

        #region VesselChangeDetector

        private void vesselChanged(Vessel vessel)
        {
            sensorCache.refresh(vessel);
        }

        #endregion
    }

    public class SensorCache
    {
        ReaderWriterLock theLock = new ReaderWriterLock();
        Dictionary<string, List<ModuleEnviroSensor>> sensors = new Dictionary<string, List<ModuleEnviroSensor>>();

        public void refresh(Vessel vessel)
        {
            theLock.AcquireWriterLock(0);

            try
            {
                sensors.Clear();

                List<Part> partsWithSensors = vessel.parts.FindAll(p => p.Modules.Contains("ModuleEnviroSensor"));

                foreach (Part part in partsWithSensors)
                {
                    foreach (var module in part.Modules)
                    {
                        if (module.GetType().Equals(typeof(ModuleEnviroSensor)))
                        {
                            List<ModuleEnviroSensor> list = null;
                            sensors.TryGetValue(((ModuleEnviroSensor)module).sensorType, out list);
                            if (list == null)
                            {
                                list = new List<ModuleEnviroSensor>();
                                sensors[((ModuleEnviroSensor)module).sensorType] = list;
                            }

                            list.Add((ModuleEnviroSensor)module);
                        }
                    }
                }

            }
            catch(Exception e)
            {
                PluginLogger.Log(e.Message);
            }

            theLock.ReleaseWriterLock();
        }

        public List<ModuleEnviroSensor> get(string ID)
        {
            theLock.AcquireReaderLock(0);

            List<ModuleEnviroSensor> ret = new List<ModuleEnviroSensor>(sensors[ID]);                    
 
            theLock.ReleaseReaderLock();
            
            return ret;
        }
    }

    public class PausedDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public PausedDataLinkHandler()
        {
            buildAPI();
        }

        protected void buildAPI()
        {
            registerAPI(new APIEntry(
                dataSources => { return FlightDriver.Pause || (!TelemachusPowerDrain.isActive || !TelemachusPowerDrain.activeToggle); },
                "p.paused", "Paused"));
        }

        #endregion
    }

    public class DefaultDataLinkHandler : DataLinkHandler
    {
        #region DataLinkHandler

        public override bool process(String API, out APIEntry result)
        {
            throw new SoftException("Bad data link reference.");
        }

        #endregion
    }

    public abstract class DataLinkHandler
    {
        #region API Delegates

        public delegate object APIDelegate(DataSources datasources);

        #endregion

        #region API Fields

        Dictionary<string, APIEntry> APIEntries =
           new Dictionary<string, APIEntry>();

        #endregion

        #region DataLinkHandler

        public virtual bool process(String API, out APIEntry result)
        {
            APIEntry entry = null;

            APIEntries.TryGetValue(API, out entry);

            if (entry == null)
            {
                result = null;
                return false;
            }
            else
            {
                result = entry;
                return true;
            } 
        }

        public void appendAPIList(ref List<KeyValuePair<String, String>> APIList)
        {
            while(APIEntries.GetEnumerator().MoveNext())
            {
                APIList.Add(new KeyValuePair<string, string>(
                    APIEntries.GetEnumerator().Current.Key, APIEntries.GetEnumerator().Current.Value.name));
            }
        }

        protected void registerAPI(APIEntry entry)
        {
            APIEntries.Add(entry.APIString, entry);
        }

        #endregion
    }

    public class APIEntry
    {
        #region Fields

        public DataLinkHandler.APIDelegate function { get; set; }
        public string APIString { get; set; }
        public string name { get; set; }

        #endregion

        #region Initialisation

        public APIEntry(DataLinkHandler.APIDelegate function, string APIString, string name)
        {
            this.function = function;
            this.APIString = APIString;
            this.name = name;
        }

        #endregion
    }
}
