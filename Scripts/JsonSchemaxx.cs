using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniJson
{
    [Flags]
    public enum PropertyExportFlags
    {
        None,
        PublicFields = 1,
        PublicProperties = 2,

        Default = PublicFields | PublicProperties,
    }

    public class JsonSchemaProperty
    {
        public string Type { get; private set; }
        public string Description { get; private set; }
        public object Minimum { get; private set; }
        public bool Required { get; private set; }

        public JsonSchemaProperty(string type, JsonSchemaAttribute a = null)
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

    public class JsonSchema
    {
        public string Title { get; private set; }
        public string Type { get; private set; }
        public Dictionary<string, JsonSchemaProperty> Properties { get; private set; }
        public string[] Required { get; private set; }

        public static JsonSchema Create<T>()
        {
            return Create(typeof(T));
        }

        static readonly Dictionary<Type, string> JsonSchemaTypeMap = new Dictionary<Type, string>
        {
            {typeof(string), "string"},
            {typeof(int), "integer"},
        };

        static string GetJsonType(Type t)
        {
            string name;
            if (JsonSchemaTypeMap.TryGetValue(t, out name))
            {
                return name;
            }

            if (t.IsClass)
            {
                return "object";
            }

            throw new NotImplementedException(t.Name);
        }

        static IEnumerable<KeyValuePair<string, JsonSchemaProperty>> GetProperties(Type t, PropertyExportFlags exportFlags)
        {
            foreach (var fi in t.GetFields())
            {
                var a = fi.GetCustomAttributes(typeof(JsonSchemaAttribute), true).FirstOrDefault() as JsonSchemaAttribute;
                if (a != null)
                {
                    yield return new KeyValuePair<string, JsonSchemaProperty>(fi.Name, new JsonSchemaProperty(GetJsonType(fi.FieldType), a));
                }
                else if (exportFlags.HasFlag(PropertyExportFlags.PublicFields))
                {
                    yield return new KeyValuePair<string, JsonSchemaProperty>(fi.Name, new JsonSchemaProperty(GetJsonType(fi.FieldType)));
                }
            }

            foreach (var pi in t.GetProperties())
            {
                var a = pi.GetCustomAttributes(typeof(JsonSchemaAttribute), true).FirstOrDefault() as JsonSchemaAttribute;
                if (a != null)
                {
                    yield return new KeyValuePair<string, JsonSchemaProperty>(pi.Name, new JsonSchemaProperty(GetJsonType(pi.PropertyType), a));
                }
                if (exportFlags.HasFlag(PropertyExportFlags.PublicProperties))
                {
                    yield return new KeyValuePair<string, JsonSchemaProperty>(pi.Name, new JsonSchemaProperty(GetJsonType(pi.PropertyType)));
                }
            }
        }

        public static JsonSchema Create(Type t, PropertyExportFlags exportFlags = PropertyExportFlags.Default)
        {
            var props = GetProperties(t, exportFlags).ToArray();
            return new JsonSchema
            {
                Title = t.Name,
                Type = GetJsonType(t),
                Properties = props.ToDictionary(x => x.Key, x => x.Value),
                Required = props.Where(x => x.Value.Required).Select(x => x.Key).ToArray(),
            };
        }

        public static JsonSchema Parse(Byte[] bytes)
        {
            var json = Encoding.UTF8.GetString(bytes);
            var parsed=JsonParser.Parse(json);

            return new JsonSchema
            {

            };
        }
    }
}
