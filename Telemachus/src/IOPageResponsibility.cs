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
                "temperature.html", "pressure.html", "gravity.html"};

        static IOPageResponsibility()
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
