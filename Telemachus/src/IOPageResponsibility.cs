//Author: Richard Bunt

using System;
using System.Collections.Generic;
using System.Text;
using Servers.MinimalHTTPServer;

namespace Telemachus
{
    class IOPageResponsibility : IHTTPRequestResponsibility
    {
        #region Constants

        const String PAGE_PREFIX = "/telemachus";

        #endregion

        #region Delegates

        static OKResponsePage.ByteReader KSPByteReader = fileName =>
        {
            byte[] content = System.IO.File.ReadAllBytes(buildPath(escapeFileName(fileName)));

            return content;
        };

        static OKResponsePage.TextReader KSPTextReader = fileName =>
        {

            if (fileName.Length > 0)
            {
                string content = System.IO.File.ReadAllText(buildPath(escapeFileName(fileName)));
                return content;
            }

            return "";
        };

        #endregion

        #region IHTTPRequestResponsibility

        public bool process(Servers.AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            if (request.path.StartsWith(PAGE_PREFIX))
            {
                try
                {
                    OKResponsePage page = new OKResponsePage(
                            KSPByteReader, KSPTextReader,
                            request.path.Substring(PAGE_PREFIX.Length));
                    ((Servers.MinimalHTTPServer.ClientConnection)cc).Send(page);

                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        #endregion

        #region Methods

        static protected string buildPath(string fileName)
        {
            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            const string webFiles = "PluginData/Telemachus/";
            return assemblyPath.Replace("Telemachus.dll", "") + webFiles + fileName;
        }

        static protected string escapeFileName(string fileName)
        {
            return fileName.Replace("..", "");
        }

        #endregion
    }
}
