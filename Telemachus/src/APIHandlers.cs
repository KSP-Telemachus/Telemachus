using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Telemachus
{
    public class ReflectiveAPIHandler : IAPIHandler
    {
        const int FIELD_ACCESS = 1, PROPERTY_ACCESS = 2;

        DataLink dataLinks = null;

        public ReflectiveAPIHandler(DataLink dataLinks)
        {
            this.dataLinks = dataLinks;
        }

        public bool process(String API, String assign, ref Dictionary<string, CachedAPIReference> cache, ref CachedAPIReference cachedAPIReference)
        {

            String[] argsSplit = API.Split(DataLinkResponsibility.ACCESS_DELIMITER);
            Type type = dataLinks.GetType();
            Object value = dataLinks, parentValue = null;
            FieldInfo field = null;
            PropertyInfo property = null;
            int accessType = 0;
            int i = 0;

            for (i = 0; i < argsSplit.Length; i++)
            {
                try
                {
                    field = type.GetField(argsSplit[i]);
                    parentValue = value;
                    value = field.GetValue(parentValue);
                    accessType = FIELD_ACCESS;
                }
                catch
                {
                    accessType = PROPERTY_ACCESS;
                    property = type.GetProperty(argsSplit[i]);
                    parentValue = value;
                    value = property.GetValue(parentValue, null);
                }

                type = value.GetType();
            }

            switch (accessType)
            {
                case FIELD_ACCESS:
                    cachedAPIReference = new FieldCachedAPIReference(field, parentValue, new JavaScriptGeneralFormatter());
                    cache.Add(API, cachedAPIReference);
                    return true;

                case PROPERTY_ACCESS:
                    cachedAPIReference = new PropertyCachedAPIReference(property, parentValue, new JavaScriptGeneralFormatter());
                    cache.Add(API, cachedAPIReference);
                    return true;

                default:
                    return false;
            }
        }
    }

    public interface IAPIHandler
    {
        bool process(String API, String assign, ref Dictionary<string, CachedAPIReference> cache, ref CachedAPIReference cachedAPIReference);
    }

    public class DefaultAPIHandler : IAPIHandler
    {
        public bool process(String API, String assign, ref Dictionary<string, CachedAPIReference> cache, ref CachedAPIReference cachedAPIReference)
        {
            throw new Exception("Bad API reference");
        }
    }
}
