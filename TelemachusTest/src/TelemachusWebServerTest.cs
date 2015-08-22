//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;

namespace TelemachusTest
{
    class HTTPTimeTest : ITest
    {
        private int numberOfSamples = 0;

        public HTTPTimeTest(int numberOfSamples)
        {
            this.numberOfSamples = numberOfSamples;
        }

        public void run()
        {
            Console.WriteLine("Starting time test: " + numberOfSamples);
            Stopwatch timer = new Stopwatch();
            try
            {
                string address = string.Format(
                "http://127.0.0.1:8085/telemachus/datalink?alt={0}",
                Uri.EscapeDataString("v.altitude"));
                string text;
                using (WebClient client = new WebClient())
                {
                    timer.Start();
                    for (int i = 0; i < numberOfSamples; i++)
                    {
                        text = client.DownloadString(address);
                    }
                    timer.Stop();
                }

                Console.WriteLine("Average request time: " + timer.ElapsedMilliseconds / ((double)(numberOfSamples)) + " milliseconds.");
            }
            catch (Exception d)
            {
                Console.WriteLine(d.Message);
            }
        }
    }
}
