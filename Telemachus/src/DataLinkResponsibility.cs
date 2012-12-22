//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using MinimalHTTPServer;
using System.Threading;
using System.Reflection;

namespace Telemachus
{
    class DataLinkResponsibility : IHTTPRequestResponsibility
    {
        public const String PAGE_PREFIX = "/telemachus/datalink";
        public const char ARGUMENTS_START = '?';
        public const char ARGUMENTS_ASSIGN = '=';
        public const char ARGUMENTS_DELIMETER = '&';
        public const char ACCESS_DELIMITER = '.';
        public const int INFINITE_WAIT = -1;

        public DataLink dataLinks {get; set;}
        Dictionary<string, CachedDataLinkReference> APICache =
            new Dictionary<string, CachedDataLinkReference>();
        static ReaderWriterLock valueCacheLock = new ReaderWriterLock();
        List<IDataLinkHandler> APIHandlers = new List<IDataLinkHandler>();

        public DataLinkResponsibility(DataLink dataLinks)
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
                cc.Send(new OKPage(
                    argumentsParse(
                    request.path.Remove(
                    0, request.path.IndexOf(ARGUMENTS_START) + 1))
                    ).ToString());

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
                        /*Report the type as the error message if the property cannot be accessed.
                          This is most likely because indexed properties are not supported*/
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
}
