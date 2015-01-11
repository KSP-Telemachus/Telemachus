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

        //DataSources dataSources = null;
        IKSPAPI kspAPI = null;

        #endregion

        #region Data Rate Fields

        public const int RATE_AVERAGE_SAMPLE_SIZE = 20;
        public UpLinkDownLinkRate dataRates { get { return itsDataRates; } set { itsDataRates = value; } }
        private UpLinkDownLinkRate itsDataRates = new UpLinkDownLinkRate(RATE_AVERAGE_SAMPLE_SIZE);

        #endregion

        #region Initialisation

        public DataLinkResponsibility(Servers.AsynchronousServer.ServerConfiguration serverConfiguration, IKSPAPI kspAPI)
        {
            this.kspAPI = kspAPI;
        }

        #endregion

        #region IHTTPRequestResponsibility

        public bool process(Servers.AsynchronousServer.ClientConnection cc, HTTPRequest request)
        {
            DataSources dataSources = new DataSources();

            if (request.path.StartsWith(PAGE_PREFIX))
            {
                if (request.requestType == HTTPRequest.GET)
                {
                    dataRates.addUpLinkPoint(System.DateTime.Now, request.path.Length * UpLinkDownLinkRate.BITS_PER_BYTE);
                }
                else if (request.requestType == HTTPRequest.POST)
                {
                    dataRates.addUpLinkPoint(System.DateTime.Now, request.content.Length * UpLinkDownLinkRate.BITS_PER_BYTE);
                }

                try
                {
                    dataSources.vessel = kspAPI.getVessel();
                }
                catch (Exception e)
                {
                    PluginLogger.debug(e.Message + " " + e.StackTrace);
                }

                if (request.requestType == HTTPRequest.GET)
                {
                    dataRates.addDownLinkPoint(
                        System.DateTime.Now,
                        ((Servers.MinimalHTTPServer.ClientConnection)cc).Send(new OKResponsePage(
                            argumentsParse(request.path.Remove(0,
                                request.path.IndexOf(ARGUMENTS_START) + 1),
                                dataSources)
                        )) * UpLinkDownLinkRate.BITS_PER_BYTE);
                }
                else if (request.requestType == HTTPRequest.POST)
                {
                    dataRates.addDownLinkPoint(
                        System.DateTime.Now,
                        ((Servers.MinimalHTTPServer.ClientConnection)cc).Send(new OKResponsePage(
                            argumentsParse(request.content,
                                dataSources)
                        )) * UpLinkDownLinkRate.BITS_PER_BYTE);
                }

                return true;
            }

            return false;
        }

        #endregion

        #region IKSPAPI

        public void getAPIList(ref List<APIEntry> APIList)
        {
            kspAPI.getAPIList(ref APIList);
        }

        public void getAPIEntry(string APIString, ref List<APIEntry> APIList)
        {
            kspAPI.getAPIEntry(APIString, ref APIList);
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
                kspAPI.parseParams(ref refArg, ref dataSources);
                currentEntry = argumentParse(refArg, dataSources);
                APIResults.Add(currentEntry.formatter.format(currentEntry.function(dataSources), dataSources.getVarName()));

                //Only parse the paused argument if the active vessel is null
                if (dataSources.vessel == null)
                {
                    break;
                }
            }

            return currentEntry.formatter.pack(APIResults);
        }

        

        private APIEntry argumentParse(String args, DataSources dataSources)
        {
            String[] argsSplit = args.Split(ARGUMENTS_ASSIGN);
            APIEntry result = null;

            kspAPI.process(argsSplit[1], out result);

            dataSources.setVarName(argsSplit[0]);
            return result;
        }

        #endregion
    }

    public class DataSources
    {
        #region Fields

        public Vessel vessel;
        public List<String> args = new List<String>();
        protected string varName;
        #endregion

        public DataSources Clone()
        {
            DataSources d = new DataSources();
            d.vessel = this.vessel;
            d.args = new List<string>(this.args);
            d.varName = this.getVarName();
            return d;
        }

        public void setVarName(string varName)
        {
            this.varName = "\"" + varName + "\"";
        }

        public string getVarName()
        {
            return varName;
        }
    }
}
