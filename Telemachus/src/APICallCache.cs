using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

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
