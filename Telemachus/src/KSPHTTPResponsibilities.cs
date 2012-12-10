using System;
using System.Text;
using MinimalHTTPServer;
using System.Reflection;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Telemachus
{
    class TelemachusResponsibility : IHTTPRequestResponsibility
    {
        static Dictionary<string, string> hashCheck =
            new Dictionary<string, string>();
        const String PAGE_PREFIX = "/telemachus";

        static String[] files = new String[] { "altitude.html", 
                "g-force.html", "velocity.html", 
                "fit-to-screen.css", "dynamic-pressure.html", "apoapsis-periapsis.html",
                "temperature.html", "pressure.html", "gravity.html"};

        static TelemachusResponsibility()
        {
            String[] hashes = new String[] {"6B-D5-88-E0-47-5A-2E-EB-82-4E-B7-3F-EA-B1-90-E4-71-DD-D3-C4",
                                            "C5-C2-2A-EA-1C-C7-AE-45-B7-C2-6F-3D-0C-CE-F9-B7-5C-C2-17-50",
                                            "C2-AB-83-C5-EB-36-F6-43-14-C5-2B-50-0D-F2-9F-8B-81-D0-3F-E0",
                                            "F7-35-B7-A4-41-27-68-6A-D2-38-FB-60-0B-36-6C-82-C2-4E-A1-D2",
                                            "3D-17-6A-14-EC-89-C9-E0-11-DB-34-30-6B-99-A7-B4-B0-51-02-32",
                                            "5F-EB-57-93-4A-35-84-00-E2-5C-FB-F9-6B-AC-D0-35-88-CB-67-4F",
                                            "F1-BE-D1-96-42-A3-C9-34-28-22-C5-BD-E1-74-D4-9E-15-BF-11-A3",
                                            "F6-E8-73-6B-C8-6A-8F-DD-BC-F1-C0-67-C4-F3-0E-DE-0A-CC-DE-6D",
                                            "2C-07-56-1C-97-1E-F0-6A-D8-D9-45-12-FA-EB-8B-30-4C-B7-8C-F8",
                                            ""};

            foreach (String hash in hashes)
            {
                hashCheck.Add(hash, "");
            }
        }
        
        public bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            if (request.path.StartsWith(PAGE_PREFIX))
            {
                try
                {
                    String fileName = request.path.Substring(PAGE_PREFIX.Length - 1);
                    KSP.IO.TextReader tr = null;
                    if (fileName.Length > 0)
                    {
                         tr = KSP.IO.TextReader.CreateForType<TelemachusDataLink>
                            (fileName);
                    }

                    String fileContents = tr.ReadToEnd();
#if (!DEBUG)
                    if(!checkHash(fileContents))
                    {
                        return false;
                    }
#endif

                    cc.Send(new OKPage(fileContents, fileName).ToString());
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        private bool checkHash(String contents)
        {
            string hash = "";
            using (var cryptoProvider = new SHA1CryptoServiceProvider())
            {
                hash = BitConverter
                        .ToString(cryptoProvider.ComputeHash(stringToByteArray(contents)));
            }

            if (hashCheck.ContainsKey(hash))
            {
                return true;
            }

            return false;
        }

        public static List<String> getPageHashes()
        {
            List<String> theHashes = new List<String>();

            foreach (String fileName in files)
            {
                KSP.IO.TextReader tr = null;

                tr = KSP.IO.TextReader.CreateForType<TelemachusDataLink>
                   (fileName);

                using (var cryptoProvider = new SHA1CryptoServiceProvider())
                {
                    string hash = BitConverter
                            .ToString(cryptoProvider.ComputeHash(
                            TelemachusResponsibility.stringToByteArray(tr.ReadToEnd())));

                    theHashes.Add(hash);
                }
            }

            return theHashes;
        }

        private static byte[] stringToByteArray(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }

    class DataLinkResponsibility : IHTTPRequestResponsibility
    {
        public const String PAGE_PREFIX = "/telemachus/datalink";
        public const char ARGUMENTS_START = '?';
        public const char ARGUMENTS_ASSIGN = '=';
        public const char ARGUMENTS_DELIMETER = '&';
        public const char ACCESS_DELIMITER = '.';
        public const int INFINITE_WAIT = -1;

        DataLink dataLinks = null;
        Dictionary<string, CachedAPIReference> APICache =
            new Dictionary<string, CachedAPIReference>();
        static ReaderWriterLock valueCacheLock = new ReaderWriterLock();
        List<IAPIHandler> APIHandlers = new List<IAPIHandler>();

        public DataLinkResponsibility(DataLink dataLinks)
        {
            this.dataLinks = dataLinks;

            APIHandlers.Add(new SensorAPIHandler(dataLinks));
            APIHandlers.Add(new ReflectiveAPIHandler(dataLinks));
            APIHandlers.Add(new DefaultAPIHandler());
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
                    catch(Exception e)
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
            CachedAPIReference cachedAPIReference = null;

            valueCacheLock.AcquireReaderLock(INFINITE_WAIT);

            try
            {
                APICache.TryGetValue(argsSplit[1], out cachedAPIReference);

                if (cachedAPIReference == null)
                {
                    LockCookie lc = valueCacheLock.UpgradeToWriterLock(INFINITE_WAIT);

                    try
                    {
                        foreach(IAPIHandler APIHandler in APIHandlers)
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
    
    class InformationResponsibility : IHTTPRequestResponsibility
    {
        DataLinkResponsibility dataLinks = null;


        public InformationResponsibility(DataLinkResponsibility dataLinks)
        {
            this.dataLinks = dataLinks;
        }

        public bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            if (request.path.StartsWith("/telemachus/information"))
            {
                StringBuilder sb = new StringBuilder();

                header(ref sb);

#if (DEBUG)
                api(ref sb);
                hash(ref sb);
#endif

                cc.Send(new OKPage(sb.ToString()).ToString());

                return true;
            }

            return false;
        }

        private void header(ref StringBuilder sb)
        {
            sb.Append("<h1>Telemachus Information Page</h1>");
            sb.Append("Version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            sb.Append("</br>");
        }

        private void api(ref StringBuilder sb)
        {
            sb.Append("<h1>API</h1>");

            List<String> theAPI = dataLinks.getAccessList();
            foreach (String api in theAPI)
            {
                sb.Append(api + "</br>");
            }

            sb.Append("</br>");
        }

        private void hash(ref StringBuilder sb)
        {
            sb.Append("<h1>Hash</h1>");

            List<String> theHashes = TelemachusResponsibility.getPageHashes();
            foreach (String hash in theHashes)
            {
                sb.Append("\"" + hash + "\",</br>");
            }

            sb.Append("\"\"};");
            sb.Append("</br>");
        }
    }

    class ElseResponsibility : IHTTPRequestResponsibility
    {
        public bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            cc.Send(new IOLessDataLinkNotFound().ToString());
            return true;
        }
    }
}
