using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TelemachusTest
{
    class Program
    {
        static void Main(string[] args)
        {
            time(2000);
        }

        static private void time(int numberOfTests)
        {
            Console.WriteLine("Starting time test: " + numberOfTests);
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
                    for (int i = 0; i < numberOfTests; i++)
                    {
                        text = client.DownloadString(address);
                    }
                    timer.Stop();
                }

                Console.WriteLine("Average request time: " + timer.ElapsedMilliseconds / ((double)(numberOfTests)) + " milliseconds.");
            }
            catch (Exception d)
            {
                Console.WriteLine(d.Message);
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}
