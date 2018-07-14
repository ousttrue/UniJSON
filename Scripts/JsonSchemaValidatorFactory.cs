using System;
using System.Collections.Generic;
using System.Linq;


namespace UniJSON
{
    public static class JsonSchemaValidatorFactory
    {
        struct JsonSchemaItem
        {
            public string Key;
            public JsonSchema Schema;
            public bool Required;
        }

        static IEnumerable<JsonSchemaItem> GetProperties(Type t, PropertyExportFlags exportFlags)
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
                    yield return new JsonSchemaItem
                    {
                        Key = fi.Name,
                        Schema = JsonSchema.FromType(fi.FieldType, a),
                        Required = a.Required,
                    };
                }
            }

            // properties
            foreach (var pi in t.GetProperties())
            {
                var a = pi.GetCustomAttributes(typeof(JsonSchemaAttribute), true).FirstOrDefault() as JsonSchemaAttribute;

                if (a != null)
                {
                    yield return new JsonSchemaItem
                    {
                        Key = pi.Name,
                        Schema = JsonSchema.FromType(pi.PropertyType, a),
                        Required = a.Required,
                    };
                }
            }
        }

        public static JsonSchemaValidatorBase Create(JsonValueType t)
        {
            switch (t)
            {
                case JsonValueType.Integer: return new JsonIntValidator();
                case JsonValueType.Number: return new JsonNumberValidator();
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
            JsonValueType jsonValueType;
            if (s_typeMap.TryGetValue(t, out jsonValueType))
            {
                return jsonValueType;
            }

            if (t.IsClass)
            {
                return JsonValueType.Object;
            }

            throw new NotImplementedException();
        }

        public static JsonSchemaValidatorBase Create(Type t, JsonSchemaAttribute a)
        {
            var validator = Create(ToJsonType(t));
            var obj = validator as JsonObjectValidator;
            if (obj != null)
            {
                // props
                foreach (var prop in GetProperties(t, a.ExportFlags))
                {
                    obj.Properties.Add(prop.Key, prop.Schema);
                    if (prop.Required)
                    {
                        obj.Required.Add(prop.Key);
                    }
                }
            }
            return validator;
        }
    }
}
