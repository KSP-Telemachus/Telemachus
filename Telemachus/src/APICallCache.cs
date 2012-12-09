using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Telemachus
{
    public class FieldCachedAPIReference : CachedAPIReference
    {
        FieldInfo field = null;

        public FieldCachedAPIReference(FieldInfo field, Object parentValue, ResultFormatter resultFormatter)
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

    public class PropertyCachedAPIReference : CachedAPIReference
    {
        PropertyInfo property = null;

        public PropertyCachedAPIReference(PropertyInfo property, Object parentValue, ResultFormatter resultFormatter)
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

    public class SensorCachedAPIReference : CachedAPIReference
    {
        List<ModuleEnviroSensor> sensors = null;

        public SensorCachedAPIReference(List<ModuleEnviroSensor> sensors, ResultFormatter resultFormatter)
            : base(null, resultFormatter)
        {
            this.sensors = sensors;
        }

        public override String getValue()
        {
            StringBuilder sb0 = new StringBuilder();
            StringBuilder sb1 = new StringBuilder();
            sb0.Append("[");
            sb1.Append("[");
            foreach (ModuleEnviroSensor m in sensors)
            {
                if (m == null || !m.isEnabled)
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

                sb1.Append(m.part.parent.name + ",");
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

            return "[" + sb1.ToString() + "," + sb0.ToString() + "];" ;
        }
    }

    public abstract class CachedAPIReference
    {
        protected Object parentValue = null;
        protected ResultFormatter resultFormatter = null;

        public CachedAPIReference(Object parentValue, ResultFormatter resultFormatter)
        {
            this.parentValue = parentValue;
            this.resultFormatter = resultFormatter;
        }

        public abstract String getValue();
    }
}
