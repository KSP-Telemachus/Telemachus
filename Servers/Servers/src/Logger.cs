//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Servers
{
    public class Logger
    {
        public static void debug(String s)
        {
#if (DEBUG)
            Console.WriteLine("[Server Debug] " + s);
#endif
        }

        public static void fine(String s)
        {
#if (DEBUG && FINE)
            Console.WriteLine("[Server Fine]" + s);
#endif
        }

        public static void print(String s)
        {
            Console.WriteLine("[Server] " + s);
        }

        public static string listToString(List<int> l)
        {
            string ret = "[";
            foreach (int i in l)
            {
                ret += i + ",";
            }

            ret = ret.Remove(ret.LastIndexOf(","));
            ret += "]";

            return ret;
        }
    }
}
