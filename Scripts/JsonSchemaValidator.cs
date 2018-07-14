using System;
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
        OneOf,
    }

    public abstract class JsonSchemaValidatorBase
    {
        public abstract JsonValueType JsonValueType { get; }

        public virtual void Assign(JsonSchemaValidatorBase rhs)
        {
            if (JsonValueType != rhs.JsonValueType)
            {
                throw new NotImplementedException();
            }
        }

        public abstract bool Parse(IFileSystemAccessor fs, string key, JsonNode value);
    }

    public class JsonBoolValidator: JsonSchemaValidatorBase
    {
        public override JsonValueType JsonValueType { get { return JsonValueType.Boolean; } }

        public override bool Parse(IFileSystemAccessor fs, string key, JsonNode value)
        {
            return false;
        }
    }

    /// <summary>
    /// http://json-schema.org/latest/json-schema-validation.html#numeric
    /// </summary>
    public class JsonIntValidator : JsonSchemaValidatorBase
    {
        public override JsonValueType JsonValueType { get { return JsonValueType.Integer; } }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.1
        /// </summary>
        public int? MultipleOf
        {
            get; private set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.2
        /// </summary>
        public int? Maximum
        {
            get; private set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.3
        /// </summary>
        public int? ExclusiveMaximum
        {
            get; private set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.4
        /// </summary>
        public int? Minimum
        {
            get; private set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.5
        /// </summary>
        public int? ExclusiveMinimum
        {
            get; private set;
        }

        public int? Default
        {
            get; private set;
        }

        public override bool Parse(IFileSystemAccessor fs, string key, JsonNode value)
        {
            switch (key)
            {
                case "multipleOf":
                    MultipleOf = value.GetInt32();
                    return true;

                case "maximum":
                    Maximum = value.GetInt32();
                    return true;

                case "exclusiveMaximum":
                    ExclusiveMaximum = value.GetInt32();
                    return true;

                case "minimum":
                    Minimum = value.GetInt32();
                    return true;

                case "exclusiveMinimum":
                    ExclusiveMinimum = value.GetInt32();
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// http://json-schema.org/latest/json-schema-validation.html#numeric
    /// </summary>
    public class JsonNumberValidator : JsonSchemaValidatorBase
    {
        public override JsonValueType JsonValueType { get { return JsonValueType.Number; } }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.1
        /// </summary>
        public double? MultipleOf
        {
            get; private set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.2
        /// </summary>
        public double? Maximum
        {
            get; private set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.3
        /// </summary>
        public double? ExclusiveMaximum
        {
            get; private set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.4
        /// </summary>
        public double? Minimum
        {
            get; private set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.5
        /// </summary>
        public double? ExclusiveMinimum
        {
            get; private set;
        }

        public double? Default
        {
            get; private set;
        }

        public override bool Parse(IFileSystemAccessor fs, string key, JsonNode value)
        {
            switch (key)
            {
                case "multipleOf":
                    MultipleOf = value.GetDouble();
                    return true;

                case "maximum":
                    Maximum = value.GetDouble();
                    return true;

                case "exclusiveMaximum":
                    ExclusiveMaximum = value.GetDouble();
                    return true;

                case "minimum":
                    Minimum = value.GetDouble();
                    return true;

                case "exclusiveMinimum":
                    ExclusiveMinimum = value.GetDouble();
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// http://json-schema.org/latest/json-schema-validation.html#string
    /// </summary>
    public class JsonStringValidator : JsonSchemaValidatorBase
    {
        public override JsonValueType JsonValueType { get { return JsonValueType.String; } }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.3.1
        /// </summary>
        public int? MaxLength
        {
            get; private set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.3.2
        /// </summary>
        public int? MinLength
        {
            get; private set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.3.3
        /// </summary>
        public string Pattern
        {
            get; private set;
        }

        public override bool Parse(IFileSystemAccessor fs, string key, JsonNode value)
        {
            switch(key)
            {
                case "maxLength":
                    MaxLength = value.GetInt32();
                    return true;

                case "minLength":
                    MinLength = value.GetInt32();
                    return true;

                case "pattern":
                    Pattern = value.GetString();
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.4
    /// </summary>
    public class JsonArrayValidator : JsonSchemaValidatorBase
    {
        public override JsonValueType JsonValueType { get { return JsonValueType.Array; } }

        public override bool Parse(IFileSystemAccessor fs, string key, JsonNode value)
        {
            switch (key)
            {
                case "items":
                    return true;

                case "additionalItems":
                    return true;

                case "maxItems":
                    return true;

                case "minItems":
                    return true;

                case "uniqueItems":
                    return true;

                case "contains":
                    return true;
            }

            return false;
        }
    }

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
            get; private set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.5.2
        /// </summary>
        public int MinProperties
        {
            get; private set;
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
    }
}
