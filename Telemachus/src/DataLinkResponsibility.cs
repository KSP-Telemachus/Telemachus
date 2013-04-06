//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using MinimalHTTPServer;
using System.Threading;
using System.Reflection;

namespace Telemachus
{
    public class DataLinkResponsibility : IHTTPRequestResponsibility
    {
        #region Constants

        public const String PAGE_PREFIX = "/telemachus/datalink";
        public const char ARGUMENTS_START = '?';
        public const char ARGUMENTS_ASSIGN = '=';
        public const char ARGUMENTS_DELIMETER = '&';
        public const char ACCESS_DELIMITER = '.';
        public const int INFINITE_WAIT = -1;

        #endregion

        #region Fields
        
        List<DataLinkHandler> APIHandlers = new List<DataLinkHandler>();
        DataSources ds = new DataSources();
        JavaScriptGeneralFormatter f = new JavaScriptGeneralFormatter();
        #endregion

        #region Data Rate Fields

        public const int RATE_AVERAGE_SAMPLE_SIZE = 20;
        public UpLinkDownLinkRate dataRates { get { return itsDataRates; } set { itsDataRates = value; } }
        private UpLinkDownLinkRate itsDataRates = new UpLinkDownLinkRate(RATE_AVERAGE_SAMPLE_SIZE);

        #endregion

        #region Initialisation

        public DataLinkResponsibility()
        {
            loadHandlers();
        }

        private void loadHandlers()
        {
            APIHandlers.Add(new VesselDataLinkHandler());
            APIHandlers.Add(new DefaultDataLinkHandler());
        }

        #endregion

        #region IHTTPRequestResponsibility

        public bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            if (request.path.StartsWith(PAGE_PREFIX))
            {
                dataRates.addUpLinkPoint(System.DateTime.Now, request.path.Length);
                ds.vessel = FlightGlobals.ActiveVessel;

                String returnMessage = new OKPage(
                   argumentsParse(
                   request.path.Remove(
                   0, request.path.IndexOf(ARGUMENTS_START) + 1), ds)
                   ).ToString();

                dataRates.addDownLinkPoint(System.DateTime.Now, returnMessage.Length);

                cc.Send(returnMessage);

                return true;
            }

            return false;
        }

        #endregion

        #region Parse URL

        private String argumentsParse(String args, DataSources dataSources)
        {
            StringBuilder sb = new StringBuilder();
            String[] argsSplit = args.Split(ARGUMENTS_DELIMETER);

            foreach (String arg in argsSplit)
            {
                sb.Append(argumentParse(arg, dataSources));
            }

            return sb.ToString();
        }

        private String argumentParse(String args, DataSources dataSources)
        {
            String[] argsSplit = args.Split(ARGUMENTS_ASSIGN);
            APIEntry result = null;

            foreach (DataLinkHandler APIHandler in APIHandlers)
            {
                if (APIHandler.process(argsSplit[1], out result))
                {
                    break;
                }
            }

            object r = result.function(dataSources);
            return JavaScriptGeneralFormatter.formatWithAssignment(
                f.format(r.ToString(), r.GetType()), argsSplit[0]);
        }

        #endregion
    }

    public class DataSources
    {
        #region Fields

        public Vessel vessel;
        public string format;
        public string varName;
        public List<DataLinkHandler> APIHandlers;

        #endregion
    }

    /*
    public class DataLinkResponsibilityOld : IHTTPRequestResponsibility
    {
        public const String PAGE_PREFIX = "/telemachus/datalink";
        public const char ARGUMENTS_START = '?';
        public const char ARGUMENTS_ASSIGN = '=';
        public const char ARGUMENTS_DELIMETER = '&';
        public const char ACCESS_DELIMITER = '.';
        public const int INFINITE_WAIT = -1;
        public const int RATE_AVERAGE_SAMPLE_SIZE = 20;
   
        string itsApiList =  "{" +
                                "'Mission-Time':'v.missionTime'," +
                                "'Altitude':'v.altitude'," +
                                "'ApA':'o.ApA'," +
                                "'PeA':'o.PeA'," +
                                "'Density':'v.atmDensity'," +
                                "'Orbital-Velocity':'d.orbitalVelocity'," +
                                "'Surface-Velocity':'d.surfaceVelocity'" +
                             "}";
        public string apiList { get { return itsApiList; } }

        public DataLink dataLinks {get; set;}
        Dictionary<string, CachedDataLinkReference> APICache =
            new Dictionary<string, CachedDataLinkReference>();
        static ReaderWriterLock valueCacheLock = new ReaderWriterLock();
        List<IDataLinkHandler> APIHandlers = new List<IDataLinkHandler>();

        private UpLinkDownLinkRate itsDataRates = new UpLinkDownLinkRate(RATE_AVERAGE_SAMPLE_SIZE);
        public UpLinkDownLinkRate dataRates { get { return itsDataRates; } set { itsDataRates = value; } }

        public DataLinkResponsibilityOld(DataLink dataLinks)
        {
            this.dataLinks = dataLinks;

            APIHandlers.Add(new SensorDataLinkHandler(dataLinks));
            APIHandlers.Add(new ReflectiveDataLinkHandler(dataLinks));
            APIHandlers.Add(new DefaultDataLinkHandler());
        }

        public bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            if (request.path.StartsWith(PAGE_PREFIX))
            {
                dataRates.addUpLinkPoint(System.DateTime.Now, request.path.Length);

                String returnMessage = new OKPage(
                    argumentsParse(
                    request.path.Remove(
                    0, request.path.IndexOf(ARGUMENTS_START) + 1))
                    ).ToString();

                dataRates.addDownLinkPoint(System.DateTime.Now, returnMessage.Length);

                cc.Send(returnMessage);

                return true;
            }
            
            return false;
        }

        public void clearCache()
        {
            valueCacheLock.AcquireWriterLock(INFINITE_WAIT);

            try
            {
                APICache.Clear();
            }
            finally
            {
                valueCacheLock.ReleaseWriterLock();
            }
        }

        public List<String> getAccessList()
        {
            List<String> theAPI = new List<String>();
            Object value = null, innerValue = null;
            Type type = typeof(DataLink);
            FieldInfo[] fields = type.GetFields(), innerFields = null;
            PropertyInfo[] innerProperties = null;

            foreach (var field in fields)
            {
                value = field.GetValue(dataLinks);
                innerFields = value.GetType().GetFields();

                foreach (FieldInfo innerField in innerFields)
                {
                    innerValue = innerField.GetValue(value);

                    if (innerValue != null)
                    {
                        theAPI.Add("Field " + "Type: " +
                            innerValue.GetType().Name + " " + field.Name + ACCESS_DELIMITER + innerField.Name);
                    }
                }

                innerProperties = value.GetType().GetProperties();

                foreach (PropertyInfo innerProperty in innerProperties)
                {
                    innerValue = null;
                    String typeName = "";
                    try
                    {

                        innerValue = innerProperty.GetValue(value, null);
                        typeName = innerValue.GetType().Name;
                    }
                    catch (Exception e)
                    {
                        typeName = "[" + e.Message + "]";

                    }

                    if (innerValue != null)
                    {
                        theAPI.Add("Property " + "Type: " +
                             typeName + " " + field.Name + ACCESS_DELIMITER + innerProperty.Name);
                    }
                }
            }

            return theAPI;
        }

        private String argumentsParse(String args)
        {
            StringBuilder sb = new StringBuilder();
            String[] argsSplit = args.Split(ARGUMENTS_DELIMETER);

            foreach (String arg in argsSplit)
            {
                sb.Append(argumentParse(arg));
            }

            return sb.ToString();
        }

        private String argumentParse(String args)
        {
            String[] argsSplit = args.Split(ARGUMENTS_ASSIGN);
            CachedDataLinkReference cachedAPIReference = null;

            valueCacheLock.AcquireReaderLock(INFINITE_WAIT);

            try
            {
                APICache.TryGetValue(argsSplit[1], out cachedAPIReference);

                if (cachedAPIReference == null)
                {
                    LockCookie lc = valueCacheLock.UpgradeToWriterLock(INFINITE_WAIT);

                    try
                    {
                        foreach (IDataLinkHandler APIHandler in APIHandlers)
                        {
                            if (APIHandler.process(argsSplit[1], argsSplit[0], ref APICache, ref cachedAPIReference))
                            {
                                break;
                            }
                        }
                    }
                    finally
                    {
                        valueCacheLock.DowngradeFromWriterLock(ref lc);
                    }
                }
            }
            finally
            {
                valueCacheLock.ReleaseReaderLock();
            }

            return JavaScriptGeneralFormatter.formatWithAssignment(cachedAPIReference.getValue(), argsSplit[0]);
        }
    }
     */
}
