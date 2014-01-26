using Servers.MinimalHTTPServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Servers
{
    public class WebsocketUpgrade : HTTPResponse
    {
        public const String WEB_SOCKET_KEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public WebsocketUpgrade(String key)
            : base("")
        {
            
            String returnKey = key.Trim() + WEB_SOCKET_KEY;
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] sha1 = sha.ComputeHash(Encoding.ASCII.GetBytes(returnKey));

            protocol = "HTTP/1.1";
            responseType = "Switching Protocols";
            responseCode = "101";
            attributes.Add("Upgrade", "websocket");
            
            attributes.Remove("Content-Length");
            attributes.Remove("Content-Type");
            attributes.Remove("Date");

            protocol = "HTTP/1.1";
            attributes.Add("Connection", "Upgrade");
            attributes.Add("Sec-WebSocket-Accept", System.Convert.ToBase64String(sha1));
        }
    }
}
