//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Telemachus
{
    public class JavaScriptGeneralFormatter : DataSourceResultFormatter
    {
        protected const String JAVASCRIPT_DELIMITER = ";";
        protected const String JAVASCRIPT_ASSIGN = " = ";

        public String format(object input, Type type)
        {
            if (type.Name.Equals("String"))
            {
                return "'" + input.ToString() + "'" + JAVASCRIPT_DELIMITER;
            }
            else if (type.Name.Equals("Vector3d"))
            {
                Vector3d vector = (Vector3d)input;
                return "[" +  vector.x + "," + vector.y + "," + vector.z + "]" + JAVASCRIPT_DELIMITER;
            }
            else if (type.Name.Equals("Boolean"))
            {
                return input.ToString().ToLower() + JAVASCRIPT_DELIMITER;
            }
            else if (input.ToString().Equals("System.Collections.Generic.List`1[ModuleEnviroSensor]"))
            {
                List<ModuleEnviroSensor> sensors = (List<ModuleEnviroSensor>)input;
                if (sensors.Count > 0)
                {
                    StringBuilder sb0 = new StringBuilder();
                    StringBuilder sb1 = new StringBuilder();
                    sb0.Append("[");
                    sb1.Append("[");

                    foreach (ModuleEnviroSensor m in sensors)
                    {
                        if (!m.isEnabled)
                        {
                            sb0.Append("0,");
                        }
                        else
                        {
                            float f = 0;

                            try
                            {
                                float.TryParse(Regex.Replace(m.readoutInfo, @"([0-9]+\.?[0-9]*).*", "$1"), out f);
                            }
                            catch
                            {
                                f = 0;
                            }

                            sb0.Append(f.ToString() + ",");
                        }

                        try
                        {
                            sb1.Append("'" + (m.isEnabled ? "" : "Inactive ") + m.part.parent.name +
                                 "',");
                        }
                        catch
                        {
                            sb1.Append("'Unavailable',");
                        }
                    }

                    if (sb0.Length > 1)
                    {
                        sb0.Remove(sb0.Length - 1, 1);
                    }

                    if (sb1.Length > 1)
                    {
                        sb1.Remove(sb1.Length - 1, 1);
                    }

                    sb0.Append("]");
                    sb1.Append("]");

                    return "[" + sb1.ToString() + "," + sb0.ToString() + "];";
                }
                else
                {
                    return "[['No Sensors of the Appropriate Type'], [0]];";
                }
            }
            else
            {
                return input.ToString() + JAVASCRIPT_DELIMITER;
            }
        }

        public String formatWithAssignment(String input, String varName)
        {
            return varName + " " + JAVASCRIPT_ASSIGN + " " + input;
        }
    }

    public interface DataSourceResultFormatter
    {
        String format(object input, Type type);
        String formatWithAssignment(String input, String varName);
    }
}
