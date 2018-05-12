using System;
using System.Collections.Generic;
using System.Linq;


namespace UniJSON
{
    [Flags]
    public enum PropertyExportFlags
    {
        None,
        PublicFields = 1,
        PublicProperties = 2,

        Default = PublicFields | PublicProperties,
    }

    public class JSONSchemaProperty
    {
        public string Type { get; private set; }
        public string Description { get; private set; }
        public object Minimum { get; private set; }
        public bool Required { get; private set; }

        public JSONSchemaProperty(string type, JSONSchemaAttribute a = null)
        {
            Type = type;
            if (a != null)
            {
                Description = a.Description;
                Minimum = a.Minimum;
                Required = a.Required;
            }
        }
    }

    public class JSONSchema
    {
        public string Title { get; private set; }
        public string Type { get; private set; }
        public Dictionary<string, JSONSchemaProperty> Properties { get; private set; }
        public string[] Required { get; private set; }

        public static JSONSchema Create<T>()
        {
            return Create(typeof(T));
        }

        static readonly Dictionary<Type, string> JSONSchemaTypeMap = new Dictionary<Type, string>
        {
            {typeof(string), "string"},
            {typeof(int), "integer"},
        };

        static string GetJSONType(Type t)
        {
            string name;
            if (JSONSchemaTypeMap.TryGetValue(t, out name))
            {
                return name;
            }

            if (t.IsClass)
            {
                return "object";
            }

            throw new NotImplementedException(t.Name);
        }

        static IEnumerable<KeyValuePair<string, JSONSchemaProperty>> GetProperties(Type t, PropertyExportFlags exportFlags)
        {
            foreach (var fi in t.GetFields())
            {
                var a = fi.GetCustomAttributes(typeof(JSONSchemaAttribute), true).FirstOrDefault() as JSONSchemaAttribute;
                if (a != null)
                {
                    yield return new KeyValuePair<string, JSONSchemaProperty>(fi.Name, new JSONSchemaProperty(GetJSONType(fi.FieldType), a));
                }
                else if (exportFlags.HasFlag(PropertyExportFlags.PublicFields))
                {
                    yield return new KeyValuePair<string, JSONSchemaProperty>(fi.Name, new JSONSchemaProperty(GetJSONType(fi.FieldType)));
                }
            }

            foreach (var pi in t.GetProperties())
            {
                var a = pi.GetCustomAttributes(typeof(JSONSchemaAttribute), true).FirstOrDefault() as JSONSchemaAttribute;
                if (a != null)
                {
                    yield return new KeyValuePair<string, JSONSchemaProperty>(pi.Name, new JSONSchemaProperty(GetJSONType(pi.PropertyType), a));
                }
                if (exportFlags.HasFlag(PropertyExportFlags.PublicProperties))
                {
                    yield return new KeyValuePair<string, JSONSchemaProperty>(pi.Name, new JSONSchemaProperty(GetJSONType(pi.PropertyType)));
                }
            }
        }

        public static JSONSchema Create(Type t, PropertyExportFlags exportFlags = PropertyExportFlags.Default)
        {
            var props = GetProperties(t, exportFlags).ToArray();
            return new JSONSchema
            {
                Title = t.Name,
                Type = GetJSONType(t),
                Properties = props.ToDictionary(x => x.Key, x => x.Value),
                Required = props.Where(x => x.Value.Required).Select(x => x.Key).ToArray(),
            };
        }

        public static JSONSchema Parse(Byte[] bytes)
        {


            return new JSONSchema
            {

            };
        }
    }
}
