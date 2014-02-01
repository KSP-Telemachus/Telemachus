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

    public enum OpCode
    {
        Continue = 0x0,
        Text = 0x1,
        Binary = 0x2,
        Close = 0x8,
        Ping = 0x9,
        Pong = 0xA
    }

    public class WebSocketFrame
    {
        public byte fin { get; set; }
        public byte rsv1 { get; set; }
        public byte rsv2 { get; set; }
        public byte rsv3 { get; set; }

        OpCode opCode { get; set; }

        bool mask { get; set; }

        byte[] data = null;

        ulong length = 0;
        

        public WebSocketFrame()
        {
        }

        public WebSocketFrame(byte[] data)
        {
            fin = 0x80;
            rsv1 = 0;
            rsv2 = 0;
            rsv3 = 0;

            length = (ulong)data.Length;
            this.data = data;
        }

        public bool parse(ArraySegment<byte> input)
        {
            try
            {
                byte[] b = input.Array.Take(2).ToArray<byte>();
                IEnumerable<byte> position = input.Array.Skip(2);

                fin = (byte)(b[0] & 0x80);
                rsv1 = (byte)(b[0] & 0x40);
                rsv2 = (byte)(b[0] & 0x20);
                rsv3 = (byte)(b[0] & 0x10);

                opCode = (OpCode)((b[0] & 0x8)
                    | (b[0] & 0x4) | (b[0] & 0x2) | (b[0] & 0x1));

                mask = (b[1] & 0x80) == 0x80;

                byte payload = (byte)((input.Array[1] & 0x40)
                    | (b[1] & 0x20)
                    | (b[1] & 0x10) | (b[1] & 0x8)
                    | (b[1] & 0x4) | (b[1] & 0x2)
                    | (b[1] & 0x1));

                length = 0;

                switch (payload)
                {
                    case 126:
                        byte[] bytesUShort = position.Take(2).ToArray<byte>();
                        if (bytesUShort != null)
                        {
                            length = BitConverter.ToUInt16(bytesUShort.Reverse().ToArray(), 0);
                        }
                        position = position.Skip(2);
                        break;
                    case 127:
                        byte[] bytesULong = position.Take(8).ToArray<byte>();
                        if (bytesULong != null)
                        {
                            length = BitConverter.ToUInt16(bytesULong.Reverse().ToArray(), 0);
                        }
                        position = position.Skip(8);
                        break;
                    default:
                        length = payload;
                        break;
                }

                byte[] maskKeys = null;
                if (mask)
                {
                    maskKeys = position.Take(4).ToArray<byte>();
                    position = position.Skip(4);
                }

                data = position.Take((int)length).ToArray<byte>();

                if (mask)
                {
                    for (int i = 0; i < data.Length; ++i)
                    {
                        data[i] = (byte)(data[i] ^ maskKeys[i % 4]);
                    }
                }

                ushort closeCode = 0;
                if (opCode == OpCode.Close && data.Length == 2)
                {
                    closeCode = BitConverter.ToUInt16(((byte[])data.Clone()).Reverse().ToArray(), 0);
                }

                Logger.debug("Message:" + AsUTF8());
                Logger.debug("Payload length: " + length);
                Logger.debug("Mask: " + mask);
                Logger.debug("Fin: " + fin);
                Logger.debug("RSV1: " + rsv1);
                Logger.debug("RSV2: " + rsv2);
                Logger.debug("RSV3: " + rsv3);
                Logger.debug("Op Code: " + opCode);
            }
            catch (Exception ex)
            {
                Logger.debug("Websocket Read failed: " + ex.ToString());
            }

            return true;
        }

        public String AsUTF8()
        {
            return Encoding.UTF8.GetString(data);
        }

        public ArraySegment<byte> AsFrame()
        {
            List<byte> frame = new List<byte>();

            byte one = (byte)(fin | rsv1 | rsv2 | rsv3 | 0x1);
            frame.Add(one);

            if (length < 126)
            {
                byte two = (byte)length;
                frame.Add(two);
            }
            else if (length <= (ulong)Int16.MaxValue)
            {
                byte two = 126;
                frame.Add(two);
                byte[] size = BitConverter.GetBytes((Int16)length);
                frame.AddRange(size);
            }
            else if (length <= (ulong)Int32.MaxValue)
            {
                byte two = 127;
                frame.Add(two);
                byte[] size = BitConverter.GetBytes((Int32)length);
                frame.AddRange(size);
            }
            
            frame.AddRange(data);

            return new ArraySegment<byte>(frame.ToArray<byte>());
        }
    }
}
