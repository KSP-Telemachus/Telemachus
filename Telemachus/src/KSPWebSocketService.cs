using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Telemachus
{
    class KSPWebSocketService : WebSocketBehavior
    {
        /// How often we should send out reports to clients
        private int streamRate = 500;
        /// The list of variables to send out every time we communicate
        private HashSet<string> subscriptions = new HashSet<string>();
        /// A list of variables to evaluate ONLY the next time we are triggered
        private HashSet<string> oneShotRuns   = new HashSet<string>();
        ///  A lock to prevent simultaneous reading/updating of the client parameters
        readonly private object dataLock = new object();

        /// Track how much data we are sending/receiving
        private UpLinkDownLinkRate dataRates = null;

        /// Prevent trying to send more data when the last lot hasn't finished yet
        private bool readyToSend = true;
        private float lastUpdate = -500;
        private IKSPAPI api = null;

        public KSPWebSocketService(IKSPAPI api, UpLinkDownLinkRate rateTracker)
        {
            this.api = api;
            dataRates = rateTracker;
        }

        /// Does this client connection need to be updated?
        /// Checks the elapsed time, and the current sending state, to determine
        /// if we want to be updated.
        /// <param name="time">The current time, in seconds.</param>
        public bool UpdateRequired(float time)
        {
            if ((time-lastUpdate) > streamRate/1000)
            {
                return readyToSend;
            }
            return false;
        }

        /// Process a message recieved from a client
        protected override void OnMessage(MessageEventArgs e)
        {
            // We only care about text messages, for now.
            if (e.Type != Opcode.Text) return;
            dataRates.RecieveDataFromClient(e.RawData.Length);

            // deserialize the message as JSON
            var json = SimpleJson.SimpleJson.DeserializeObject(e.Data) as SimpleJson.JsonObject;

            lock(dataLock)
            {
                // Do any tasks requested here
                foreach (var entry in json)
                {
                    // Try converting the item to a list - this is the most common expected.
                    // If we got a string, then add it to the list to allow "one-shot" submission
                    string[] listContents = new string[] { };
                    if (entry.Value is SimpleJson.JsonArray)
                    {
                        listContents = (entry.Value as SimpleJson.JsonArray).OfType<string>().Select(x => x.Trim()).ToArray();
                    } else if (entry.Value is string)
                    {
                        listContents = new[] { entry.Value as string };
                    }
                    
                    // Process the possible API entries
                    if (entry.Key == "+")
                    {
                        PluginLogger.print(string.Format("Client {0} added {1}", ID, string.Join(",", listContents)));
                        subscriptions.UnionWith(listContents);
                    }
                    else if (entry.Key == "-")
                    {
                        PluginLogger.print(string.Format("Client {0} removed {1}", ID, string.Join(",", listContents)));
                        subscriptions.ExceptWith(listContents);
                    }
                    else if (entry.Key == "run")
                    {
                        PluginLogger.print(string.Format("Client {0} running {1}", ID, string.Join(",", listContents)));
                        oneShotRuns.UnionWith(listContents);
                    }
                    else if (entry.Key == "rate")
                    {
                        streamRate = Convert.ToInt32(entry.Value);
                        PluginLogger.print(string.Format("Client {0} setting rate {1}", ID, streamRate));
                    }
                    else
                    {
                        PluginLogger.print(String.Format("Client {0} send unrecognised key {1}", ID, entry.Key));
                    }
                } 
            } // Lock
        } // OnMessage

        /// Read all variables and send back the responses for just this client
        public void SendDataUpdate()
        {
            // Don't do anything if we are e.g. still awaiting data to be fully set
            if (!readyToSend) return;
            lastUpdate = UnityEngine.Time.time;

            // Grab all of the variables at once
            string[] allVariables;
            lock (dataLock)
            {
                allVariables = subscriptions.Union(oneShotRuns).ToArray();
                oneShotRuns.Clear();
            }

            var vessel = api.getVessel();

            // Now, process them all into a data dictionary
            var apiResults = new Dictionary<string, object>();
            var unknowns = new List<string>();
            var errors = new Dictionary<string, string>();
            foreach (var apiString in allVariables)
            {
                try {
                    apiResults[apiString] = api.ProcessAPIString(apiString);
                } catch (IKSPAPI.UnknownAPIException)
                {
                    // IF we get this message, we know it was because no variable was found
                    unknowns.Add(apiString);
                } catch (Exception ex)
                {
                    errors[apiString] = ex.ToString();
                }
            }
            if (unknowns.Count > 0) apiResults["unknown"] = unknowns;
            if (errors.Count > 0)   apiResults["errors"] = errors;

            // Now, if we have data send a message, otherwise send a null message
            readyToSend = false;
            var data = SimpleJson.SimpleJson.SerializeObject(apiResults);
            SendAsync(data, (b) => readyToSend = true );
            dataRates.SendDataToClient(data.Length);
        }
    }
}
