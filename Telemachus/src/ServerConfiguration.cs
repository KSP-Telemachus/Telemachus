using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;


namespace Telemachus
{

    public class ServerConfiguration
    {
        public string version { get; set; }
        public string name { get; set; }
        public int port { get; set; }
        public int backLog { get; set; }
        public virtual int bufferSize { get; set; }
        public List<IPAddress> ipAddresses { get; set; }

        public ServerConfiguration()
        {
            ipAddresses = new List<IPAddress>();
            port = 8085;
            backLog = 100;
            bufferSize = 512;
        }

        public String getIPsAsString()
        {
            const String CONCAT = " and ";
            StringBuilder sb = new StringBuilder();

            foreach (IPAddress ip in ipAddresses)
            {
                sb.Append(ip.ToString());
                sb.Append(":");
                sb.Append(port);
                sb.Append(CONCAT);
            }

            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - CONCAT.Length, CONCAT.Length);
            }

            return sb.ToString();
        }

        public void addIPAddressAsString(String ip)
        {
            ipAddresses.Insert(0, IPAddress.Parse(ip));
        }
    }
}