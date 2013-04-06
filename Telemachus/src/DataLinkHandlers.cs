//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MinimalHTTPServer;

namespace Telemachus
{
    public class VesselDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public VesselDataLinkHandler()
        {
            buildAPI();
        }

        private void buildAPI()
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
                dataSources => { return FlightDriver.Pause || (!TelemachusPowerDrain.isActive || !TelemachusPowerDrain.activeToggle); }, 
                "p.paused", "Paused"));
        }

        #endregion

        #region API Functions

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

            try
            {
                APIEntries.TryGetValue(API, out entry);
                result = entry;
            }
            catch
            {
                result = null;
                return false;
            }

            return true;
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

        public DataLinkHandler.APIDelegate function { get; set; }

        public string APIString { get; set; }

        public string name { get; set; }

        public APIEntry(DataLinkHandler.APIDelegate function, string APIString, string name)
        {
            this.function = function;
            this.APIString = APIString;
            this.name = name;
        }
    }

    /*
    public class ReflectiveDataLinkHandler : IDataLinkHandler
    {
        const int FIELD_ACCESS = 1, PROPERTY_ACCESS = 2;

        DataLink dataLinks = null;

        public ReflectiveDataLinkHandler(DataLink dataLinks)
        {
            this.dataLinks = dataLinks;
        }

        public bool process(String API, String assign, ref Dictionary<string, CachedDataLinkReference> cache, ref CachedDataLinkReference cachedAPIReference)
        {

            String[] argsSplit = API.Split(DataLinkResponsibilityOld.ACCESS_DELIMITER);
            Type type = dataLinks.GetType();
            Object value = dataLinks, parentValue = null;
            FieldInfo field = null;
            PropertyInfo property = null;
            int accessType = 0;
            int i = 0;

            for (i = 0; i < argsSplit.Length; i++)
            {
                try
                {
                    field = type.GetField(argsSplit[i]);
                    parentValue = value;
                    value = field.GetValue(parentValue);
                    accessType = FIELD_ACCESS;
                }
                catch
                {
                    accessType = PROPERTY_ACCESS;
                    property = type.GetProperty(argsSplit[i]);
                    parentValue = value;
                    value = property.GetValue(parentValue, null);
                }

                type = value.GetType();
            }

            switch (accessType)
            {
                case FIELD_ACCESS:
                    cachedAPIReference = new FieldCachedDataLinkReference(field, parentValue, new JavaScriptGeneralFormatter());
                    cache.Add(API, cachedAPIReference);
                    return true;

                case PROPERTY_ACCESS:
                    cachedAPIReference = new PropertyCachedDataLinkReference(property, parentValue, new JavaScriptGeneralFormatter());
                    cache.Add(API, cachedAPIReference);
                    return true;

                default:
                    return false;
            }
        }
    }

    public class SensorDataLinkHandler : IDataLinkHandler
    {
        DataLink dataLinks = null;

        List<ModuleEnviroSensor> sensors =
            new List<ModuleEnviroSensor>();

        public SensorDataLinkHandler(DataLink dataLinks)
        {
            this.dataLinks = dataLinks;
        }

        public bool process(String API, String assign, ref Dictionary<string, CachedDataLinkReference> cache, ref CachedDataLinkReference cachedAPIReference)
        {
            String[] path = API.Split(DataLinkResponsibilityOld.ACCESS_DELIMITER);
           
            if(path[0].Equals("sensor"))
            {
                List<ModuleEnviroSensor> sensors = new List<ModuleEnviroSensor>();

                foreach (Part part in dataLinks.v.parts.FindAll(p => p.Modules.Contains("ModuleEnviroSensor")))
                {
                    foreach (var module in part.Modules)
                    {
                        if (module.GetType().Equals(typeof(ModuleEnviroSensor)))
                        {
                            ModuleEnviroSensor sensor = (ModuleEnviroSensor)module;
            
                            if(sensor.sensorType.Equals(path[1]))
                            {
                                sensors.Add(sensor);
                            }
                        }
                    }
                }

                cachedAPIReference = new SensorCachedDataLinkReference(sensors, new JavaScriptGeneralFormatter());
                cache.Add(API, cachedAPIReference);

                return true;
            }

            return false;
        }
    }
    */
}
