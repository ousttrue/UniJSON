using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    public struct JsonSchemaPropertyItem
    {
        public object[] Enum;
        public string Description;
        public string Type;
    }

    public class JsonSchemaProperty
    {
        public string Description { get; private set; }
        public bool Required { get; private set; }

        public string Type { get; private set; }
        public object Minimum { get; private set; }
        public JsonSchemaPropertyItem[] AnyOf { get; private set; }

        public JsonSchemaProperty(string type=null, JsonSchemaPropertyAttributeAttribute a = null)
        {
            Type = type;
            if (a != null)
            {
                Description = a.Description;
                Minimum = a.Minimum;
                Required = a.Required;
            }
        }

        public static JsonSchemaProperty FromEnum(Type enumType)
        {
            var enumValues = Enum.GetValues(enumType).Cast<Object>().Select(x => new JsonSchemaPropertyItem
            {
                Enum = new[] { x.ToString() },
                Description = x.ToString(),
            }).ToArray();

            return new JsonSchemaProperty
            {
                AnyOf = enumValues,
            };
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

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            var c = (JsonSchema)obj;
            return (this.Title == c.Title);
        }

        public override int GetHashCode()
        {
            return Title.GetHashCode();
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

            if (t.IsEnum)
            {
                // anyof
                return "";
            }

            if (t.IsClass)
            {
                return "object";
            }

            throw new NotImplementedException(t.Name);
        }

        static KeyValuePair<string, JsonSchemaProperty> CreateProperty(string key, Type type, JsonSchemaPropertyAttributeAttribute a)
        {
            if (type.IsEnum)
            {
                return new KeyValuePair<string, JsonSchemaProperty>(key, JsonSchemaProperty.FromEnum(type));
            }
            else
            {
                var jsonType = GetJsonType(type);
                return new KeyValuePair<string, JsonSchemaProperty>(key, new JsonSchemaProperty(jsonType, a));
            }
        }

        static IEnumerable<KeyValuePair<string, JsonSchemaProperty>> GetProperties(Type t, PropertyExportFlags exportFlags)
        {
            foreach (var fi in t.GetFields())
            {
                var a = fi.GetCustomAttributes(typeof(JsonSchemaPropertyAttributeAttribute), true).FirstOrDefault() as JsonSchemaPropertyAttributeAttribute;
                yield return CreateProperty(fi.Name, fi.FieldType, a);
            }

            foreach (var pi in t.GetProperties())
            {
                var a = pi.GetCustomAttributes(typeof(JsonSchemaPropertyAttributeAttribute), true).FirstOrDefault() as JsonSchemaPropertyAttributeAttribute;
                yield return CreateProperty(pi.Name, pi.PropertyType, a);
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
            var values = JsonParser.Parse(json);

            var root = new JsonNode(values.ToArray(), 0);

            if (root.GetValueType() != JsonValueType.Object)
            {
                throw new JsonParseException("root value must object: " + root.Value.ToString());
            }

            return new JsonSchema
            {
                Title = root["title"].GetString(),
            };
        }
    }
}
