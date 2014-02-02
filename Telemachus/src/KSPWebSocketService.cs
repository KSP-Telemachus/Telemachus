//Author: Richard Bunt
using Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telemachus
{
    class KSPWebSocketService : IWebSocketService
    {
        public void OpCodePing(object sender, FrameEventArgs e)
        {

        }

        public void OpCodePong(object sender, FrameEventArgs e)
        {

        }

        public void OpCodeText(object sender, FrameEventArgs e)
        {
            WebSocketFrame frame = new WebSocketFrame(ASCIIEncoding.UTF8.GetBytes("Echo: " + e.frame.PayloadAsUTF8()));
            e.clientConnection.Send(frame.AsBytes());
        }

        public void OpCodeBinary(object sender, FrameEventArgs frameEventArgs)
        {

        }

        public void OpCodeClose(object sender, FrameEventArgs frameEventArgs)
        {

        }

        public IWebSocketService buildService()
        {
            return new KSPWebSocketService();
        }
    }
}
