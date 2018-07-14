using System;
using System.Collections;
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
                var _a = fi.GetCustomAttributes(typeof(JsonSchemaAttribute), true).FirstOrDefault();
                JsonSchemaAttribute a = null;
                if (_a != null)
                {
                    a = _a as JsonSchemaAttribute;
                }

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

        public static JsonSchemaValidatorBase Create(JsonValueType valueType, Type t = null, JsonSchemaAttribute a = null)
        {
            switch (valueType)
            {
                case JsonValueType.Integer:
                    {
                        var v = new JsonIntValidator();
                        if (a != null)
                        {
                            if (a.Minimum != double.PositiveInfinity)
                            {
                                v.Minimum = (int)a.Minimum;
                            }
                        }
                        return v;
                    }

                case JsonValueType.Number:
                    {
                        var v = new JsonNumberValidator();
                        if (a != null)
                        {
                            if (a.Minimum != double.PositiveInfinity)
                            {
                                v.Minimum = a.Minimum;
                            }
                        }
                        return v;
                    }

                case JsonValueType.String:
                    return new JsonStringValidator();

                case JsonValueType.Boolean:
                    return new JsonBoolValidator();

                case JsonValueType.Array:
                    {
                        var v= new JsonArrayValidator();
                        if (a != null)
                        {
                            if (a.MinItems != 0)
                            {
                                v.MinItems = a.MinItems;
                            }
                        }
                        return v;
                    }

                case JsonValueType.Object:
                    {
                        var v = new JsonObjectValidator();
                        if (a != null)
                        {
                            // props
                            foreach (var prop in GetProperties(t, a.ExportFlags))
                            {
                                v.Properties.Add(prop.Key, prop.Schema);
                                if (prop.Required)
                                {
                                    v.Required.Add(prop.Key);
                                }
                            }
                        }
                        return v;
                    }

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

            if (t.IsArray)
            {
                return JsonValueType.Array;
            }
            if(t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(List<>)))
            {
                return JsonValueType.Array;
            }

            if (t.IsClass)
            {
                return JsonValueType.Object;
            }

            throw new NotImplementedException();
        }

        public static JsonSchemaValidatorBase Create(Type t, JsonSchemaAttribute a)
        {
            return Create(ToJsonType(t), t, a);
        }
    }
}
