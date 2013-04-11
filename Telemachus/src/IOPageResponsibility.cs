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
                "temperature.html", "pressure.html", "gravity.html", "chart-const.js", "density.html", 
                "time-apoapsis-periapsis.html", "flight-control.html", "inclination-aop.html"};

        static IOPageResponsibility()
        {
            String[] hashes = new String[] {"22-4B-9F-81-FA-30-F7-CB-16-BC-1D-CD-63-B9-B3-A9-A5-F7-A4-06",
                                            "AC-35-AB-FA-CE-DD-24-B6-AF-0E-8F-4E-67-43-74-9B-3B-54-88-17",
                                            "7A-BE-ED-15-40-5E-6B-FE-00-D6-2E-B0-ED-07-C6-55-82-BF-84-07",
                                            "F7-35-B7-A4-41-27-68-6A-D2-38-FB-60-0B-36-6C-82-C2-4E-A1-D2",
                                            "5E-45-E6-F6-82-6B-1D-48-3F-76-00-89-B3-55-A5-F3-A1-B3-0F-1E",
                                            "17-73-EF-10-76-BA-50-11-DA-47-79-ED-E3-9A-23-10-70-C5-8E-7F",
                                            "46-08-5F-32-72-20-23-F6-78-7B-6D-60-F8-FD-C9-BE-56-F7-AA-82",
                                            "B5-6B-97-3A-75-FF-36-7E-49-03-C0-19-89-1F-7B-F8-E8-CE-03-A6",
                                            "4A-53-F5-C0-20-71-BB-2C-F4-18-FC-E3-6B-1F-30-83-C7-00-64-B4",
                                            "DE-4E-57-FC-0B-85-C4-C2-FF-6F-C1-1E-F2-69-61-0E-41-64-20-D5",
                                            "B0-D2-13-D3-CB-0E-08-D7-1C-4F-23-0F-B9-EF-01-50-61-0D-0E-E4",
                                            "4E-90-F4-2E-98-FD-DF-1B-54-2A-BE-57-94-75-8D-04-1D-18-CC-35",
                                            "D6-D2-41-F9-83-67-F7-9B-4D-95-E5-6C-58-20-C2-0E-4B-B4-80-4F",
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
}
