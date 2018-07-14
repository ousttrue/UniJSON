using System;
using System.Linq;
using System.Collections.Generic;


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

    public abstract class JsonSchemaValidatorBase
    {
        public abstract JsonValueType JsonValueType { get; }
        public bool Required { get; set; }

        public virtual void Assign(JsonSchemaValidatorBase rhs)
        {
            if (JsonValueType != rhs.JsonValueType)
            {
                throw new NotImplementedException();
            }

            if (rhs.Required)
            {
                this.Required = rhs.Required;
            }
        }
    }

    public class JsonBoolValidator: JsonSchemaValidatorBase
    {
        public override JsonValueType JsonValueType { get { return JsonValueType.Boolean; } }
    }

    public class JsonIntValidator : JsonSchemaValidatorBase
    {
        public override JsonValueType JsonValueType { get { return JsonValueType.Integer; } }
    }

    public class JsonStringValidator : JsonSchemaValidatorBase
    {
        public override JsonValueType JsonValueType { get { return JsonValueType.String; } }
    }

    public class JsonArrayValidator: JsonSchemaValidatorBase
    {
        public override JsonValueType JsonValueType { get { return JsonValueType.Array; } }
    }

    public class JsonObjectValidator : JsonSchemaValidatorBase
    {
        public override JsonValueType JsonValueType { get { return JsonValueType.Object; } }

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
        }

        public override int GetHashCode()
        {
            return 1;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonObjectValidator;
            if (rhs == null)
            {
                return false;
            }

            if (Properties.Count != rhs.Properties.Count)
            {
                return false;
            }

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

        public override void Assign(JsonSchemaValidatorBase obj)
        {
            base.Assign(obj);

            var rhs = obj as JsonObjectValidator;
            if (rhs == null)
            {
                throw new ArgumentException();
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

        public void AddProperty(IFileSystemAccessor fs, string key, JsonNode value)
        {
            var sub = new JsonSchema();
            sub.Parse(fs, value, key);
            Properties.Add(key, sub);
        }

        public void SetRequired(string key)
        {
            var validator = Properties[key].Validator;
            validator.Required = true;
        }
    }

    public static class JsonSchemaValidatorFactory
    {
        static IEnumerable<KeyValuePair<string, JsonSchema>> GetProperties(Type t, PropertyExportFlags exportFlags)
        {
            // fields
            foreach (var fi in t.GetFields())
            {
                var a = fi.GetCustomAttributes(typeof(JsonSchemaAttribute), true).FirstOrDefault() as JsonSchemaAttribute;
                if (a == null)
                {
                    // default
                    // only public instance field
                    if (!fi.IsStatic && fi.IsPublic)
                    {
                        a = new JsonSchemaAttribute();
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
                var a = pi.GetCustomAttributes(typeof(JsonSchemaAttribute), true).FirstOrDefault() as JsonSchemaAttribute;

                if (a != null)
                {
                    yield return new KeyValuePair<string, JsonSchema>(pi.Name, JsonSchema.FromType(pi.PropertyType, a));
                }
            }
        }

        public static JsonSchemaValidatorBase Create(JsonValueType t)
        {
            switch (t)
            {
                case JsonValueType.Integer: return new JsonIntValidator();
                case JsonValueType.String: return new JsonStringValidator();
                case JsonValueType.Boolean: return new JsonBoolValidator();
                case JsonValueType.Array: return new JsonArrayValidator();
                case JsonValueType.Object: return new JsonObjectValidator();
                default:
                    throw new NotImplementedException();
            }
        }

        public static JsonSchemaValidatorBase Create(string t)
        {
            return Create((JsonValueType)Enum.Parse(typeof(JsonValueType), t, true));
        }

        static Dictionary<Type, JsonValueType> s_typeMap = new Dictionary<Type, JsonValueType>
        {
            {typeof(int), JsonValueType.Integer },
            {typeof(float), JsonValueType.Number },
            {typeof(string), JsonValueType.String },
            {typeof(bool), JsonValueType.Boolean },
        };

        static JsonValueType ToJsonType(Type t)
        {
            if (t.IsClass)
            {
                return JsonValueType.Object;
            }

            JsonValueType jsonValueType;
            if(s_typeMap.TryGetValue(t, out jsonValueType))
            {
                return jsonValueType;
            }

            throw new NotImplementedException();
        }

        public static JsonSchemaValidatorBase Create(Type t, JsonSchemaAttribute a)
        {
            var validator =  Create(ToJsonType(t));
            var obj = validator as JsonObjectValidator;
            if (obj != null)
            {
                // props
                foreach(var prop in GetProperties(t, a.ExportFlags))
                {
                    obj.Properties.Add(prop.Key, prop.Value);
                }
            }
            return validator;
        }
    }
}
