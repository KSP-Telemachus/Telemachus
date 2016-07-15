//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;

namespace TelemachusTest
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

            Console.Read();
            return 0;
        }

        private static void buildTests()
        {
            tests["HTTPTimeTest"] = new HTTPTimeTest(500);
            tests["GenerateAPIDocumentation"] = new GenerateAPIDocumentation(); 
        }
    }

    public interface ITest
    {
        void run();
    }
}
