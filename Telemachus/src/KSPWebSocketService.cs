//Author: Richard Bunt
using Servers;
using Servers.MinimalWebSocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;

namespace Telemachus
{
    public class KSPWebSocketService : IWebSocketService
    {
        private int MAX_STREAM_RATE = 0;

        private IKSPAPI kspAPI = null;
        private DataSources dataSources = null;
        private Servers.AsynchronousServer.ClientConnection clientConnection = null;

        private Regex matchJSONAttributes = new Regex(@"[\{|,\s+]""([^:]*)"":([^:]*)[,|\}]");

        private Timer streamTimer = new Timer();

        private int streamRate = 500;
        HashSet<string> subscriptions = new HashSet<string>();
        HashSet<string> toRun = new HashSet<string>();
        readonly private System.Object subscriptionLock = new System.Object();

        public KSPWebSocketService(IKSPAPI kspAPI, DataSources dataSources, Servers.AsynchronousServer.ClientConnection clientConnection)
            : this(kspAPI, dataSources)
        {
            this.clientConnection = clientConnection;
            streamTimer.Interval = streamRate;
            streamTimer.Elapsed += streamData;
            streamTimer.Enabled = true;
        }

        public KSPWebSocketService(IKSPAPI kspAPI, DataSources dataSources)
        {
            this.kspAPI = kspAPI;
            this.dataSources = dataSources;
        }

        private void streamData(object sender, ElapsedEventArgs e)
        {
            streamTimer.Interval = streamRate;

            dataSources.vessel = FlightGlobals.ActiveVessel;

            if (toRun.Count + subscriptions.Count > 0)
            {
                try
                {
                    List<string> entries = new List<string>();

                    APIEntry entry = null;

                    lock (subscriptionLock)
                    {
                        toRun.UnionWith(subscriptions);

                        foreach (string s in toRun)
                        {
                            kspAPI.process(s.Substring(1, s.Length - 2), out entry);
                            if (entry != null)
                            {
                                entry.formatter.setVarName(entry.APIString);
                                entries.Add(entry.formatter.format(entry.function(dataSources)));
                            }
                        }

                        toRun.Clear();
                    }

                    if (entry != null)
                    {
                        WebSocketFrame frame = new WebSocketFrame(ASCIIEncoding.UTF8.GetBytes(entry.formatter.pack(entries)));
                        clientConnection.Send(frame.AsBytes());
                    }
                }
                catch(Exception ex)
                {
                    Logger.debug(ex.ToString()); 
                }
            }
        }

        public void OpCodeText(object sender, FrameEventArgs e)
        {
            string command = e.frame.PayloadAsUTF8();

            MatchCollection mc = matchJSONAttributes.Matches(command);

            foreach (Match m in mc)
            {
                switch (m.Groups[1].ToString())
                {
                    case "+":
                        subscribe(m.Groups[2].ToString());
                        break;
                    case "-":
                        unsubscribe(m.Groups[2].ToString());
                        break;
                    case "run":
                        run(m.Groups[2].ToString());
                        break;
                    case "rate":
                        rate(m.Groups[2].ToString());
                        break;
                }
            }
        }

        private void rate(string p)
        {
            int proposedRate = 0;

            try
            {
                proposedRate = int.Parse(p);

                if (proposedRate >= MAX_STREAM_RATE)
                {
                    streamRate = proposedRate;
                }

            }
            catch (Exception ex)
            {
                Logger.debug(ex.ToString());
            }
        }

        private string[] splitString(string p)
        {
            return p.Substring(1, p.Length - 2).Split(',');
        }

        private void run(string p)
        {
            lock (subscriptionLock)
            {
                toRun.UnionWith(splitString(p));
            }
        }

        private void unsubscribe(string p)
        {
            string[] toRemove = splitString(p);

            lock (subscriptionLock)
            {
                foreach (string item in toRemove)
                {
                    subscriptions.Remove(item);
                }
            }
        }

        private void subscribe(string p)
        {
            lock (subscriptionLock)
            {
                subscriptions.UnionWith(splitString(p));
            }
        }

        public void OpCodeClose(object sender, FrameEventArgs frameEventArgs)
        {
            close();
        }

        private void close()
        {
            streamTimer.Stop();
            clientConnection.tryShutdown();
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
