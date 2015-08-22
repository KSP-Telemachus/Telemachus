//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Telemachus;

namespace TelemachusTest
{
    class GenerateAPIDocumentation : ITest
    {
        public GenerateAPIDocumentation()
        {

        }

        public void run()
        {
            ServerConfiguration config = new ServerConfiguration();
            Telemachus.VesselChangeDetector vesselChangeDetector = new Telemachus.VesselChangeDetector(false);
            Telemachus.KSPAPI api = new Telemachus.KSPAPI(Telemachus.JSONFormatterProvider.Instance, vesselChangeDetector, config, null);

            List<Telemachus.APIEntry> apiList = new List<Telemachus.APIEntry>();
            api.getAPIList(ref apiList);

            writeToMarkdownWiki(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Telemachus.wiki"),
                "API-String.md", ref apiList);
        }

        private void writeToMarkdownWiki(string wikiPath, string fileName, ref List<Telemachus.APIEntry> apiList)
        {
            MDFile file = new MDFile(Path.Combine(wikiPath, fileName));

            string intro = "_API strings_ are used to identify values or functions within KSP, such as" +
                "v.altitude which is mapped to the altitude of the vessel you are currently flying." +
                "The API strings which are currently supported are listed below.";
            
            file.write(intro);
            foreach (Telemachus.APIEntry apiEntry in apiList)
            {
                file.writeHeading(apiEntry.APIString);
                file.writeBody(apiEntry.name);
                file.writeBody(apiEntry.plotable ? "Plotable" : "Not Plotable");
            }

            file.close();
        }
    }

    public class MDFile
    {
        private StreamWriter mdFile = null;
        
        private const string TITLE_MARKER = "# ";
        private const string SUB_MARKER = "## ";
        private const string SUB_SUB_MARKER = "### ";

        public MDFile(string fileName)
        {
            mdFile = new StreamWriter(fileName);
        }

        public void close()
        {
            mdFile.Close();
        }

        public void writeTitle(string s)
        {
            mdFile.WriteLine(TITLE_MARKER + s);
        }

        public void writeHeading(string s)
        {
            mdFile.WriteLine("\n" + SUB_SUB_MARKER + s);
        }

        public void writeBody(string s)
        {
            mdFile.WriteLine("* " + s);
        }

        public void write(string s)
        {
            mdFile.WriteLine(s);
        }

    }
}
