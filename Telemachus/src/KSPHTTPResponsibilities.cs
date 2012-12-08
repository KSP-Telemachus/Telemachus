using System;
using System.Text;
using MinimalHTTPServer;
using System.Reflection;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;

namespace Telemachus
{
    class TelemachusResponsibility : IHTTPRequestResponsibility
    {
        static Dictionary<string, string> hashCheck =
            new Dictionary<string, string>();
        const String PAGE_PREFIX = "/telemachus";

        static String[] files = new String[] { "altitude.html", 
                "g-force.html", "velocity.html", 
                "fit-to-screen.css", "dynamicpressure.html", "apoapsis-periapsis.html" };

        static TelemachusResponsibility()
        {
            String[] hashes = new String[] {"6B-D5-88-E0-47-5A-2E-EB-82-4E-B7-3F-EA-B1-90-E4-71-DD-D3-C4",
                                            "23-B5-C4-37-BE-EF-D6-7F-F3-D2-7B-17-77-97-CD-EC-E4-06-AA-D8",
                                            "50-E4-D6-2B-E7-10-EA-7A-2D-A9-C4-65-38-AF-78-BD-97-1D-D2-CB",
                                            "F7-35-B7-A4-41-27-68-6A-D2-38-FB-60-0B-36-6C-82-C2-4E-A1-D2",
                                            "3D-17-6A-14-EC-89-C9-E0-11-DB-34-30-6B-99-A7-B4-B0-51-02-32",
                                            "BD-A0-A7-22-CF-B2-81-05-6C-C1-8A-16-7B-CD-77-2F-AA-35-B1-85",
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
        const String PAGE_PREFIX = "/telemachus/datalink";
        const char ARGUMENTS_START = '?';
        const char ARGUMENTS_ASSIGN = '=';
        const char ARGUMENTS_DELIMETER = '&';
        const char ACCESS_DELIMITER = '.';
        const String JAVASCRIPT_DELIMITER = ";";
        const String JAVASCRIPT_ASSIGN = " = ";

        DataLink dataLinks = null;
        Dictionary<string, ReflectiveArgumentData> abstractArgument =
            new Dictionary<string, ReflectiveArgumentData>();

        public DataLinkResponsibility(DataLink dataLinks)
        {
            this.dataLinks = dataLinks;
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
            abstractArgument.Clear();
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
            ReflectiveArgumentData ad = null;
            abstractArgument.TryGetValue(argsSplit[1], out ad);

            if (ad == null)
            {
                ad = new ReflectiveArgumentData();
                ad.key = argsSplit[1];
                abstractArgument.Add(argsSplit[1], ad);
            }

            ad.variableName = argsSplit[0];
            ad.updateValue(dataLinks);

            return ad.toJavascriptString();
        }

        public class ReflectiveArgumentData
        {
            public String variableName { get; set; }
            public String key { get; set; }
            private Object value = null, parentValue = null;

            FieldInfo field = null;
            PropertyInfo pfield = null;

            public void updateValue(DataLink dl)
            {
                if (field == null && pfield == null)
                {
                    reflectiveUpdate(dl);
                }
                else
                {
                    if (pfield == null)
                    {
                        value = field.GetValue(parentValue);
                    }
                    else
                    {
                        value = pfield.GetValue(parentValue, null);
                    }
                }
            }

            private void reflectiveUpdate(DataLink dataLinks)
            {
                String[] argsSplit = key.Split(ACCESS_DELIMITER);
                Type type = dataLinks.GetType();
                value = dataLinks;
                FieldInfo field = null;
                PropertyInfo pfield = null;
                int accessType = 0;

                for (int i = 0; i < argsSplit.Length; i ++ )
                {
                    String s = argsSplit[i];

                    try
                    {
                        field = type.GetField(s);
                        parentValue = value;
                        value = field.GetValue(parentValue);
                        accessType = 1;
                    }
                    catch
                    {
                        pfield = type.GetProperty(s);
                        parentValue = value;
                        value = pfield.GetValue(parentValue, null);
                        accessType = 2;
                    }

                    type = value.GetType();

                    if (i == argsSplit.Length - 1)
                    {
                        if (accessType == 1)
                        {
                            this.field = field;
                        }
                        else
                        {
                            this.pfield = pfield;
                        }
                    }
                }
            }

            public String toJavascriptString()
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(variableName);
                sb.Append(JAVASCRIPT_ASSIGN);
                sb.Append(value.ToString());
                sb.Append(JAVASCRIPT_DELIMITER);

                return sb.ToString();
            }
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
