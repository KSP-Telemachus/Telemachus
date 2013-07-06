//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Servers
{
    namespace MinimalHTTPServer
    {
        public abstract class HTTPTransaction : Exception
        {
            protected const string NEW_LINE = "\r\n";
            protected const char HEADER_ATTR_ASSIGN = ':';
            protected Dictionary<String, String> attributes =
                new Dictionary<String, String>();

            public String protocol { get; set; }
            private String itsContent = "";

            public String content
            {
                get { return itsContent; }
                set
                {
                    //Dynamically update the conetent length when it is set
                    if (attributes.ContainsKey("Content-Length"))
                    {
                        attributes.Remove("Content-Length");
                    }

                    attributes.Add("Content-Length", value.Length.ToString());
                    itsContent = value;
                }
            }

            private byte[] itsByteContent = null;
            public byte[] byteContent
            {
                get { return itsByteContent; }
                set
                {
                    //Dynamically update the conetent length when it is set
                    if (attributes.ContainsKey("Content-Length"))
                    {
                        attributes.Remove("Content-Length");
                    }

                    attributes.Add("Content-Length", value.Length.ToString());
                    itsByteContent = value;
                }
            }

            public HTTPTransaction()
            {

            }
        }

        public class HTTPRequest : HTTPTransaction
        {
            public String requestType { get; set; }
            public String path { get; set; }

            public HTTPRequest()
            {

            }

            public void parse(String input)
            {
                String[] lines = input.Split(NEW_LINE.ToCharArray());
                int i = parseHeader(lines) + 1;
                for (; i < input.Length; i++)
                {
                    content += input[i];
                }
            }

            private int parseHeader(String[] input)
            {
                String[] firstLine = input[0].Split(' ');

                requestType = firstLine[0];
                path = firstLine[1];
                protocol = firstLine[2];

                int i = 0;
                for (i = 1; i < input.Length && input[i] != Server.HEADER_END; i++)
                {
                    String[] attr = input[i].Split(HEADER_ATTR_ASSIGN);
                    if (attr.Length == 2)
                    {
                        attributes.Add(attr[0], attr[1]);
                    }
                }

                return i;
            }
        }

        public class HTTPResponse : HTTPTransaction
        {
            public String responseType { get; set; }
            public String responseCode { get; set; }

            public delegate byte[] ByteReader(string fileName);
            public delegate string TextReader(string fileName);
            public delegate void ContentReader(ByteReader br, TextReader tr, string fileName,
                ref Dictionary<string, string> attr, HTTPResponse r);

            static ContentReader TextContentReader =
                (ByteReader br, TextReader tr, string filename, ref Dictionary<string, string> attr, HTTPResponse r) =>
                {
                    r.content = tr(filename);
                };

            static ContentReader Base64ImageContentReader =
                (ByteReader br, TextReader tr, string filename, ref Dictionary<string, string> attr, HTTPResponse r) =>
                {
                    r.byteContent = br(filename);
                };

            static Dictionary<string, KeyValuePair<string, ContentReader>>
                contentTypes = new Dictionary<string, KeyValuePair<string, ContentReader>>();

            static HTTPResponse()
            {
                contentTypes[".html"] = new KeyValuePair<string, ContentReader>(
                    "text/html", TextContentReader);
                contentTypes[".css"] = new KeyValuePair<string, ContentReader>(
                    "text/css", TextContentReader);
                contentTypes[".jpg"] = new KeyValuePair<string, ContentReader>("image/jpeg", Base64ImageContentReader);
                contentTypes[".jpeg"] = new KeyValuePair<string, ContentReader>("image/jpeg", Base64ImageContentReader);
                contentTypes[".png"] = new KeyValuePair<string, ContentReader>("image/png", Base64ImageContentReader);
                contentTypes[".js"] = new KeyValuePair<string, ContentReader>(
                    "application/x-javascript", TextContentReader);
            }

            public HTTPResponse(ByteReader br, TextReader tr, String fileName) : this()
            {
                KeyValuePair<string, ContentReader> reader = contentTypes[
                   fileName.Substring(fileName.LastIndexOf('.')).ToLower()];
                attributes.Add("Content-Type", reader.Key);

                reader.Value(br, tr, fileName, ref attributes, this);
            }

            public HTTPResponse(String content) : this()
            {
                attributes.Add("Content-Type", "text/html");
                this.content = content;
            }

            public HTTPResponse()
            {
                commonResponseFields();
                attributes.Add("Content-Length", "0");
            }

            public void setAttribute(string name, string value)
            {
                attributes.Add(name, value);
            }

            public override String ToString()
            {
                StringBuilder response = new StringBuilder();
                response.Append(protocol + " ");
                response.Append(responseCode + " ");
                response.Append(responseType + NEW_LINE);

                foreach (KeyValuePair<String, String> attribute in attributes)
                {
                    response.Append(attribute.Key + HEADER_ATTR_ASSIGN + " " + attribute.Value + NEW_LINE);
                }

                response.Append(NEW_LINE);

                response.Append(content);

                return response.ToString();
            }

            public virtual byte[] ToBytes()
            {
                List<byte> byteResponse = new List<byte>(Encoding.ASCII.GetBytes(ToString()));

                if (byteContent != null)
                {
                    byteResponse.AddRange(byteContent);
                }

                return byteResponse.ToArray();
            }

            private void commonResponseFields()
            {
                protocol = "HTTP/1.0";
                attributes.Add("Date", DateTime.Now.ToString());
            }
        }

        public class OKResponsePage : HTTPResponse
        {
            public OKResponsePage(String content) : base(content)
            {
                commonOK();
            }

            public OKResponsePage(ByteReader br, TextReader tr, String fileName) : base (br, tr, fileName)
            {
                commonOK();
            }

            private void commonOK()
            {
                responseType = "OK";
                responseCode = "200";
            }
        }

        public class ExceptionResponsePage : ResponseCodePage
        {
            public ExceptionResponsePage(String reason)
                : base(500, "Internal Server Error", reason)
            {
            }

            public override String ToString()
            {
                return base.ToString();
            }
        }

        public class RequestEntityTooLargeResponsePage : ResponseCodePage
        {
            const String status = "Request Entity Too Large";
            const int code = 414;

            public RequestEntityTooLargeResponsePage(string reason)
                : base(code, status, reason)
            {
            }

            public RequestEntityTooLargeResponsePage()
                : base(code, status, "")
            {
            }
        }

        public class BadRequestResponsePage : ResponseCodePage
        {
            const String status = "Bad Request";
            const int code = 400;

            public BadRequestResponsePage(string reason)
                : base(code, status, reason)
            {
            }

            public BadRequestResponsePage()
                : base(code, status, "")
            {
            }
        }

        public class PageNotFoundResponsePage : ResponseCodePage
        {
            const String status = "Page Not Found";
            const int code = 404;

            public PageNotFoundResponsePage(string reason)
                : base(code, status, reason)
            {
            }

            public PageNotFoundResponsePage()
                : base(code, status, "")
            {
            }
        }

        public class ResponseCodePage : HTTPResponse
        {
            public ResponseCodePage(int code, string name, string reason)
                : base("")
            {
                responseType = name;
                responseCode = code.ToString();
                content = buildPage(reason);
            }

            protected string buildPage(string reason)
            {
                string content = Servers.Properties.Resources.ResponseCodePageTemplate.ToString().Replace("$reason", reason);
                content = content.Replace("$code", base.responseCode);
                content = content.Replace("$status", base.responseType);
                return content;
            }
        }
    }
}
