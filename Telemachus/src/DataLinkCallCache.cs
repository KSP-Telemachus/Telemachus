using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using MinimalHTTPServer;

namespace Telemachus
{
    public class FieldCachedDataLinkReference : CachedDataLinkReference
    {
        FieldInfo field = null;

        public FieldCachedDataLinkReference(FieldInfo field, Object parentValue, ResultFormatter resultFormatter)
            : base(parentValue, resultFormatter)
        {
            this.field = field;
        }

        public override String getValue()
        {
            Object value = field.GetValue(parentValue);
            return resultFormatter.format(value.ToString(), value.GetType());
        }
    }

    public class PropertyCachedDataLinkReference : CachedDataLinkReference
    {
        PropertyInfo property = null;

        public PropertyCachedDataLinkReference(PropertyInfo property, Object parentValue, ResultFormatter resultFormatter)
            : base(parentValue, resultFormatter)
        {
            this.property = property;
        }

        public override String getValue()
        {
            Object value = property.GetValue(parentValue, null);
            return resultFormatter.format(value.ToString(), value.GetType());
        }
    }

    public class SensorCachedDataLinkReference : CachedDataLinkReference
    {
        List<ModuleEnviroSensor> sensors = null;

        public SensorCachedDataLinkReference(List<ModuleEnviroSensor> sensors, ResultFormatter resultFormatter)
            : base(null, resultFormatter)
        {
            this.sensors = sensors;
        }

        public override String getValue()
        {
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
                        sb1.Append("'" + (m.isEnabled ? "" : "(Inactive) ") + "Sensor location: " + m.part.parent.name  +
                             "',");
                    }
                    catch
                    {
                        sb1.Append("'(Part Unavailable)',");
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
                throw new SoftException("There are no sensors on this craft able to supply this data.");
            }
        }
    }

    public abstract class CachedDataLinkReference
    {
        protected Object parentValue = null;
        protected ResultFormatter resultFormatter = null;

        public CachedDataLinkReference(Object parentValue, ResultFormatter resultFormatter)
        {
            this.parentValue = parentValue;
            this.resultFormatter = resultFormatter;
        }

        public abstract String getValue();
    }
}
