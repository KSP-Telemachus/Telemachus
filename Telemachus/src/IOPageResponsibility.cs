//Author: Richard Bunt
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
                "temperature.html", "pressure.html", "gravity.html", "chart-const.js", "density.html"};

        static IOPageResponsibility()
        {
            String[] hashes = new String[] {"D0-4C-60-2F-92-D1-E4-DA-33-54-02-95-44-CC-E6-F7-F4-A9-54-84",
"6C-42-18-65-F8-F4-60-3A-F0-B3-33-5E-1D-D3-01-39-53-2F-7D-09",
"B2-BD-07-B3-AA-95-38-23-C1-90-F8-A6-9D-72-1C-28-AD-E4-BF-79",
"F7-35-B7-A4-41-27-68-6A-D2-38-FB-60-0B-36-6C-82-C2-4E-A1-D2",
"55-02-BF-0F-FD-54-BE-3B-C9-0D-69-61-B9-F9-D4-26-56-FE-13-44",
"8A-07-1A-52-8E-07-2D-DC-C3-8F-00-7C-43-5A-61-BA-63-0E-5F-96",
"41-FE-B4-5D-13-85-02-FC-C5-57-42-81-6A-2F-A7-64-7E-8A-B6-25",
"6D-48-1D-38-85-A5-D2-5F-F6-D7-57-64-36-3A-85-87-6C-59-E9-E9",
"CC-22-5F-06-EC-7A-6E-7D-C8-41-2B-1E-EE-AF-16-31-85-9F-27-A2",
"4A-A6-AB-0B-82-77-F2-46-BF-D9-38-23-82-D8-DC-C6-00-26-41-7E",
"82-65-F9-35-F6-0A-81-72-60-15-C2-4B-3C-22-BD-27-03-36-B7-F1",
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
