//Author: Richard Bunt
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServersTest
{
    class Program
    {
        static Dictionary<string, ITest> tests = new Dictionary<string, ITest>();

        public static int Main(String[] args)
        {
            buildTests();

            foreach (string s in args)
            {
                tests[s].run();
            }

            return 0;
        }

        private static void buildTests()
        {
            tests["WebServer"] = new MinimalWebServerTest();
            tests["WebSocket"] = new MinimalWebSocketServerTest();
            tests["SocketStealing"] = new SocketStealingTest();
        }
    }

    public interface ITest
    {
        void run();
    }
}
