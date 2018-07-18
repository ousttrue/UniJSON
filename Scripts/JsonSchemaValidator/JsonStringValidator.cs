using System;


namespace UniJSON
{
    /// <summary>
    /// http://json-schema.org/latest/json-schema-validation.html#string
    /// </summary>
    public class JsonStringValidator : IJsonSchemaValidator
    {
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

        public void Assign(IJsonSchemaValidator obj)
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

        public bool Parse(IFileSystemAccessor fs, string key, JsonNode value)
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

        public bool Validate(object o)
        {
            var value = o as string;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (value.Length < MinLength)
            {
                return false;
            }
            if (value.Length > MaxLength)
            {
                return false;
            }

            return true;
        }

        public void Serialize(JsonFormatter f, object o)
        {
            f.Value((string)o);
        }
    }
}
