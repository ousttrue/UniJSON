using System;
using System.Collections.Generic;


namespace UniJSON
{
    /// <summary>
    /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5
    /// </summary>
    public class JsonObjectValidator : JsonSchemaValidatorBase
    {
        public override JsonValueType JsonValueType { get { return JsonValueType.Object; } }

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

        Dictionary<string, JsonSchema> m_props = new Dictionary<string, JsonSchema>();
        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5.4
        /// </summary>
        public Dictionary<string, JsonSchema> Properties
        {
            get { return m_props; }
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5.5
        /// </summary>
        public string PatternProperties
        {
            get; private set;
        }

        public override void Assign(JsonSchemaValidatorBase obj)
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
        }

        public void AddProperty(IFileSystemAccessor fs, string key, JsonNode value)
        {
            var sub = new JsonSchema();
            sub.Parse(fs, value, key);

            if (Properties.ContainsKey(key))
            {
                if (sub.Validator != null)
                {
                    Properties[key].Validator.Assign(sub.Validator);
                }
            }
            else
            {
                Properties.Add(key, sub);
            }
        }

        public override bool Parse(IFileSystemAccessor fs, string key, JsonNode value)
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
                        foreach (var req in value.ArrayItems)
                        {
                            m_required.Add(req.GetString());
                        }
                    }
                    return true;

                case "properties":
                    {
                        foreach (var prop in value.ObjectItems)
                        {
                            AddProperty(fs, prop.Key, prop.Value);
                        }
                    }
                    return true;

                case "patternProperties":
                    PatternProperties = value.GetString();
                    return true;

                case "additionalProperties":
                    return true;

                case "dependencies":
                    return true;

                case "propertyNames":
                    return true;
            }

            return false;
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
                        Console.WriteLine(string.Format("{0}", pair.Key));
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

            return true;
        }

        public override void Serialize(JsonFormatter f, Object o)
        {
            f.BeginMap();
            foreach (var kv in Properties)
            {
                var value = o.GetValue(kv.Key);
                var v = kv.Value.Validator;
                if (v != null && v.Validate(value))
                {
                    // key
                    f.Key(kv.Key);

                    // value
                    v.Serialize(f, value);
                }
            }
            f.EndMap();
        }
    }
}
