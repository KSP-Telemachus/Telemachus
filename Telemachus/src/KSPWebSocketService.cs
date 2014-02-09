//Author: Richard Bunt
using Servers;
using Servers.MinimalWebSocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telemachus
{
    public class KSPWebSocketService : IWebSocketService
    {
        private IKSPAPI kspAPI = null;
        private DataSources dataSources = null;
        private Servers.AsynchronousServer.ClientConnection clientConnection = null;

        public KSPWebSocketService(IKSPAPI kspAPI, DataSources dataSources, Servers.AsynchronousServer.ClientConnection clientConnection)
            : this(kspAPI, dataSources)
        {
            this.clientConnection = clientConnection;
        }

        public KSPWebSocketService(IKSPAPI kspAPI, DataSources dataSources)
        {
            this.kspAPI = kspAPI;
            this.dataSources = dataSources;
        }

        public void OpCodeText(object sender, FrameEventArgs e)
        {
            APIEntry entry = null;
            kspAPI.process("d.unitless", out entry);
            
            WebSocketFrame frame = new WebSocketFrame(ASCIIEncoding.UTF8.GetBytes(entry.formatter.format(entry.function(dataSources))));
            e.clientConnection.Send(frame.AsBytes());
        }

        public void OpCodeClose(object sender, FrameEventArgs frameEventArgs)
        {

        }

        public IWebSocketService buildService(Servers.AsynchronousServer.ClientConnection clientConnection)
        {
            return new KSPWebSocketService(kspAPI, dataSources, clientConnection);
        }

        #region Unused Callbacks

        public void OpCodePing(object sender, FrameEventArgs e)
        {

        }

        public void OpCodePong(object sender, FrameEventArgs e)
        {

        }

        public void OpCodeBinary(object sender, FrameEventArgs frameEventArgs)
        {

        }

        #endregion
    }
}
