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
            registerAPI(new APIEntry(new APIDelegate(altitude), "v.altitude", "Altitude"));
        }

        #endregion

        #region API Functions

        private object altitude(DataSources dataSources)
        {
            return dataSources.vessel.altitude;
        }

        #endregion
    }

    public class APIListDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public APIListDataLinkHandler()
        {
            buildAPI();
        }

        private void buildAPI()
        {
            registerAPI(new APIEntry(new APIDelegate(getAPIList), "c.api", "API List"));
        }

        #endregion

        #region API Functions

        private object getAPIList(DataSources dataSources)
        {
            List<KeyValuePair<String, String>> APIList = new List<KeyValuePair<string, string>>();
            
            foreach(DataLinkHandler dlh in dataSources.APIHandlers)
            {
                dlh.appendAPIList(ref APIList);
            }

            return APIList;
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

        protected DataSources dataSources = new DataSources();
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
        DataLinkHandler.APIDelegate itsFunction = null;
        public DataLinkHandler.APIDelegate function { get; set; }
        string itsAPIString = null;
        public string APIString { get; set; }
        string itsName = null;
        public string name { get; set; }

        public APIEntry(DataLinkHandler.APIDelegate function, string APIString, string name)
        {
            itsFunction = function;
            itsAPIString = APIString;
            itsName = name;
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
