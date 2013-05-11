//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using Servers.MinimalHTTPServer;
using System.Threading;
using System.Reflection;

namespace Telemachus
{
    public class DataLinkResponsibility : IHTTPRequestResponsibility
    {
        #region Constants

        public const String PAGE_PREFIX = "/telemachus/datalink";
        public const char ARGUMENTS_START = '?';
        public const char ARGUMENTS_ASSIGN = '=';
        public const char ARGUMENTS_DELIMETER = '&';
        public const char ACCESS_DELIMITER = '.';

        #endregion

        #region Fields
        
        List<DataLinkHandler> APIHandlers = new List<DataLinkHandler>();
        DataSources dataSources = new DataSources();
        VesselChangeDetector vesselChangeDetector = new VesselChangeDetector();

        #endregion

        #region Data Rate Fields

        public const int RATE_AVERAGE_SAMPLE_SIZE = 20;
        public UpLinkDownLinkRate dataRates { get { return itsDataRates; } set { itsDataRates = value; } }
        private UpLinkDownLinkRate itsDataRates = new UpLinkDownLinkRate(RATE_AVERAGE_SAMPLE_SIZE);

        #endregion

        #region Initialisation

        public DataLinkResponsibility(FormatterProvider formatters)
        {
            loadHandlers(formatters);
        }

        private void loadHandlers(FormatterProvider formatters)
        {
            APIHandlers.Add(new PausedDataLinkHandler(formatters));
            APIHandlers.Add(new FlyByWireDataLinkHandler(formatters));
            APIHandlers.Add(new FlightDataLinkHandler(formatters));
            APIHandlers.Add(new MechJebDataLinkHandler(formatters));
            APIHandlers.Add(new TimeWarpDataLinkHandler(formatters));

            APIHandlers.Add(new CompoundDataLinkHandler(
                new List<DataLinkHandler>() { 
                    new OrbitDataLinkHandler(formatters),
                    new SensorDataLinkHandler(vesselChangeDetector, formatters),
                    new VesselDataLinkHandler(formatters),
                    new BodyDataLinkHandler(formatters),
                    new ResourceDataLinkHandler(vesselChangeDetector, formatters),
                    new APIDataLinkHandler(this,formatters),
                    new NavBallDataLinkHandler(formatters)
                    }, formatters
                ));

            APIHandlers.Add(new DefaultDataLinkHandler(formatters));
        }

        #endregion

        #region IHTTPRequestResponsibility

        public bool process(Servers.AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            if (request.path.StartsWith(PAGE_PREFIX))
            {
                dataRates.addUpLinkPoint(System.DateTime.Now, request.path.Length);

                try
                {
                    dataSources.vessel = FlightGlobals.ActiveVessel;
                    vesselChangeDetector.update(FlightGlobals.ActiveVessel);
                }
                catch (Exception e)
                {
                    PluginLogger.debug(e.Message + " "  + e.StackTrace);
                }

                dataRates.addDownLinkPoint(
                    System.DateTime.Now, 
                    ((Servers.MinimalHTTPServer.ClientConnection)cc).Send(new OKResponsePage(
                        argumentsParse(request.path.Remove(0, 
                            request.path.IndexOf(ARGUMENTS_START) + 1), 
                            dataSources)
                   )));

                return true;
            }

            return false;
        }

        #endregion

        #region API

        public void getAPIList(ref List<APIEntry> APIList)
        {
            foreach (DataLinkHandler APIHandler in APIHandlers)
            {
                APIHandler.appendAPIList(ref APIList);
            }

        }

        public void getAPIEntry(string APIString, ref List<APIEntry> APIList)
        {
            APIEntry result = null;

            foreach (DataLinkHandler APIHandler in APIHandlers)
            {
                if (APIHandler.process(APIString, out result))
                {
                    break;
                }
            }

            APIList.Add(result);
        }
            
        #endregion

        #region Parse URL

        private String argumentsParse(String args, DataSources dataSources)
        {

            APIEntry currentEntry = null;
            List<string> APIResults = new List<string>(); 
            String[] argsSplit = args.Split(ARGUMENTS_DELIMETER);

            foreach (String arg in argsSplit)
            {
                string refArg = arg;
                PluginLogger.fine(refArg);
                parseParams(ref refArg, ref dataSources);
                currentEntry = argumentParse(refArg, dataSources);
                APIResults.Add(currentEntry.formatter.format(currentEntry.function(dataSources)));

                //Only parse the paused argument if the active vessel is null
                if (dataSources.vessel == null)
                {
                    break;
                }
            }

            return currentEntry.formatter.pack(APIResults);
        }

        private void parseParams(ref String arg, ref DataSources dataSources)
        {
            dataSources.args.Clear();

            try
            {
                if (arg.Contains("["))
                {
                    
                    String[] argsSplit = arg.Split('[');
                    argsSplit[1] = argsSplit[1].Substring(0, argsSplit[1].Length - 1);
                    arg = argsSplit[0];
                    String[] paramSplit = argsSplit[1].Split(',');

                    for (int i = 0; i < paramSplit.Length; i++)
                    {
                        dataSources.args.Add(paramSplit[i]);
                    }
                }
            }
            catch (Exception e)
            {
                PluginLogger.debug(e.Message + " " + e.StackTrace);
            }
        }

        private APIEntry argumentParse(String args, DataSources dataSources)
        {
            String[] argsSplit = args.Split(ARGUMENTS_ASSIGN);
            APIEntry result = null;

            foreach (DataLinkHandler APIHandler in APIHandlers)
            {
                if (APIHandler.process(argsSplit[1], out result))
                {
                    break;
                }
            }

            result.formatter.setVarName(argsSplit[0]);
            return result;
        }

        #endregion
    }

    public class DataSources
    {
        #region Fields

        public Vessel vessel;
        public List<String> args = new List<String>();

        #endregion
    }
}
