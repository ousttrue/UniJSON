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
        OneOf,
    }

    public abstract class JsonSchemaValidatorBase
    {
        public abstract JsonValueType JsonValueType { get; }

        public abstract void Assign(JsonSchemaValidatorBase obj);

        public abstract bool Parse(IFileSystemAccessor fs, string key, JsonNode value);
    }

    public class JsonBoolValidator : JsonSchemaValidatorBase
    {
        public override JsonValueType JsonValueType { get { return JsonValueType.Boolean; } }

        public override void Assign(JsonSchemaValidatorBase obj)
        {
            throw new NotImplementedException();
        }

        public override bool Parse(IFileSystemAccessor fs, string key, JsonNode value)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return 1;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonBoolValidator;
            if (rhs == null) return false;
            return true;
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
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.2
        /// </summary>
        public int? Maximum
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.3
        /// </summary>
        public bool ExclusiveMaximum
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.4
        /// </summary>
        public int? Minimum
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.5
        /// </summary>
        public bool ExclusiveMinimum
        {
            get; set;
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
                    ExclusiveMaximum = value.GetBoolean();
                    return true;

                case "minimum":
                    Minimum = value.GetInt32();
                    return true;

                case "exclusiveMinimum":
                    ExclusiveMinimum = value.GetBoolean();
                    return true;
            }

            return false;
        }

        public override void Assign(JsonSchemaValidatorBase obj)
        {
            var rhs = obj as JsonIntValidator;
            if (rhs == null)
            {
                throw new ArgumentException();
            }

            MultipleOf = rhs.MultipleOf;
            Maximum = rhs.Maximum;
            ExclusiveMaximum = rhs.ExclusiveMaximum;
            Minimum = rhs.Minimum;
            ExclusiveMinimum = rhs.ExclusiveMinimum;
        }

        public override int GetHashCode()
        {
            return 2;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonIntValidator;
            if (rhs == null) return false;

            if (MultipleOf != rhs.MultipleOf)
            {
                Console.WriteLine("MultipleOf");
                return false;
            }
            if (Maximum != rhs.Maximum)
            {
                Console.WriteLine("Maximum");
                return false;
            }

            if (ExclusiveMaximum != rhs.ExclusiveMaximum)
            {
                Console.WriteLine("ExclusiveMaximum");
                return false;
            }

            if (Minimum != rhs.Minimum)
            {
                Console.WriteLine("Minimum");
                return false;
            }

            if (ExclusiveMinimum != rhs.ExclusiveMinimum)
            {
                Console.WriteLine("ExclusiveMinimum");
                return false;
            }

            return true;
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
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.2
        /// </summary>
        public double? Maximum
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.3
        /// </summary>
        public bool ExclusiveMaximum
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.4
        /// </summary>
        public double? Minimum
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.2.5
        /// </summary>
        public bool ExclusiveMinimum
        {
            get; set;
        }

        public override void Assign(JsonSchemaValidatorBase rhs)
        {
            throw new NotImplementedException();
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
                    ExclusiveMaximum = value.GetBoolean();
                    return true;

                case "minimum":
                    Minimum = value.GetDouble();
                    return true;

                case "exclusiveMinimum":
                    ExclusiveMinimum = value.GetBoolean();
                    return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return 3;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonNumberValidator;
            if (rhs == null) return false;

            if (MultipleOf != rhs.MultipleOf) return false;
            if (Maximum != rhs.Maximum) return false;
            if (ExclusiveMaximum != rhs.ExclusiveMaximum) return false;
            if (Minimum != rhs.Minimum) return false;
            if (ExclusiveMinimum != rhs.ExclusiveMinimum) return false;

            return true;
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
            get; set;
        }

        public override void Assign(JsonSchemaValidatorBase obj)
        {
            var rhs = obj as JsonStringValidator;
            if (rhs == null)
            {
                throw new ArgumentException();
            }

            MaxLength = rhs.MaxLength;
            MinLength = rhs.MinLength;
            Pattern = rhs.Pattern;
        }

        public override bool Parse(IFileSystemAccessor fs, string key, JsonNode value)
        {
            switch (key)
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

        public override int GetHashCode()
        {
            return 4;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonStringValidator;
            if (rhs == null) return false;

            if (MaxLength != rhs.MaxLength) return false;
            if (MinLength != rhs.MinLength) return false;
            if (Pattern != rhs.Pattern) return false;

            return true;
        }
    }

    /// <summary>
    /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.4
    /// </summary>
    public class JsonArrayValidator : JsonSchemaValidatorBase
    {
        public override JsonValueType JsonValueType { get { return JsonValueType.Array; } }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.4.1
        /// </summary>
        public JsonSchema Items
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.4.3
        /// </summary>
        public int? MaxItems
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.4.4
        /// </summary>
        public int? MinItems
        {
            get; set;
        }

        public override void Assign(JsonSchemaValidatorBase rhs)
        {
            throw new NotImplementedException();
        }

        public override bool Parse(IFileSystemAccessor fs, string key, JsonNode value)
        {
            switch (key)
            {
                case "items":
                    if (value.Value.ValueType == JsonValueType.Array)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        var sub = new JsonSchema();
                        sub.Parse(fs, value, "items");
                        Items = sub;
                    }
                    return true;

                case "additionalItems":
                    return true;

                case "maxItems":
                    MaxItems = value.GetInt32();
                    return true;

                case "minItems":
                    MinItems = value.GetInt32();
                    return true;

                case "uniqueItems":
                    return true;

                case "contains":
                    return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return 5;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonArrayValidator;
            if (rhs == null) return false;

            if (Items != rhs.Items) return false;
            if (MaxItems != rhs.MaxItems) return false;
            if (MinItems != rhs.MinItems) return false;

            return true;
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
    }

    public static class EnumValidator
    {
        public static JsonSchemaValidatorBase Create(JsonNode value)
        {
            foreach (var x in value.ArrayItems)
            {
                switch (x.Value.ValueType)
                {
                    case JsonValueType.Integer:
                    case JsonValueType.Number:
                        return IntEnumValidator.Create(value.ArrayItems
                            .Where(y => y.Value.ValueType == JsonValueType.Integer || y.Value.ValueType == JsonValueType.Number)
                            .Select(y => y.GetInt32())
                            );

                    case JsonValueType.String:
                        return StringEnumValidator.Create(value.ArrayItems
                            .Where(y => y.Value.ValueType==JsonValueType.String)
                            .Select(y => y.GetString())
                            );

                    default:
                        break;
                }
            }

            throw new NotImplementedException();
        }

        public static JsonSchemaValidatorBase Create(IEnumerable<JsonSchema> composition)
        {
            foreach (var x in composition)
            {
                if (x.Validator is StringEnumValidator)
                {
                    return StringEnumValidator.Create(composition
                        .Select(y => y.Validator as StringEnumValidator)
                        .Where(y => y != null)
                        .SelectMany(y => y.Values)
                        );
                }
                if (x.Validator is IntEnumValidator)
                {
                    return IntEnumValidator.Create(composition
                        .Select(y => y.Validator as IntEnumValidator)
                        .Where(y => y != null)
                        .SelectMany(y => y.Values)
                        );
                }
            }

            throw new NotImplementedException();
        }

        static IEnumerable<string> GetStringValues(Type t, object[] excludes, Func<String, String> filter)
        {
            foreach(var x in Enum.GetValues(t))
            {
                if (excludes == null || !excludes.Contains(x))
                {
                    yield return filter(x.ToString());
                }
            }
        }

        static IEnumerable<int> GetIntValues(Type t, object[] excludes)
        {
            foreach(var x in Enum.GetValues(t))
            {
                if (excludes == null || !excludes.Contains(x))
                {
                    yield return (int)x;
                }
            }
        }

        public static JsonSchemaValidatorBase Create(Type t, EnumSerializationType serializationType, object[] excludes)
        {
            switch (serializationType)
            {
                case EnumSerializationType.AsLowerString:
                    return StringEnumValidator.Create(GetStringValues(t, excludes, x=>x.ToLower()));

                case EnumSerializationType.AsInt:
                    return IntEnumValidator.Create(GetIntValues(t, excludes));

                default:
                    throw new NotImplementedException();
            }
        }

        public static JsonSchemaValidatorBase Create(object[] values)
        {
            foreach (var x in values)
            {
                if (x is string)
                {
                    return StringEnumValidator.Create(values.Select(y => (string)y));
                }
                if (x is int)
                {
                    return IntEnumValidator.Create(values.Select(y =>(int)y));
                }
            }

            throw new NotImplementedException();
        }
    }

    public class StringEnumValidator : JsonSchemaValidatorBase
    {
        public override JsonValueType JsonValueType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public String[] Values
        {
            get; set;
        }

        public static StringEnumValidator Create(IEnumerable<string> values)
        {
            return new StringEnumValidator
            {
                Values = values.ToArray(),
            };
        }

        public override void Assign(JsonSchemaValidatorBase obj)
        {
            throw new NotImplementedException();
        }

        public override bool Parse(IFileSystemAccessor fs, string key, JsonNode value)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return 7;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as StringEnumValidator;
            if (rhs == null) return false;

            if (Values.Length != rhs.Values.Length) return false;

            var l = Values.OrderBy(x => x).GetEnumerator();
            var r = rhs.Values.OrderBy(x => x).GetEnumerator();
            while (l.MoveNext() && r.MoveNext())
            {
                if (l.Current != r.Current)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class IntEnumValidator : JsonSchemaValidatorBase
    {
        public override JsonValueType JsonValueType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int[] Values
        {
            get; set;
        }

        public static IntEnumValidator Create(IEnumerable<int> values)
        {
            return new IntEnumValidator
            {
                Values = values.ToArray()
            };
        }

        public override void Assign(JsonSchemaValidatorBase obj)
        {
            throw new NotImplementedException();
        }

        public override bool Parse(IFileSystemAccessor fs, string key, JsonNode value)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return 7;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as IntEnumValidator;
            if (rhs == null) return false;

            if (Values.Length != rhs.Values.Length) return false;

            var l = Values.OrderBy(x => x).GetEnumerator();
            var r = rhs.Values.OrderBy(x => x).GetEnumerator();
            while (l.MoveNext() && r.MoveNext())
            {
                if (l.Current != r.Current)
                {
                    return false;
                }
            }
            return true;
        }
    }

}
