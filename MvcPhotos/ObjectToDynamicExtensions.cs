using System.Collections.Generic;
using System.Dynamic;

namespace MvcPhotos
{
    public static class ObjectToDynamicExtensions
    {
        public static dynamic ToDynamic(this object obj)
        {
            dynamic model = new ExpandoObject();
            var properties = obj.GetType().GetProperties();
            foreach (var property in properties)
            {
                ((IDictionary<string, object>)model)[property.Name] = property.GetValue(obj, null);
            }
            return model;
        }
    }
}