using System;
using System.Collections.Generic;
using System.IO;
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

        public static JsonSchemaPropertyItem Create(JsonNode node)
        {
            return new JsonSchemaPropertyItem
            {

            };
        }
    }

    public class JsonSchemaProperty
    {
        public string Description { get; private set; }
        public object Required { get; private set; } // boolean or string[]

        public string Type { get; private set; }
        public object Minimum { get; private set; } // int, float, int[], float[]
        public JsonSchemaPropertyItem[] AnyOf { get; private set; }
        public string[] AllOf { get; private set; }

        public JsonSchemaProperty(string type = null, JsonSchemaPropertyAttribute a = null)
        {
            Type = type;
            ApplyAttribute(a);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            var c = (JsonSchemaProperty)obj;

            /*
            if (this.Required != c.Required)
                return false;
            */
            if (this.Type != c.Type)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(JsonSchemaProperty lhs, JsonSchemaProperty rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(JsonSchemaProperty lhs, JsonSchemaProperty rhs)
        {
            return !lhs.Equals(rhs);
        }

        private void ApplyAttribute(JsonSchemaPropertyAttribute a)
        {
            if (a != null)
            {
                Description = a.Description;
                Minimum = a.Minimum;
                Required = a.Required;
            }
        }

        public static JsonSchemaProperty FromEnum(Type enumType, JsonSchemaPropertyAttribute a = null)
        {
            if (a.EnumSerializationType == EnumSerializationType.AsInt)
            {
                var enumValues = Enum.GetValues(enumType).Cast<Object>().Select(x => new JsonSchemaPropertyItem
                {
                    Enum = new[] { (object)(int)x },
                    Description = x.ToString(),
                }).ToArray();

                var prop = new JsonSchemaProperty
                {
                    AnyOf = enumValues,
                };
                prop.ApplyAttribute(a);
                return prop;
            }
            else if (a.EnumSerializationType == EnumSerializationType.AsString)
            {
                var enumValues = Enum.GetValues(enumType).Cast<Object>().Select(x => new JsonSchemaPropertyItem
                {
                    Enum = new[] { x.ToString() },
                    Description = x.ToString(),
                }).ToArray();

                var prop = new JsonSchemaProperty
                {
                    AnyOf = enumValues,
                };
                prop.ApplyAttribute(a);
                return prop;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static JsonSchemaProperty FromJsonNode(JsonNode node)
        {
            //var tmp = node.ObjectItems.ToArray();

            var required = default(object);
            if (node.ContainsKey("required"))
            {
                var req = node["required"];
                var reqType = req.Value.ValueType;
                if (reqType == JsonValueType.Boolean)
                {
                    required = req.GetBoolean();
                }
                else if (reqType == JsonValueType.Array)
                {
                    required = req.ArrayItems.Select(x => x.GetString()).ToArray();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            if (node.ContainsKey("allOf"))
            {
                return new JsonSchemaProperty
                {
                    AllOf = node["allOf"].ArrayItems.Select(x => x.GetString()).ToArray(),
                    Required = required,
                };
            }
            else if (node.ContainsKey("anyOf"))
            {
                return new JsonSchemaProperty
                {
                    AnyOf = node["anyOf"].ArrayItems.Select(x => JsonSchemaPropertyItem.Create(x)).ToArray(),
                    Required = required,
                };
            }
            else if (node.ContainsKey("type"))
            {
                return new JsonSchemaProperty
                {
                    Type = node["type"].GetString(),
                    Required = required,
                };
            }
            else
            {
                return new JsonSchemaProperty
                {
                    Required = required,
                };
            }
        }
    }

    public class JsonSchema
    {
        public string Title { get; private set; }
        public string Type { get; private set; }
        public Dictionary<string, JsonSchemaProperty> Properties { get; private set; }
        public string[] Required { get; private set; }

        public override string ToString()
        {
            return string.Format("<{0}>", Title);
        }

        public static JsonSchema Create<T>()
        {
            return Create(typeof(T));
        }

        public bool MatchProperties(JsonSchema rhs)
        {
            if (this.Properties.Count != rhs.Properties.Count)
                return false;

            foreach (var pair in Properties)
            {
                JsonSchemaProperty value;
                if (rhs.Properties.TryGetValue(pair.Key, out value))
                {
#if false
                    // ToDo
                    if (value.Type != pair.Value.Type)
                    {
                        return false;
                    }
#else
                    // key name match
                    return true;
#endif
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            var c = (JsonSchema)obj;

            if (this.Title != c.Title)
                return false;
            if (this.Type != c.Type)
                return false;
            if (this.Properties.Count != c.Properties.Count)
                return false;
            foreach (var pair in Properties)
            {
                JsonSchemaProperty value;
                if (c.Properties.TryGetValue(pair.Key, out value))
                {
                    // Require value be equal.
                    if (value != pair.Value)
                    {
                        return false;
                    }
                }
                else
                {
                    // Require key be present.
                    return false;
                }
            }
            if (!this.Required.OrderBy(x => x).SequenceEqual(c.Required.OrderBy(x => x)))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return Title.GetHashCode();
        }

        static readonly Dictionary<Type, string> JsonSchemaTypeMap = new Dictionary<Type, string>
        {
            {typeof(bool), "boolean" },
            {typeof(string), "string"},
            {typeof(int), "integer"},
            {typeof(float), "floatg" }
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

        static KeyValuePair<string, JsonSchemaProperty> CreateProperty(string key, Type type, JsonSchemaPropertyAttribute a)
        {
            if (type.IsEnum)
            {
                return new KeyValuePair<string, JsonSchemaProperty>(key, JsonSchemaProperty.FromEnum(type, a));
            }
            else
            {
                var jsonType = GetJsonType(type);
                if (!string.IsNullOrEmpty(jsonType))
                {
                    return new KeyValuePair<string, JsonSchemaProperty>(key, new JsonSchemaProperty(jsonType, a));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        static IEnumerable<KeyValuePair<string, JsonSchemaProperty>> GetProperties(Type t, PropertyExportFlags exportFlags)
        {
            foreach (var fi in t.GetFields())
            {
                var a = fi.GetCustomAttributes(typeof(JsonSchemaPropertyAttribute), true).FirstOrDefault() as JsonSchemaPropertyAttribute;
                if (a != null)
                {
                    yield return CreateProperty(fi.Name, fi.FieldType, a);
                }
                else
                {
                    // default
                    // only public instance field
                    if (!fi.IsStatic && fi.IsPublic)
                    {
                        yield return CreateProperty(fi.Name, fi.FieldType, new JsonSchemaPropertyAttribute());
                    }
                }
            }

            foreach (var pi in t.GetProperties())
            {
                var a = pi.GetCustomAttributes(typeof(JsonSchemaPropertyAttribute), true).FirstOrDefault() as JsonSchemaPropertyAttribute;
                if (a != null)
                {
                    yield return CreateProperty(pi.Name, pi.PropertyType, a);
                }
                else
                {
                    // default
                    // skip
                }
            }
        }

        public static JsonSchema Create(Type t, PropertyExportFlags exportFlags = PropertyExportFlags.Default)
        {
            var props = GetProperties(t, exportFlags).ToArray();

            var a = (JsonSchemaObjectAttribute)t.GetCustomAttributes(typeof(JsonSchemaObjectAttribute), true).FirstOrDefault();

            return new JsonSchema
            {
                Title = a != null ? a.Title : t.Name,
                Type = GetJsonType(t),
                Properties = props.ToDictionary(x => x.Key, x => x.Value),
                Required = props.Where(x => x.Value.Required != null && (bool)x.Value.Required != false).Select(x => x.Key).ToArray(),
            };
        }

        public static JsonSchema ParseFromPath(string path)
        {
            var baseDir = Path.GetDirectoryName(path);
            var json = File.ReadAllText(path, Encoding.UTF8);
            var root = JsonParser.Parse(json);
            if (root.Value.ValueType != JsonValueType.Object)
            {
                throw new JsonParseException("root value must object: " + root.Value.ToString());
            }

            // extend $ref
            // "allOf": [ { "$ref": "glTFid.schema.json" } ]
            //
            /*
            {
                "$schema": "http://json-schema.org/draft-04/schema",
                "title": "glTF Id",
                "type": "integer",
                "minimum": 0
            }
             */
            while (true)
            {
                var replaced = false;
                foreach (var kv in root.TraverseObjects())
                {
                    if (kv.Key == "allOf")
                    {
                        if (kv.Value.Value.ValueType == JsonValueType.Array)
                        {
                            var parent = kv.Value.Parent;
                            //var parentIndex = kv.Value.Value.ParentIndex;

                            var refObj = kv.Value[0];
                            if (refObj.Value.ValueType == JsonValueType.Object && refObj.ContainsKey("$ref"))
                            {
                                // replace $ref
                                var refPath = Path.Combine(baseDir, refObj["$ref"].GetString());
                                var refJson = File.ReadAllText(refPath, Encoding.UTF8);
                                var refRoot = JsonParser.Parse(refJson);

                                // remove allOf
                                parent.RemoveKey("allOf");

                                // add Values
                                foreach (var _kv in refRoot.ObjectItems)
                                {
                                    parent.AddNode(_kv.Key, _kv.Value);
                                }

                                replaced = true;
                                break;
                            }
                        }
                    }
                }

                if (!replaced)
                {
                    break;
                }
            }

            var schema = new JsonSchema
            {
                Title = root["title"].GetString(),
                Type = "object",
                Properties = root["properties"].ObjectItems.ToDictionary(x => x.Key, x => JsonSchemaProperty.FromJsonNode(x.Value)),
            };

            var required = root.ObjectItems.FirstOrDefault(x => x.Key == "Required").Value;
            if (required.Values != null)
            {
                schema.Required = required.ArrayItems.Select(x => x.GetString()).ToArray();
            }

            return schema;
        }
    }
}
