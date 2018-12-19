using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace UniJSON
{
    /// <summary>
    /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5
    /// </summary>
    public class JsonObjectValidator : IJsonSchemaValidator
    {
        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5.1
        /// </summary>
        public int MaxProperties
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5.2
        /// </summary>
        public int MinProperties
        {
            get; set;
        }

        List<string> m_required = new List<string>();
        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5.3
        /// </summary>
        public List<string> Required
        {
            get { return m_required; }
        }

        Dictionary<string, JsonSchema> m_props;
        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5.4
        /// </summary>
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

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5.5
        /// </summary>
        public string PatternProperties
        {
            get; private set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5.6
        /// </summary>
        public JsonSchema AdditionalProperties
        {
            get; set;
        }

        Dictionary<string, string[]> m_depndencies;
        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5.7
        /// </summary>
        public Dictionary<string, string[]> Dependencies
        {
            get
            {
                if (m_depndencies == null)
                {
                    m_depndencies = new Dictionary<string, string[]>();
                }
                return m_depndencies;
            }
        }

        public void AddProperty(IFileSystemAccessor fs, string key, JsonNode value)
        {
            var sub = new JsonSchema();
            sub.Parse(fs, value, key);

            if (Properties.ContainsKey(key))
            {
                if (sub.Validator != null)
                {
                    Properties[key].Validator.Merge(sub.Validator);
                }
            }
            else
            {
                Properties.Add(key, sub);
            }
        }

        public override int GetHashCode()
        {
            return 6;
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
#if true
                    if (!value.Equals(pair.Value))
                    {
                        Console.WriteLine(string.Format("{0} is not equals", pair.Key));
                        var l = pair.Value.Validator;
                        var r = value.Validator;
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

            if (Required.Count != rhs.Required.Count)
            {
                return false;
            }
            if (!Required.OrderBy(x => x).SequenceEqual(rhs.Required.OrderBy(x => x)))
            {
                return false;
            }

            if (Dependencies.Count != rhs.Dependencies.Count)
            {
                return false;
            }
            foreach (var kv in Dependencies)
            {
                if (!kv.Value.OrderBy(x => x).SequenceEqual(rhs.Dependencies[kv.Key].OrderBy(x => x)))
                {
                    return false;
                }
            }

            if (AdditionalProperties == null
                && rhs.AdditionalProperties == null)
            {
                // ok
            }
            else if (AdditionalProperties == null)
            {
                return false;
            }
            else if (rhs.AdditionalProperties == null)
            {
                return false;
            }
            else
            {
                if (!AdditionalProperties.Equals(rhs.AdditionalProperties))
                {
                    return false;
                }
            }

            return true;
        }

        public void Merge(IJsonSchemaValidator obj)
        {
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

            foreach (var x in rhs.Required)
            {
                this.Required.Add(x);
            }

            if (rhs.AdditionalProperties != null)
            {
                if (AdditionalProperties != null)
                {
                    throw new NotImplementedException();
                }
                AdditionalProperties = rhs.AdditionalProperties;
            }
        }

        public bool FromJsonSchema(IFileSystemAccessor fs, string key, JsonNode value)
        {
            switch (key)
            {
                case "maxProperties":
                    MaxProperties = value.GetInt32();
                    return true;

                case "minProperties":
                    MinProperties = value.GetInt32();
                    return true;

                case "required":
                    {
                        foreach (var req in value.ArrayItemsRaw)
                        {
                            m_required.Add(req.GetString());
                        }
                    }
                    return true;

                case "properties":
                    {
                        foreach (var prop in value.ObjectItemsRaw)
                        {
                            AddProperty(fs, prop.Key.ToString(), prop.Value);
                        }
                    }
                    return true;

                case "patternProperties":
                    PatternProperties = value.GetString();
                    return true;

                case "additionalProperties":
                    {
                        var sub = new JsonSchema();
                        sub.Parse(fs, value, "additionalProperties");
                        AdditionalProperties = sub;
                    }
                    return true;

                case "dependencies":
                    {
                        foreach (var kv in value.ObjectItemsRaw)
                        {
                            Dependencies.Add(kv.Key.ToString(), kv.Value.ArrayItemsRaw.Select(x => x.GetString()).ToArray());
                        }
                    }
                    return true;

                case "propertyNames":
                    return true;
            }

            return false;
        }

        public void ToJsonScheama(IFormatter f)
        {
            f.Key("type"); f.Value("object");
            if (Properties.Count > 0)
            {
                f.Key("properties");
                f.BeginMap(Properties.Count);
                foreach (var kv in Properties)
                {
                    f.Key(kv.Key);
                    kv.Value.ToJson(f);
                }
                f.EndMap();
            }
        }

        public JsonSchemaValidationException Validate<T>(JsonSchemaValidationContext c, T o)
        {
            if (o == null)
            {
                return new JsonSchemaValidationException(c, "null");
            }

            if (Properties.Count < MinProperties)
            {
                return new JsonSchemaValidationException(c, "no properties");
            }

            if (Required != null)
            {
                foreach (var x in Required)
                {
                    using (c.Push(x))
                    {
                        var value = o.GetValueByKey(x);
                        var ex = Properties[x].Validator.Validate(c, value);
                        if (ex != null)
                        {
                            return ex;
                        }
                    }
                }
            }

            return null;
        }

        static class GenericSerializer<T>
        {
            class Serializer
            {
                delegate void FieldSerializer(JsonObjectValidator v, IFormatter f, JsonSchemaValidationContext c, T src);

                List<FieldSerializer> m_fieldSerializers = new List<FieldSerializer>();

                static FieldSerializer CreateSerializer<U>(FieldInfo fi)
                {
                    var src = Expression.Parameter(typeof(T), "src");
                    var getter = Expression.Field(src, fi);
                    var compiled = (Func<T, U>)Expression.Lambda(getter, src).Compile();
                    var name = fi.Name;
                    return (v, f, c, s) =>
                    {
                        c.Push(name);

                        var validator = v.Properties[name].Validator;

                        var value = compiled(s);

                        // validate
                        if (validator.Validate(c, value) != null)
                        {
                            return;
                        }

                        /*
                        // depencencies
                        string[] dependencies;
                        if (validator.Dependencies.TryGetValue(name, out dependencies))
                        {
                            // check dependencies
                            bool hasDependencies = true;
                            foreach (var x in dependencies)
                            {
                                if (!map.ContainsKey(x))
                                {
                                    hasDependencies = false;
                                    break;
                                }
                            }
                            if (!hasDependencies)
                            {
                                continue;
                            }
                        }
                        */

                        f.Key(name);
                        validator.Serialize(f, c, value);
                        //f.Serialize(value);

                        c.Pop();
                    };
                }

                public void AddField(FieldInfo fi)
                {
                    var mi = typeof(Serializer).GetMethod("CreateSerializer",
                        BindingFlags.Static | BindingFlags.NonPublic);
                    var g = mi.MakeGenericMethod(fi.FieldType);
                    var f = Expression.Constant(fi);
                    var call = Expression.Call(g, f);
                    var compiled = (Func<FieldSerializer>)Expression.Lambda(call).Compile();
                    m_fieldSerializers.Add(compiled());
                }

                public void Serialize(JsonObjectValidator v, IFormatter f, JsonSchemaValidationContext c, T value)
                {
                    f.BeginMap(m_fieldSerializers.Count);
                    foreach (var s in m_fieldSerializers)
                    {
                        s(v, f, c, value);
                    }
                    f.EndMap();
                }
            }

            static Serializer s_serializer;

            public static void Serialize(JsonObjectValidator validator,
                    IFormatter f, JsonSchemaValidationContext c, T value)
            {
                if (s_serializer == null)
                {
                    if (typeof(T) == typeof(object))
                    {
                        throw new ArgumentException("object cannot serialize");
                    }
                    var serializer = new Serializer();
                    var t = typeof(T);
                    UnityEngine.Debug.LogFormat("{0}", t.Name);
                    var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public);
                    foreach (var fi in fields)
                    {
                        serializer.AddField(fi);
                    }
                    s_serializer = serializer;
                }
                s_serializer.Serialize(validator, f, c, value);
            }
        }

        public void Serialize<T>(IFormatter f, JsonSchemaValidationContext c, T value)
        {
            GenericSerializer<T>.Serialize(this, f, c, value);
        }

        static class GenericDeserializer<T>
        {
            delegate T Deserializer(IValueNode src);

            static Deserializer s_d;

            delegate void FieldSetter(IValueNode s, object o);
            static FieldSetter GetFieldDeserializer<U>(FieldInfo fi)
            {
                return (s, o) =>
                {
                    var u = default(U);
                    s.Deserialize(ref u);
                    fi.SetValue(o, u);
                };
            }

            static U DeserializeField<U>(JsonSchema prop, IValueNode s)
            {
                var u = default(U);
                prop.Validator.Deserialize(s, ref u);
                return u;
            }

            public static void Deserialize(IValueNode src, ref T dst, Dictionary<string, JsonSchema> props)
            {
                if (s_d == null)
                {
                    var target = typeof(T);

                    var fields = target.GetFields(BindingFlags.Instance | BindingFlags.Public);
                    var fieldDeserializers = fields.ToDictionary(x => Utf8String.From(x.Name), x =>
                    {
                        /*
                        var mi = typeof(GenericDeserializer<T>).GetMethod("GetFieldDeserializer",
                            BindingFlags.Static | BindingFlags.NonPublic);
                        var g = mi.MakeGenericMethod(x.FieldType);
                        return (FieldSetter)g.Invoke(null, new object[] { x });
                        */
                        JsonSchema prop;
                        if (!props.TryGetValue(x.Name, out prop))
                        {
                            return null;
                        }

                        var mi = typeof(GenericDeserializer<T>).GetMethod("DeserializeField",
                            BindingFlags.Static | BindingFlags.NonPublic);
                        var g = mi.MakeGenericMethod(x.FieldType);

                        return (FieldSetter)((s, o) =>
                        {
                            var f = g.Invoke(null, new object[] { prop, s });
                            x.SetValue(o, f);
                        });
                    });

                    s_d = (IValueNode s) =>
                    {
                        if (!s.IsMap())
                        {
                            throw new ArgumentException(s.ValueType.ToString());
                        }

                        // boxing
                        var t = (object)Activator.CreateInstance<T>();
                        foreach (var kv in s.ObjectItems)
                        {
                            FieldSetter setter;
                            if (fieldDeserializers.TryGetValue(kv.Key, out setter))
                            {
                                if (setter != null)
                                {
                                    setter(kv.Value, t);
                                }
                            }
                        }
                        return (T)t;
                    };

                }
                dst = s_d(src);
            }
        }

        public void Deserialize<T>(IValueNode src, ref T dst)
        {
            GenericDeserializer<T>.Deserialize(src, ref dst, Properties);
        }
    }
}
