using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
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

        class LockQueue<T> where T : class
        {
            Queue<T> m_queue = new Queue<T>();

            public void Enqueue(T t)
            {
                lock (((ICollection)m_queue).SyncRoot)
                {
                    m_queue.Enqueue(t);
                }
            }
            public T Dequeue()
            {
                T t = null;
                lock (((ICollection)m_queue).SyncRoot)
                {
                    if (m_queue.Count > 0)
                    {
                        t = m_queue.Dequeue();
                    }
                }
                return t;
            }
        }
        static LockQueue<Dictionary<string, object>> s_validValueMap = new LockQueue<Dictionary<string, object>>();

        public void Serialize<T>(IFormatter f, JsonSchemaValidationContext c, T o)
        {
            var map = s_validValueMap.Dequeue();
            if (map == null)
            {
                map = new Dictionary<string, object>();
            }

            // validate properties
            map.Clear();
            foreach (var kv in Properties)
            {
                var value = o.GetValueByKey(kv.Key);
                var v = kv.Value.Validator;
                using (c.Push(kv.Key))
                {
                    if (v != null && v.Validate(c, value) == null)
                    {
                        map.Add(kv.Key, value);
                    }
                }
            }

            f.BeginMap(Properties.Count);
            {
                foreach (var kv in Properties)
                {
                    object value;
                    if (!map.TryGetValue(kv.Key, out value))
                    {
                        continue;
                    }

                    string[] dependencies;
                    if (Dependencies.TryGetValue(kv.Key, out dependencies))
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

                    if (kv.Value.Validator.Validate(c, value) == null)
                    {
                        // key
                        f.Key(kv.Key);

                        // value
                        using (c.Push(kv.Key))
                        {
                            kv.Value.Validator.Serialize(f, c, value);
                        }
                    }
                }
            }
            f.EndMap();

            s_validValueMap.Enqueue(map);
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
                        if(!props.TryGetValue(x.Name, out prop))
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
