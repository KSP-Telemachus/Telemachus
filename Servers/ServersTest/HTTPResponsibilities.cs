using System;
using MinimalHTTPServer;
using System.Collections.Generic;

namespace ServersTest
{
    class TelemachusResponsibility : IHTTPRequestResponsibility
    {
        public bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            if (request.path.StartsWith("/telemachus"))
            {
                try
                {
                    cc.Send(new OKPage(System.IO.File.ReadAllText(request.path.TrimStart(new char[]{'/'}))).ToString());
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
    }

    class DataLinkResponsibility : IHTTPRequestResponsibility
    {
        DataLinks dataLinks = null;
        Dictionary<string, string> dictionary =
        new Dictionary<string, string>();

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

                foreach (String arg in argsSplit)
                {

                }
            }

            return false;
        }
    }

    public class DataLinks
    {
        public TestProgram tp = new TestProgram();
        public TestProgram tp2 = new TestProgram();
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
