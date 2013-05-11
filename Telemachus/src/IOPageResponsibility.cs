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
            KSP.IO.BinaryReader binaryReader = null;
            long fileLen = 0;
            if (fileName.Length > 0)
            {
                fileLen = KSP.IO.FileInfo.CreateForType<TelemachusDataLink>(fileName).Length;

                if (fileLen > int.MaxValue)
                {
                    throw new ExceptionResponsePage("Unable to serve file, too large.");
                }

                binaryReader = KSP.IO.BinaryReader.CreateForType<TelemachusDataLink>
                   (fileName);
            }
            byte[] content = binaryReader.ReadBytes((int)fileLen);
            binaryReader.Close();

            return content;
        };

        static OKResponsePage.TextReader KSPTextReader = fileName => 
        {
            KSP.IO.TextReader textReader = null;
            if (fileName.Length > 0)
            {
                textReader = KSP.IO.TextReader.CreateForType<TelemachusDataLink>
                   (fileName);
            }
            string content = textReader.ReadToEnd();
            textReader.Close();

            return content;
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
                            request.path.Substring(PAGE_PREFIX.Length - 1));
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
    }
}
