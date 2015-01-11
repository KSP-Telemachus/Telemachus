//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Telemachus
{
    public interface DataSourceResultFormatter
    {
        string format(object input, string varName);
        string pack(List<String> list);
    }

    public abstract class FormatterProvider
    {
        public DataSourceResultFormatter SensorModuleList { get; set; }
        public DataSourceResultFormatter ResourceList { get; set; }
        public DataSourceResultFormatter CurrentResourceList { get; set; }
        public DataSourceResultFormatter ActiveResourceList { get; set; }
        public DataSourceResultFormatter MaxResourceList { get; set; }
        public DataSourceResultFormatter MaxCurrentResourceList { get; set; }
        public DataSourceResultFormatter APIEntry { get; set; }
        public DataSourceResultFormatter String { get; set; }
        public DataSourceResultFormatter Vector3d { get; set; }
        public DataSourceResultFormatter Default { get; set; }
        public DataSourceResultFormatter StringArray { get; set; }
    }

    public class JSONFormatterProvider : FormatterProvider
    {
        private static JSONFormatterProvider instance;
        public static JSONFormatterProvider Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new JSONFormatterProvider();
                }
                return instance;
            }
        }

        private JSONFormatterProvider()
        {
            SensorModuleList = new SensorModuleListJSONFormatter();
            ResourceList = new ResourceListJSONFormatter();
            MaxResourceList = new MaxResourceListJSONFormatter();
            ActiveResourceList = new ActiveResourceListJSONFormatter();
            CurrentResourceList = new CurrentResourceListJSONFormatter();
            MaxCurrentResourceList = new ActiveResourceTotalListJSONFormatter();
            APIEntry = new APIEntryJSONFormatter();
            String = new StringJSONFormatter();
            Vector3d = new Vector3dJSONFormatter();
            StringArray = new APIEntryStringArrayFormatter();
            Default = new DefaultJSONFormatter();
        }

        public abstract class JSONFormatter : DataSourceResultFormatter
        {
            protected const string JSON_ASSIGN = ":";
            protected const string JSON_DELIMITER = ",";
            protected const string JSON_START = "{";
            protected const string JSON_END = "}";
            protected const string JSON_SEPARATE_LIST = ",";
            protected const string JSON_CLOSE_LIST = "]";
            protected const string JSON_OPEN_LIST = "[";

            public string pack(List<string> list)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(JSON_START);

                for (int i = 0; i < list.Count; i++)
                {
                    sb.Append(list[i]);

                    if (i < list.Count - 1)
                    {
                        sb.Append(JSON_DELIMITER);
                    }
                }

                sb.Append(JSON_END);

                return sb.ToString();
            }

            public abstract string format(object input, string varName);
        }

        public class DefaultJSONFormatter : JSONFormatter
        {
            public override string format(object input, string varName)
            {
                return varName + JSON_ASSIGN + input.ToString().ToLower();
            }
        }

        public class Vector3dJSONFormatter : JSONFormatter
        {
            public override string format(object input, string varName)
            {
                Vector3d vector = (Vector3d)input;
                return varName + JSON_ASSIGN + JSON_OPEN_LIST + vector.x + JSON_SEPARATE_LIST + vector.y +
                    JSON_SEPARATE_LIST + vector.z + JSON_CLOSE_LIST;
            }
        }

        public class StringJSONFormatter : JSONFormatter
        {
            public override string format(object input, string varName)
            {
                return varName + JSON_ASSIGN + "\"" + input.ToString() + "\"";
            }
        }

        public class APIEntryJSONFormatter : JSONFormatter
        {
            public override string format(object input, string varName)
            {
                StringBuilder sb = new StringBuilder();
                List<APIEntry> APIList = (List<APIEntry>)input;

                sb.Append("[");

                for (int i = 0; i < APIList.Count; i++)
                {
                    sb.Append("{");
                    sb.Append("\"apistring\": \"" + APIList[i].APIString + "\"");
                    sb.Append(",");
                    sb.Append("\"name\": \"" + APIList[i].name + "\"");
                    sb.Append(",");
                    sb.Append("\"units\": \"" + APIList[i].units + "\"");
                    sb.Append(",");
                    sb.Append("\"plotable\": " + APIList[i].plotable.ToString().ToLower());
                    sb.Append("}");

                    if (i < APIList.Count - 1)
                    {
                        sb.Append(",");
                    }
                }

                sb.Append("]");

                return varName + JSON_ASSIGN + sb.ToString();
            }
        }

        public class APIEntryStringArrayFormatter : JSONFormatter
        {
            public override string format(object input, string varName)
            {
                StringBuilder sb = new StringBuilder();
                List<String> APIList = (List<String>)input;

                sb.Append("[");

                for (int i = 0; i < APIList.Count; i++)
                {
                    sb.Append("\"" + APIList[i] + "\"");


                    if (i < APIList.Count - 1)
                    {
                        sb.Append(",");
                    }
                }

                sb.Append("]");

                return varName + JSON_ASSIGN + sb.ToString();
            }
        }

        public class ResourceListJSONFormatter : JSONFormatter
        {
            public override string format(object input, string varName)
            {
                List<PartResource> resources = (List<PartResource>)input;

                if (resources.Count > 0)
                {
                    double amount = 0d;

                    foreach (PartResource p in resources)
                    {
                        amount += p.amount;
                    }

                    return varName + JSON_ASSIGN + amount.ToString();
                }

                return varName + JSON_ASSIGN + "-1";
            }
        }

        public class CurrentResourceListJSONFormatter : JSONFormatter
        {
            public override string format(object input, string varName)
            {
                List<PartResource> resources = (List<PartResource>)input;

                if (resources.Count > 0)
                {
                    double amount = 0d;

                    foreach (PartResource p in resources)
                    {
                        if (p.part.inStageIndex == Staging.CurrentStage)
                        {
                            amount += p.amount;
                        }
                    }

                    return varName + JSON_ASSIGN + amount.ToString();
                }

                return varName + JSON_ASSIGN + "-1";
            }
        }

        public class ActiveResourceListJSONFormatter : JSONFormatter
        {
            public override string format(object input, string varName)
            {
                List<Vessel.ActiveResource> resources = (List<Vessel.ActiveResource>)input;

                if (resources.Count > 0)
                {
                    double amount = 0d;

                    foreach (Vessel.ActiveResource p in resources)
                    {
                        amount += p.amount;
                    }

                    return varName + JSON_ASSIGN + amount.ToString();
                }

                return varName + JSON_ASSIGN + "-1";
            }
        }

        public class ActiveResourceTotalListJSONFormatter : JSONFormatter
        {
            public override string format(object input, string varName)
            {
                List<Vessel.ActiveResource> resources = (List<Vessel.ActiveResource>)input;

                if (resources.Count > 0)
                {
                    double amount = 0d;

                    foreach (Vessel.ActiveResource p in resources)
                    {
                        amount += p.maxAmount;
                    }

                    return varName + JSON_ASSIGN + amount.ToString();
                }

                return varName + JSON_ASSIGN + "-1";
            }
        }

        public class MaxResourceListJSONFormatter : JSONFormatter
        {
            public override string format(object input, string varName)
            {
                List<PartResource> resources = (List<PartResource>)input;

                if (resources.Count > 0)
                {
                    double amount = 0d;

                    foreach (PartResource p in resources)
                    {
                        amount += p.maxAmount;
                    }

                    return varName + JSON_ASSIGN + amount.ToString();
                }

                return varName + JSON_ASSIGN + "-1";
            }
        }

        public class SensorModuleListJSONFormatter : JSONFormatter
        {
            public override string format(object input, string varName)
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
                            sb1.Append("\"" + (m.isEnabled ? "" : "Inactive ") + m.part.parent.name +
                                    "\",");
                        }
                        catch
                        {
                            sb1.Append("\"Unavailable\",");
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

                    return varName + JSON_ASSIGN + "[" + sb1.ToString() + "," + sb0.ToString() + "]";
                }
                else
                {
                    return varName + JSON_ASSIGN + "[[\"No Sensors of the Appropriate Type\"], [0]]";
                }
            }
        }
    }
}
