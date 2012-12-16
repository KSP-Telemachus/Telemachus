using System;
using System.Collections.Generic;
using System.Text;
using MinimalHTTPServer;
using System.Security.Cryptography;

namespace Telemachus
{
    class IOPageResponsibility : IHTTPRequestResponsibility
    {
        static Dictionary<string, string> hashCheck =
            new Dictionary<string, string>();
        const String PAGE_PREFIX = "/telemachus";

        public readonly static String[] files = new String[] { "altitude.html", 
                "g-force.html", "velocity.html", 
                "fit-to-screen.css", "dynamic-pressure.html", "apoapsis-periapsis.html",
                "temperature.html", "pressure.html", "gravity.html", "chart-const.js"};

        static IOPageResponsibility()
        {
            String[] hashes = new String[] {"5B-1E-CF-0E-B9-7F-C8-88-27-F4-5B-C5-06-BB-A9-2E-CD-B4-63-0A",
                                            "A6-D5-55-50-24-68-FA-A6-2D-0A-A2-AC-8B-79-77-39-CE-A4-45-8A",
                                            "81-D7-46-D7-36-4B-CD-2A-9E-CB-B8-2B-B8-13-09-08-FF-2C-C9-7A",
                                            "F7-35-B7-A4-41-27-68-6A-D2-38-FB-60-0B-36-6C-82-C2-4E-A1-D2",
                                            "F0-B5-96-5F-15-D6-E5-02-D5-89-A8-9C-4F-CD-69-1B-02-F4-EF-3C",
                                            "3A-28-9C-65-D1-39-4D-7D-3F-51-56-94-BB-28-0C-73-73-82-A6-99",
                                            "F3-DB-CD-2B-45-49-34-5D-50-2A-73-08-AF-F4-E9-EF-85-48-18-CA",
                                            "D1-85-95-94-CC-86-E7-AA-FC-DC-D8-3B-5E-41-2A-A7-41-D2-2A-04",
                                            "03-20-15-E0-28-2F-6F-B9-40-05-EB-7F-95-18-CA-EA-5D-EE-3D-C9",
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
                    KSP.IO.TextReader textReader = null;
                    if (fileName.Length > 0)
                    {
                        textReader = KSP.IO.TextReader.CreateForType<TelemachusDataLink>
                           (fileName);
                    }

                    String fileContents = textReader.ReadToEnd();
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
                KSP.IO.TextReader textReader = null;

                textReader = KSP.IO.TextReader.CreateForType<TelemachusDataLink>
                   (fileName);

                using (var cryptoProvider = new SHA1CryptoServiceProvider())
                {
                    string hash = BitConverter
                            .ToString(cryptoProvider.ComputeHash(
                            IOPageResponsibility.stringToByteArray(textReader.ReadToEnd())));

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

    class ElseResponsibility : IHTTPRequestResponsibility
    {
        public bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            cc.Send(new IOLessDataLinkDisplayNotFoundPage(IOPageResponsibility.files).ToString());
            return true;
        }
    }

    class IOLessDataLinkDisplayNotFoundPage : HTTPResponse
    {
        const String HTML_FILE_SUFFIX = ".html";

        public IOLessDataLinkDisplayNotFoundPage(String[] files)
        {
            protocol = "HTTP/1.0";
            responseType = "OK";
            responseCode = "200";
            attributes.Add("Content-Type", "text / html");
            attributes.Add("Content-Length", "0");

            StringBuilder sb = new StringBuilder();

            sb.Append("<html><body><h1>");
            sb.Append("Data Link Not Found");
            sb.Append("</h1>");
            sb.Append("<p>");
            sb.Append("The data link you have requested was not found, ");
            sb.Append("this craft is broadcasting the following:</p>");

            foreach (String fileName in files)
            {
                if(fileName.EndsWith(HTML_FILE_SUFFIX))
                {
                    sb.Append("<a href=\"telemachus/" + fileName 
                        + "\"" + ">" + fileName.Replace(HTML_FILE_SUFFIX, "") + "</a></br>");
                }
            }

            sb.Append("</body></html>");

            content = sb.ToString();
        }

        public override String ToString()
        {
            StringBuilder response = new StringBuilder(base.ToString());

            return response.ToString();
        }
    }  
}
