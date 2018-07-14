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

    public enum CompositionType
    {
        Unknown,

        AllOf,
        AnyOf,
    }

    public class JsonSchema
    {
        public string Schema; // http://json-schema.org/draft-04/schema

        #region Annotations
        public string Title { get; private set; }
        public string Description { get; private set; }
        #endregion

        #region Validations
        public JsonValueType Type { get; private set; }
        public bool Required { get; private set; }
        public IEnumValues EnumValues { get; private set; }
        Dictionary<string, JsonSchema> m_props;
        public Dictionary<string, JsonSchema> Properties
        {
            get
            {
                if (m_props == null)
                {
                    m_props = new Dictionary<string, JsonSchema>();
                }
                return m_props;
            }
        } // for object
        #endregion

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
                JsonSchema value;
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

            if (this.Type != c.Type)
                return false;
            if (this.Properties.Count != c.Properties.Count)
                return false;
            foreach (var pair in Properties)
            {
                JsonSchema value;
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

            return true;
        }

        public override int GetHashCode()
        {
            return Title.GetHashCode();
        }

        static readonly Dictionary<Type, JsonValueType> JsonSchemaTypeMap = new Dictionary<Type, JsonValueType>
        {
            {typeof(bool), JsonValueType.Boolean },
            {typeof(string), JsonValueType.String },
            {typeof(int), JsonValueType.Integer},
            {typeof(float), JsonValueType.Number }
        };

        static JsonValueType GetJsonType(Type t)
        {
            JsonValueType jsonValueType;
            if (JsonSchemaTypeMap.TryGetValue(t, out jsonValueType))
            {
                return jsonValueType;
            }

            if (t.IsClass)
            {
                return JsonValueType.Object;
            }

            throw new NotImplementedException(t.Name);
        }

        public interface IEnumValues
        {
            Object[] Values { get; }
        }

        public class IntEnum<T> : IEnumValues
        {
            public object[] Values
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public class StringEnum<T> : IEnumValues
        {
            public object[] Values
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        static JsonSchema FromType(Type type, JsonSchemaPropertyAttribute a)
        {
            var jsonType = default(JsonValueType);
            if (type.IsEnum)
            {
                switch (a.EnumSerializationType)
                {
                    case EnumSerializationType.AsInt:
                        jsonType = JsonValueType.Integer;
                        break;

                    case EnumSerializationType.AsString:
                        jsonType = JsonValueType.String;
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                jsonType = GetJsonType(type);
                if (jsonType == JsonValueType.Unknown)
                {
                    throw new NotImplementedException();
                }
            }

            return new JsonSchema
            {
                Description = a.Description,
                Type = jsonType,
                Required = a.Required,
            };
        }

        static IEnumerable<KeyValuePair<string, JsonSchema>> GetProperties(Type t, PropertyExportFlags exportFlags)
        {
            // fields
            foreach (var fi in t.GetFields())
            {
                var a = fi.GetCustomAttributes(typeof(JsonSchemaPropertyAttribute), true).FirstOrDefault() as JsonSchemaPropertyAttribute;
                if (a == null)
                {
                    // default
                    // only public instance field
                    if (!fi.IsStatic && fi.IsPublic)
                    {
                        a = new JsonSchemaPropertyAttribute();
                    }
                }

                if (a != null)
                {
                    yield return new KeyValuePair<string, JsonSchema>(fi.Name, JsonSchema.FromType(fi.FieldType, a));
                }
            }

            // properties
            foreach (var pi in t.GetProperties())
            {
                var a = pi.GetCustomAttributes(typeof(JsonSchemaPropertyAttribute), true).FirstOrDefault() as JsonSchemaPropertyAttribute;

                if (a != null)
                {
                    yield return new KeyValuePair<string, JsonSchema>(pi.Name, JsonSchema.FromType(pi.PropertyType, a));
                }
            }
        }

        public static JsonSchema Create(Type t, PropertyExportFlags exportFlags = PropertyExportFlags.Default)
        {
            var schema = new JsonSchema
            {
                Title = t.Name,
                Type = GetJsonType(t),
            };

            var a = (JsonSchemaObjectAttribute)t.GetCustomAttributes(typeof(JsonSchemaObjectAttribute), true).FirstOrDefault();
            if (a != null)
            {
                schema.Title = a.Title;
            }

            foreach (var x in GetProperties(t, exportFlags))
            {
                schema.Properties.Add(x.Key, x.Value);
            }

            return schema;
        }

        void Assign(JsonSchema rhs)
        {
            this.Type = rhs.Type;

            if (rhs.Required)
            {
                this.Required = rhs.Required;
            }

            foreach (var x in rhs.Properties)
            {
                if (this.Properties.ContainsKey(x.Key))
                {
                    this.Properties[x.Key] = x.Value;
                }
                else
                {
                    this.Properties.Add(x.Key, x.Value);
                }
            }
        }

        void Composite(CompositionType compositionType, List<JsonSchema> composition)
        {
            switch (compositionType)
            {
                case CompositionType.AllOf:
                    if (composition.Count == 1)
                    {
                        this.Assign(composition[0]);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;

                case CompositionType.AnyOf:
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        static JsonValueType ParseValueType(string type)
        {
            try
            {
                return (JsonValueType)Enum.Parse(typeof(JsonValueType), type, true);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException(string.Format("unknown type: {0}", type));
            }
        }

        Stack<string> m_context = new Stack<string>();

        void Parse(IFileSystemAccessor fs, JsonNode root, string Key)
        {
            m_context.Push(Key);

            var required = new List<string>();
            var compositionType = default(CompositionType);
            var composition = new List<JsonSchema>();
            foreach (var kv in root.ObjectItems)
            {
                //Console.WriteLine(kv.Key);
                switch (kv.Key)
                {
                    case "$schema":
                        Schema = kv.Value.GetString();
                        break;

                    case "title": // annotation
                        Title = kv.Value.GetString();
                        break;

                    case "description": // annotation
                        Description = kv.Value.GetString();
                        break;

                    case "type": // validation
                        Type = ParseValueType(kv.Value.GetString());
                        break;

                    case "properties": // validation
                        m_context.Push("properties");
                        foreach (var prop in kv.Value.ObjectItems)
                        {
                            var sub = new JsonSchema();
                            sub.Parse(fs, prop.Value, prop.Key);
                            Properties.Add(prop.Key, sub);
                        }
                        m_context.Pop();
                        break;

                    case "required": // validation
                        foreach (var req in kv.Value.ArrayItems)
                        {
                            required.Add(req.GetString());
                        }
                        break;

                    case "minimum": // for number
                        break;

                    case "default":
                        break;

                    case "enum": // value constraint
                        break;

                    case "items": // for array ?
                        break;

                    case "minItems":
                        break;

                    case "maxItems":
                        break;

                    case "dependencies":
                        break;

                    case "anyOf": // composition
                    case "allOf": // composition
                        {
                            compositionType = (CompositionType)Enum.Parse(typeof(CompositionType), kv.Key, true);
                            foreach (var item in kv.Value.ArrayItems)
                            {
                                if (item.ContainsKey("$ref"))
                                {
                                    var sub = JsonSchema.ParseFromPath(fs.Get(item["$ref"].GetString()));
                                    composition.Add(sub);
                                }
                                else
                                {
                                    var sub = new JsonSchema();
                                    sub.Parse(fs, item, compositionType.ToString());
                                    composition.Add(sub);
                                }
                            }
                        }
                        break;

                    case "$ref":
                        {
                            var refFs = fs.Get(kv.Value.GetString());

                            // parse JSON
                            var json = refFs.ReadAllText();
                            var refRoot = JsonParser.Parse(json);

                            Parse(refFs, refRoot, "$ref");
                        }
                        break;

                    case "additionalProperties":
                        break;

                    case "gltf_detailedDescription":
                        break;

                    case "gltf_webgl":
                        break;

                    default:
                        throw new NotImplementedException(string.Format("unknown key: {0}", kv.Key));
                }

            }
            m_context.Pop();

            foreach (var req in required)
            {
                Properties[req].Required = true;
            }

            if (composition.Count > 0)
            {
                Composite(compositionType, composition);
            }
        }

        public static JsonSchema ParseFromPath(IFileSystemAccessor fs)
        {
            // parse JSON
            var json = fs.ReadAllText();
            var root = JsonParser.Parse(json);

            // create schema
            var schema = new JsonSchema();
            schema.Parse(fs, root, "__ParseFromPath__" + fs.ToString());
            return schema;
        }
    }
}
