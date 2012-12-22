//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;

namespace Telemachus
{
    public class JavaScriptGeneralFormatter : ResultFormatter
    {
        protected const String JAVASCRIPT_DELIMITER = ";";
        protected const String JAVASCRIPT_ASSIGN = " = ";

        public String format(String input, Type type)
        {
            if (type.Name.Equals("String"))
            {
                return "'" + input.ToString() + "'" + JAVASCRIPT_DELIMITER;
            }
            else if (type.Name.Equals("Boolean"))
            {
                return input.ToString().ToLower() + JAVASCRIPT_DELIMITER;
            }
            else
            {
                return input.ToString() + JAVASCRIPT_DELIMITER;
            }
        }

        public static String formatWithAssignment(String input, String varName)
        {
            return varName + " " + JAVASCRIPT_ASSIGN + " " + input;
        }
    }

    public interface ResultFormatter
    {
        String format(String input, Type type);
    }
}
