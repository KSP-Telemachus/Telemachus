//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using MinimalHTTPServer;

namespace Telemachus
{
    class InformationResponsibility : IHTTPRequestResponsibility
    {
        #region Fields

        IOPageResponsibility ipr = null;
        DataLinkResponsibility dlr = null;

        #endregion

        #region Initialisation

        public InformationResponsibility(IOPageResponsibility ipr, DataLinkResponsibility dlr)
        {
            this.ipr = ipr;
            this.dlr = dlr;
        }

        #endregion

        #region IHTTPRequestResponsibility

        public bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            if (request.path.StartsWith("/telemachus/information"))
            {
                cc.Send((new IOLessInformation(ipr, dlr)).ToString());
                return true;
            }

            return false;
        }

        #endregion 
    }

    class IOLessInformation : OKPage
    {
        #region Constructors

        public IOLessInformation(IOPageResponsibility ipr, DataLinkResponsibility dlr)
            : base("")
        {
            StringBuilder sb = new StringBuilder();

            header(ref sb);

            pages(ref sb, ipr.getFiles());

#if (DEBUG)
            hash(ref sb, ipr.getPageHashes());

            List<KeyValuePair<String, String>> APIList = new List<KeyValuePair<String, String>>();
            dlr.getAPIList(ref APIList);
            api(ref sb, APIList);
#endif
            footer(ref sb);

            content = sb.ToString();
        }

        #endregion

        #region Page Generation

        public override String ToString()
        {
            StringBuilder response = new StringBuilder(base.ToString());

            return response.ToString();
        }

        private void header(ref StringBuilder sb)
        {
            sb.Append("<!DOCTYPE html>");
            sb.Append("<html><body>");
            sb.Append("<h1>Telemachus Information Page</h1>");
            sb.Append("Version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            sb.Append("</br>");
        }

        private void hash(ref StringBuilder sb, List<String> theHashes)
        {
            sb.Append("<h1>Hash</h1>");

            foreach (String hash in theHashes)
            {
                sb.Append("\"" + hash + "\",</br>");
            }

            sb.Append("\"\"};");
            sb.Append("</br>");
        }

        private void api(ref StringBuilder sb, List<KeyValuePair<String, String>> APIList)
        {
            sb.Append("<h1>API</h1>");

            foreach (KeyValuePair<String, String> entry in APIList)
            {
                sb.Append(entry + "</br>");
            }

            sb.Append("</br>");
        }

        private void pages(ref StringBuilder sb, string[] files)
        {
            sb.Append("<h1>Pages</h1>");

            foreach (String page in files)
            {
                //Ignore .css and .js
                if (page.EndsWith(".html"))
                {
                    sb.Append("<a href=\"/telemachus/" + page + "\">" + page + "</a></br>");
                }
            }


            sb.Append("</br>");
        }

        private void footer(ref StringBuilder sb)
        {
            sb.Append("</body></html>");
        }

        #endregion
    }  
}
