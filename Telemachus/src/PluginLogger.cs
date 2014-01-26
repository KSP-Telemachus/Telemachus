//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Telemachus
{
    public class PluginLogger
    {
        public static void debug(String s)
        {
#if (DEBUG)
            Console.WriteLine(s);
            UnityEngine.Debug.Log("[Telemachus Debug] " +  s);
#endif
        }

        public static void fine(String s)
        {
#if (DEBUG && FINE)
            UnityEngine.Debug.Log("[Telemachus Fine]" + s);
            Console.WriteLine(s);
#endif
        }

        public static void print(String s)
        {
            UnityEngine.Debug.Log("[Telemachus] " + s);
            Console.WriteLine(Assembly.GetExecutingAssembly().FullName);
        }

        public static void printParts(List<Part> parts)
        {
            foreach (Part part in parts)
            {
                debug(part.partInfo.name);
            }
        }
    }
}
