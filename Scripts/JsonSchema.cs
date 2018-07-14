using System;
using System.Collections.Generic;
using System.Linq;


namespace UniJSON
{
    public class JsonSchema
    {
        public string Schema; // http://json-schema.org/draft-04/schema

        #region Annotations
        public string Title { get; private set; }
        public string Description { get; private set; }
        #endregion

        public JsonSchemaValidatorBase Validator { get; private set; }

        public override string ToString()
        {
            return string.Format("<{0}>", Title);
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonSchema;
            if (rhs == null) return false;
            return Validator.Equals(rhs.Validator);
        }

        public override int GetHashCode()
        {
            return 1;
        }

        #region FromType
        public static JsonSchema FromType<T>()
        {
            return FromType(typeof(T), null);
        }

        public static JsonSchema FromType(Type t, JsonSchemaAttribute a)
        {
            if (a == null)
            {
                a = t.GetCustomAttributes(typeof(JsonSchemaAttribute), true)
                    .FirstOrDefault() as JsonSchemaAttribute;
            }
            if (a == null)
            {
                a = new JsonSchemaAttribute
                {
                    Title = t.Name,
                };
            }

            JsonSchemaValidatorBase validator = null;
            if (t.IsEnum)
            {
                switch (a.EnumSerializationType)
                {
                    case EnumSerializationType.AsInt:
                        validator = new JsonIntValidator();
                        break;

                    case EnumSerializationType.AsString:
                        validator = new JsonStringValidator();
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                validator = JsonSchemaValidatorFactory.Create(t, a);
            }
            validator.Required = a.Required;

            var schema = new JsonSchema
            {
                Title = a.Title,
                Description = a.Description,          
                Validator = validator,
            };

            return schema;
        }
        #endregion

        #region FromJson
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

        public void Parse(IFileSystemAccessor fs, JsonNode root, string Key)
        {
            m_context.Push(Key);

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
                        Validator = JsonSchemaValidatorFactory.Create(kv.Value.GetString());
                        break;

                    case "properties": // for object
                        m_context.Push("properties");
                        {
                            var objectValidator = Validator as JsonObjectValidator;
                            foreach (var prop in kv.Value.ObjectItems)
                            {
                                objectValidator.AddProperty(fs, prop.Key, prop.Value);
                            }
                        }
                        m_context.Pop();
                        break;

                    case "required": // for object
                        {
                            var objectValidator = Validator as JsonObjectValidator;
                            foreach (var req in kv.Value.ArrayItems)
                            {
                                objectValidator.SetRequired(req.GetString());
                            }
                        }
                        break;

                    case "minimum": // for number
                        break;

                    case "maximum":
                        break;

                    case "multipleOf":
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

                    case "uniqueItems":
                        break;

                    case "pattern":
                        break;

                    case "format":
                        break;

                    case "dependencies":
                        break;

                    case "minProperties":
                        break;

                    case "oneOf":
                        break;

                    case "not":
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

                    case "gltf_uriType":
                        break;

                    default:
                        throw new NotImplementedException(string.Format("unknown key: {0}", kv.Key));
                }

            }
            m_context.Pop();

            if (composition.Count > 0)
            {
                Composite(compositionType, composition);
            }
        }

        void Composite(CompositionType compositionType, List<JsonSchema> composition)
        {
            switch (compositionType)
            {
                case CompositionType.AllOf:
                    if (composition.Count == 1)
                    {
                        // inheritance
                        if (Validator == null)
                        {
                            Validator = JsonSchemaValidatorFactory.Create(composition[0].Validator.JsonValueType);
                        }
                        Validator.Assign(composition[0].Validator);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;

                case CompositionType.AnyOf:
                    // extend enum
                    if (Validator == null)
                    {
                        var typeSchema = composition.First(x => x.Validator != null);
                        Validator = JsonSchemaValidatorFactory.Create(typeSchema.Validator.JsonValueType);
                    }
                    //throw new NotImplementedException();
                    break;

                default:
                    throw new NotImplementedException();
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
        #endregion
    }
}
