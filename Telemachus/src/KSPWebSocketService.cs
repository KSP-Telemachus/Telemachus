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
        /// A list of variables that the user has subscribed to binary updates of
        private string[] binarySubscriptions = new string[] { };
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
            if ((time-lastUpdate) > streamRate/1000f)
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
                    } else if (entry.Key == "binary")
                    {
                        binarySubscriptions = listContents;
                        PluginLogger.print(string.Format("Client {0} requests binary packets {1}", ID, string.Join(", ", listContents)));
                    } else
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
                allVariables = subscriptions.Union(oneShotRuns).Union(binarySubscriptions).ToArray();
                oneShotRuns.Clear();
            }

            var vessel = api.getVessel();

            // Now, process them all into a data dictionary
            var apiResults = new Dictionary<string, object>();
            var unknowns = new List<string>();
            var errors = new Dictionary<string, string>();
            foreach (var apiString in allVariables)
            {
                try
                {
                    apiResults[apiString] = api.ProcessAPIString(apiString);
                }
                catch (IKSPAPI.UnknownAPIException)
                {
                    // IF we get this message, we know it was because no variable was found
                    unknowns.Add(apiString);
                } catch (IKSPAPI.VariableNotEvaluable)
                {
                    // We can't evaluate this at the moment. Just ignore until we can.
                } catch (Exception ex)
                {
                    errors[apiString] = ex.ToString();
                }
            }
            if (unknowns.Count > 0) apiResults["unknown"] = unknowns;
            if (errors.Count > 0) apiResults["errors"] = errors;

            // Handle sending a binary packet if requested
            if (binarySubscriptions.Length > 0)
            {
                var variableValues = new List<float>();
                // Read every binary value
                foreach (var name in binarySubscriptions) {
                    try {
                        variableValues.Add(Convert.ToSingle(apiResults[name]));
                    } catch (Exception ex)
                    {
                        variableValues.Add(0);
                        if (apiResults.ContainsKey(name))
                        {
                            errors[name] = "Error streaming to binary " + name + "='" + apiResults[name] + "'; " + ex.ToString();
                        } else
                        {
                            errors[name] = "Error streaming to binary: value for " + name + " not found; " + ex.ToString();
                        }
                    }
                }
                // Which byte translation?
                Func<float, IEnumerable<byte>> reverser = x => BitConverter.GetBytes(x).Reverse();
                var byteTranslation = BitConverter.IsLittleEndian ? reverser : BitConverter.GetBytes;

                // Now translate these to binary bytes
                var byteData = new List<byte>();
                byteData.Add(1);
                byteData.AddRange(variableValues.SelectMany(x => byteTranslation(x)));
                SendAsync(byteData.ToArray(), x => { });
            }

            //if (allVariables.Contains("binaryNavigation"))
            //{
            //    allVariables = allVariables.Where(x => x != "binaryNavigation").ToArray();
            //    // Build and dispatch the binary information, here and quickly....
            //    var pitch = Convert.ToSingle(api.ProcessAPIString("n.pitch"));
            //    var roll = Convert.ToSingle(api.ProcessAPIString("n.roll"));
            //    var heading = Convert.ToSingle(api.ProcessAPIString("n.heading"));
            //    var deltaV = Convert.ToSingle(api.ProcessAPIString("v.verticalSpeed"));
            //    var parts = new List<byte[]>();
            //    parts.Add(new byte[] { 1 });
            //    parts.Add(BitConverter.GetBytes(heading));
            //    parts.Add(BitConverter.GetBytes(pitch));
            //    parts.Add(BitConverter.GetBytes(roll));
            //    parts.Add(BitConverter.GetBytes(deltaV));
            //    if (BitConverter.IsLittleEndian) parts = parts.Select(x => x.Reverse().ToArray()).ToList();
            //    var byteData = parts.SelectMany(x => x).ToArray();
            //    SendAsync(byteData, x => { });
            //    PluginLogger.print(string.Format("Send byte data for {0}, {1}, {2}, {3}", heading, pitch, roll, deltaV));
            //}

            var data = SimpleJson.SimpleJson.SerializeObject(apiResults);
            // Now, if we have data send a message, otherwise send a null message
            readyToSend = false;
            try
            {
               SendAsync(data, (b) => readyToSend = true);
                dataRates.SendDataToClient(data.Length);
            }
            catch (Exception ex)
            {
                PluginLogger.print("Caught " + ex.ToString());
            }
            finally
            {
                readyToSend = true;
            }
        }
    }
}
