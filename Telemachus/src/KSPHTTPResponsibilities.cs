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
        Dictionary<string, string> hashCheck =
            new Dictionary<string, string>();
        String PAGE_PREFIX = "/telemachus";

        public TelemachusResponsibility()
        {
            String[] hashes = new String[] {"6B-D5-88-E0-47-5A-2E-EB-82-4E-B7-3F-EA-B1-90-E4-71-DD-D3-C4",
                                            "23-B5-C4-37-BE-EF-D6-7F-F3-D2-7B-17-77-97-CD-EC-E4-06-AA-D8",
                                            "50-E4-D6-2B-E7-10-EA-7A-2D-A9-C4-65-38-AF-78-BD-97-1D-D2-CB",
                                            "F7-35-B7-A4-41-27-68-6A-D2-38-FB-60-0B-36-6C-82-C2-4E-A1-D2",
                                            "8F-DD-F5-6B-E5-6B-F0-06-10-AA-39-88-A7-9D-71-12-D1-CB-A6-BC",
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
                        .ToString(cryptoProvider.ComputeHash(GetBytes(contents)));
            }

            if (hashCheck.ContainsKey(hash))
            {
                return true;
            }

            return false;
        }

        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }

    class DataLinkResponsibility : IHTTPRequestResponsibility
    {
        DataLinks dataLinks = null;
        Dictionary<string, ReflectiveArgumentData> abstractArgument =
            new Dictionary<string, ReflectiveArgumentData>();

        public DataLinkResponsibility(DataLinks dataLinks)
        {
            this.dataLinks = dataLinks;
        }

        public bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            if (request.path.StartsWith("/telemachus/datalink"))
            {
                String args = request.path.Remove(0, request.path.IndexOf('?') + 1);

                String[] argsSplit = args.Split('&');

                cc.Send(new OKPage(argumentsParse(args)).ToString());
             
                return true;
            }

            return false;
        }

        public void clear()
        {
            abstractArgument.Clear();
        }

        private String argumentsParse(String args)
        {
            StringBuilder sb = new StringBuilder();
            String[] argsSplit = args.Split('&');

            foreach (String arg in argsSplit)
            {
                sb.Append(argumentParse(arg));
            }

            return sb.ToString();
        }

        private String argumentParse(String args)
        {
            String[] argsSplit = args.Split('=');
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

            return ad.ToString();
        }

        public class ReflectiveArgumentData
        {
            public String variableName { get; set; }
            public String key { get; set; }
            private  Object value = null;
            private Object parentValue = null;

            FieldInfo field = null;

            public void updateValue(DataLinks dl)
            {
                if (field == null)
                {
                    reflectiveUpdate(dl);
                }
                else
                {
                    value = field.GetValue(parentValue);
                }
            }

            private void reflectiveUpdate(DataLinks dl)
            {
                String[] argsSplit = key.Split('.');
                Type type = dl.GetType();
                value = dl;

                foreach (String s in argsSplit)
                {
                    field = type.GetField(s);
                    parentValue = value;
                    value = field.GetValue(parentValue);

                    type = value.GetType();
                }
            }

            public override String ToString()
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(variableName);
                sb.Append(" = ");
                sb.Append(value.ToString());
                sb.Append(";");

                return sb.ToString();
            }
        }
    }

    public class DataLinks
    {
        public Vessel vessel;
        public Orbit orbit;
    }

    class ElseResponsibility : IHTTPRequestResponsibility
    {
        public bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            cc.Send(new IOLessDataLinkNotFound().ToString());
            return true;
        }
    }

    class InformationResponsibility : IHTTPRequestResponsibility
    {
        public bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            if (request.path.StartsWith("/telemachus/information"))
            {
                StringBuilder sb = new StringBuilder();

                header(ref sb);
                api(ref sb);
                hash(ref sb);
               
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
            Type type = typeof(Vessel);
            FieldInfo[] fields = type.GetFields();
            foreach (var field in fields)
            {
                sb.Append("Vessel: " + field.Name + "</br>");
            }

            type = typeof(Orbit);
            fields = type.GetFields();
            foreach (var field in fields)
            {
                sb.Append("Orbit: " + field.Name + "</br>");
            }

            sb.Append("</br>");
        }

        private void hash(ref StringBuilder sb)
        {
            sb.Append("<h1>Hash</h1>");

            String[] files = new String[] { "altitude.html", "g-force.html", "velocity.html", "fit-to-screen.css", "dynamicpressure.html" };

            sb.Append("{");
            foreach (String fileName in files)
            {
                KSP.IO.TextReader tr = null;

                tr = KSP.IO.TextReader.CreateForType<TelemachusDataLink>
                   (fileName);

                using (var cryptoProvider = new SHA1CryptoServiceProvider())
                {
                    string hash = BitConverter
                            .ToString(cryptoProvider.ComputeHash(
                            TelemachusResponsibility.GetBytes(tr.ReadToEnd())));

                    sb.Append("\"" + hash + "\",</br>");
                }

            }

            sb.Append("\"\"};");
            sb.Append("</br>");
        }
    }
}
