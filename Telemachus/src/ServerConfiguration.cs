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
        /// <summary>The IP Address configured in the Telemachus plugin configuration</summary>
        public IPAddress ipAddress { get; set; }
        /// <summary>A list of IP Addresses that the server should be accessible at</summary>
        public List<IPAddress> ValidIpAddresses { get; set; }

        public ServerConfiguration()
        {
            port = 8085;
            ipAddress = IPAddress.Any;
            ValidIpAddresses = new List<IPAddress>();
        }
    }

    internal static class ServerConfigExtensions
    {
        internal static bool IsPortNumber(this int value)
        {
            return value > 0 && value < 65536;
        }
    }
}