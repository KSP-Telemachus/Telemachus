//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using MinimalHTTPServer;
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
        public const int INFINITE_WAIT = -1;

        #endregion

        #region Fields
        
        List<DataLinkHandler> APIHandlers = new List<DataLinkHandler>();
        DataSources dataSources = new DataSources();
        DataSourceResultFormatter resultFormatter = new JavaScriptGeneralFormatter();
        VesselChangeDetector vesselChangeDetector = new VesselChangeDetector();

        #endregion

        #region Data Rate Fields

        public const int RATE_AVERAGE_SAMPLE_SIZE = 20;
        public UpLinkDownLinkRate dataRates { get { return itsDataRates; } set { itsDataRates = value; } }
        private UpLinkDownLinkRate itsDataRates = new UpLinkDownLinkRate(RATE_AVERAGE_SAMPLE_SIZE);

        #endregion

        #region Initialisation

        public DataLinkResponsibility()
        {
            loadHandlers();
        }

        private void loadHandlers()
        {
            APIHandlers.Add(new PausedDataLinkHandler());
            APIHandlers.Add(new OrbitDataLinkHandler());
            APIHandlers.Add(new SensorDataLinkHandler(vesselChangeDetector));
            APIHandlers.Add(new VesselDataLinkHandler());
            APIHandlers.Add(new FlightDataLinkHandler());
            APIHandlers.Add(new DefaultDataLinkHandler());
        }

        #endregion

        #region IHTTPRequestResponsibility

        public bool process(AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            if (request.path.StartsWith(PAGE_PREFIX))
            {
                dataRates.addUpLinkPoint(System.DateTime.Now, request.path.Length);
                
                dataSources.vessel = FlightGlobals.ActiveVessel;
                vesselChangeDetector.update(FlightGlobals.ActiveVessel);

                String returnMessage = new OKPage(
                   argumentsParse(
                   request.path.Remove(
                   0, request.path.IndexOf(ARGUMENTS_START) + 1), dataSources)
                   ).ToString();
                cc.Send(returnMessage);

                dataRates.addDownLinkPoint(System.DateTime.Now, returnMessage.Length);

                return true;
            }

            return false;
        }

        #endregion

        #region Parse URL

        private String argumentsParse(String args, DataSources dataSources)
        {
            StringBuilder sb = new StringBuilder();
            String[] argsSplit = args.Split(ARGUMENTS_DELIMETER);

            foreach (String arg in argsSplit)
            {
                sb.Append(argumentParse(arg, dataSources));

                //Only parse the paused argument if the active vessel is null
                if (dataSources.vessel == null)
                {
                    break;
                }
            }

            return sb.ToString();
        }

        private String argumentParse(String args, DataSources dataSources)
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
            
            object r = result.function(dataSources);
            return resultFormatter.formatWithAssignment(
                resultFormatter.format(r, r.GetType()), argsSplit[0]);
        }

        #endregion
    }

    public class DataSources
    {
        #region Fields

        public Vessel vessel;

        #endregion
    }
}
