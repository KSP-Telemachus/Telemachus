//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using MinimalHTTPServer;

namespace Telemachus
{
    class InformationResponsibility : IHTTPRequestResponsibility
    {
        #region IHTTPRequestResponsibility

        public bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            if (request.path.StartsWith("/telemachus/information"))
            {
                cc.Send((new IOLessInformation()).ToString());
                return true;
            }

            return false;
        }

        #endregion 
    }

    class IOLessInformation : OKPage
    {
        #region Constructors

        public IOLessInformation():base("")
        {
            StringBuilder sb = new StringBuilder();

            header(ref sb);

#if (DEBUG)
            hash(ref sb);
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

        #endregion
    }  
}
