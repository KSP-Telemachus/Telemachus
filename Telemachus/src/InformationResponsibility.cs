//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using MinimalHTTPServer;

namespace Telemachus
{
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
                resources(ref sb);
                hash(ref sb);
#endif
                footer(ref sb);
                cc.Send(new OKPage(sb.ToString()).ToString());

                return true;
            }

            return false;
        }

        private void header(ref StringBuilder sb)
        {
            sb.Append("<html><body>");
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


        private void resources(ref StringBuilder sb)
        {
            sb.Append("<h1>Resources</h1>");

            
            foreach (Part p in dataLinks.dataLinks.vessel.Parts)
            {
                sb.Append("Part: " + p.name + "</br></br>");
                foreach (PartResource ri in p.Resources.list)
                {
                    sb.Append("Name: " + ri.resourceName + 
                        "Amount: " + ri.amount + "max amount: " + ri.maxAmount + "</br>");
                }
            }

            sb.Append("</br></br>");
        }

        private void hash(ref StringBuilder sb)
        {
            sb.Append("<h1>Hash</h1>");

            List<String> theHashes = IOPageResponsibility.getPageHashes();
            foreach (String hash in theHashes)
            {
                sb.Append("\"" + hash + "\",</br>");
            }

            sb.Append("\"\"};");
            sb.Append("</br>");
        }

        private void footer(ref StringBuilder sb)
        {
            sb.Append("</body></html>");
        }
    }
}
