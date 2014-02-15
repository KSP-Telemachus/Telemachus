//Author: Richard Bunt
using Servers.MinimalWebSocketServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Servers
{
    public class WebsocketUpgrade : Servers.MinimalHTTPServer.HTTPResponse
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

    public class WebSocketHeader
    {
        public const byte FINISH_MASK = 0x80;
        public const byte RSV1_MASK = 0x40;
        public const byte RSV2_MASK = 0x20;
        public const byte RSV3_MASK = 0x10;
        public const byte OP_CODE_MASK = 0x0F;
        public const byte MASK_MASK = 0x80;
        public const byte PAYLOAD_MASK = 0x7F;

        public byte fin { get; set; }
        public byte rsv1 { get; set; }
        public byte rsv2 { get; set; }
        public byte rsv3 { get; set; }
        public OpCode opCode { get; set; }
        public bool mask { get; set; }
        public ulong length { get; set; }
        public byte[] maskKeys = null;
        public int position;

        public void parse(List<byte> input, ServerConfiguration configuration)
        {
            byte[] b = null;

            getBytes(input, ref b, ref position, 2);

            fin = (byte)(b[0] & FINISH_MASK);
            rsv1 = (byte)(b[0] & RSV1_MASK);
            rsv2 = (byte)(b[0] & RSV2_MASK);
            rsv3 = (byte)(b[0] & RSV3_MASK);

            opCode = (OpCode)(b[0] & OP_CODE_MASK);

            mask = (b[1] & MASK_MASK) == MASK_MASK;
            byte payload = (byte)(b[1] & PAYLOAD_MASK);

            byte[] bytesLength = null;
            switch (payload)
            {
                case 126:
                    getBytes(input, ref bytesLength, ref position, 2);
                    length = BitConverter.ToUInt16(bytesLength.Reverse().ToArray<byte>(), 0);
                    break;
                case 127:
                    getBytes(input, ref bytesLength, ref position, 8);
                    bytesLength.Reverse();
                    length = BitConverter.ToUInt64(bytesLength.Reverse().ToArray<byte>(), 0);
                    break;
                default:
                    length = payload;
                    break;
            }

            if (length > configuration.maxMessageSize)
            {
                throw new Servers.MinimalHTTPServer.RequestEntityTooLargeResponsePage();
            }

            if (mask)
            {
                getBytes(input, ref maskKeys, ref position, 4);
            }
        }

        public void getBytes(List<byte> input, ref byte[] b, ref int position, int count)
        {
            if (position + count > input.Count)
            {
                position = 0;
                throw new InsufficientDataToParseFrameException();
            }
            else
            {
                b = input.GetRange(position, count).ToArray<byte>();
                position += count;
            }
        }
    }

    public class WebSocketFrame
    {
        public WebSocketHeader header { get; set; }

        private byte[] data = null;
        private ushort closeCode = 0;

        public WebSocketFrame()
        {
            header = new WebSocketHeader();
            header.fin = WebSocketHeader.FINISH_MASK;
            header.rsv1 = 0;
            header.rsv2 = 0;
            header.rsv3 = 0;
        }

        public WebSocketFrame(byte[] data)
        {
            header = new WebSocketHeader();
            header.fin = WebSocketHeader.FINISH_MASK;
            header.rsv1 = 0;
            header.rsv2 = 0;
            header.rsv3 = 0;
            header.opCode = OpCode.Text;
            header.length = (ulong)data.Length;
            this.data = data;
        }

        public int parse(List<byte> input, ServerConfiguration configuration)
        {
            if (header == null)
            {
                header = new WebSocketHeader();
            }

            header.parse(input, configuration);

            header.getBytes(input, ref data, ref header.position, (int)header.length);

            if (header.mask)
            {
                for (int i = 0; i < data.Length; ++i)
                {
                    data[i] = (byte)(data[i] ^ header.maskKeys[i % 4]);
                }
            }

            if (header.opCode == OpCode.Close && data.Length == 2)
            {
                closeCode = BitConverter.ToUInt16(((byte[])data.ToArray<byte>().Clone()).Reverse().ToArray(), 0);
            }

            return header.position;
        }

        public String PayloadAsUTF8()
        {
            return Encoding.UTF8.GetString(data.ToArray<byte>());
        }

        public byte[] PayloadAsByte()
        {
            return data;
        }

        public byte[] AsBytes()
        {
            List<byte> frame = new List<byte>();

            byte one = (byte)(header.fin | header.rsv1 | header.rsv2 | header.rsv3 | (byte)header.opCode);
            frame.Add(one);

            if (header.length < 126)
            {
                byte two = (byte)header.length;
                frame.Add(two);
            }
            else if (header.length <= (ulong)Int16.MaxValue)
            {
                byte two = 126;
                frame.Add(two);
                byte[] size = BitConverter.GetBytes((Int16)header.length);
                frame.AddRange(size.Reverse<byte>());
            }
            else if (header.length <= (ulong)Int32.MaxValue)
            {
                byte two = 127;
                frame.Add(two);
                byte[] size = BitConverter.GetBytes((Int64)header.length);
                frame.AddRange(size.Reverse<byte>());
            }

            frame.AddRange(data);

            return frame.ToArray<byte>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Message: " + PayloadAsUTF8() + " ");
            sb.AppendLine("Payload length: " + header.length + " ");
            sb.AppendLine("Mask: " + header.mask + " ");
            sb.AppendLine("Fin: " + header.fin + " ");
            sb.AppendLine("RSV1: " + header.rsv1 + " ");
            sb.AppendLine("RSV2: " + header.rsv2 + " ");
            sb.AppendLine("RSV3: " + header.rsv3 + " ");
            sb.AppendLine("Op Code: " + header.opCode + " ");

            return sb.ToString();
        }
    }

    public class WebSocketPingFrame : WebSocketFrame
    {
        public WebSocketPingFrame()
        {
            header.opCode = OpCode.Pong;
        }
    }

    public class InsufficientDataToParseFrameException : Exception
    {
    }
}
