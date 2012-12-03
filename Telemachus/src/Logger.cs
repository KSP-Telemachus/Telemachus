using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Telemachus
{
    class Logger
    {
        public static void Log(String s)
        {
#if (DEBUG)
            Debug.Log(s);
#endif
        }

        public static void Out(String s)
        {
            Debug.Log(s);
        }

        public static string FormatClassName(string s)
        {
            return "[" + s + "]";
        }

        public static string ListToString(List<int> l)
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

        public static void printParts(List<Part> parts)
        {
            foreach (Part part in parts)
            {
                Log(part.partInfo.name);
            }
        }
    }
}
