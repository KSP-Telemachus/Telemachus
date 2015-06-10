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
        #region Data Rate Fields

        public const int RATE_AVERAGE_SAMPLE_SIZE = 20;
        static public UpLinkDownLinkRate dataRates { get { return itsDataRates; } set { itsDataRates = value; } }
        private static UpLinkDownLinkRate itsDataRates = new UpLinkDownLinkRate(RATE_AVERAGE_SAMPLE_SIZE);

        #endregion

        private int MAX_STREAM_RATE = 0;

        private IKSPAPI kspAPI = null;
        private Servers.AsynchronousServer.ClientConnection clientConnection = null;

        private Regex matchJSONAttributes = new Regex(@"[\{""|,""|,""]([^"":]*)"":([^:]*)[,|\}]");

        private Timer streamTimer = new Timer();

        private int streamRate = 500;
        HashSet<string> subscriptions = new HashSet<string>();
        HashSet<string> toRun = new HashSet<string>();
        readonly private System.Object subscriptionLock = new System.Object();

        public KSPWebSocketService(IKSPAPI kspAPI, Servers.AsynchronousServer.ClientConnection clientConnection)
            : this(kspAPI)
        {
            this.clientConnection = clientConnection;
            streamTimer.Interval = streamRate;
            streamTimer.Elapsed += streamData;
            streamTimer.Enabled = true;
        }

        public KSPWebSocketService(IKSPAPI kspAPI)
        {
            this.kspAPI = kspAPI;
        }

        private void streamData(object sender, ElapsedEventArgs e)
        {
            streamTimer.Interval = streamRate;

            DataSources dataSources = new DataSources();

            if (toRun.Count + subscriptions.Count > 0)
            {
                try
                {
                    List<string> entries = new List<string>();

                    APIEntry entry = null;

                    lock (subscriptionLock)
                    {
                        dataSources.vessel = kspAPI.getVessel();

                        //Only parse the paused argument if the active vessel is null
                        if (dataSources.vessel != null)
                        {
                            toRun.UnionWith(subscriptions);

                            foreach (string s in toRun)
                            {
                                DataSources dataSourcesClone = dataSources.Clone();
                                string trimedQuotes = s.Trim();
                                string refArg = trimedQuotes;
                                kspAPI.parseParams(ref refArg, ref dataSourcesClone);

                                kspAPI.process(refArg, out entry);

                                if (entry != null)
                                {
                                    dataSourcesClone.setVarName(trimedQuotes);
                                    entries.Add(entry.formatter.format(entry.function(dataSourcesClone), dataSourcesClone.getVarName()));
                                }
                            }

                            toRun.Clear();

                            if (entry != null)
                            {
                                WebSocketFrame frame = new WebSocketFrame(ASCIIEncoding.UTF8.GetBytes(entry.formatter.pack(entries)));
                                byte[] bFrame = frame.AsBytes();
                                dataRates.addDownLinkPoint(System.DateTime.Now, bFrame.Length * UpLinkDownLinkRate.BITS_PER_BYTE);
                                clientConnection.Send(bFrame);
                            }
                        }
                        else
                        {
                            sendNullMessage();
                        }
                    } 
                }
                catch(NullReferenceException)
                {
                    PluginLogger.debug("Swallowing null reference exception, potentially due to async game state change.");
                    sendNullMessage();
                }
                catch (Exception ex)
                {
                    PluginLogger.debug("Closing socket due to potential client disconnect:" + ex.GetType().ToString());
                    close();
                }
            }
            else
            {
                sendNullMessage();
            }
        }

        protected void sendNullMessage()
        {
            WebSocketFrame frame = new WebSocketFrame(ASCIIEncoding.UTF8.GetBytes("{}"));
            clientConnection.Send(frame.AsBytes());
        }

        public void OpCodeText(object sender, FrameEventArgs e)
        {
            string command = e.frame.PayloadAsUTF8();
            
            dataRates.addUpLinkPoint(System.DateTime.Now, command.Length * UpLinkDownLinkRate.BITS_PER_BYTE);
            
            command = Regex.Replace(command, @"\s+", string.Empty);

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
            catch (Exception)
            {
                PluginLogger.debug("Swallowing integer parse failure when setting stream rate.");
            }
        }

        private string[] splitString(string p)
        {
            string[] strings = p.Substring(2, p.Length - 4).Split(new string[] {"\",\""}, StringSplitOptions.None);

            for (int i = 0; i < strings.Length; i++)
            {
                strings[i] = strings[i].Trim();
            }

            return strings;
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

        public void Shutdown(EventArgs e)
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
            return new KSPWebSocketService(kspAPI, clientConnection);
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
