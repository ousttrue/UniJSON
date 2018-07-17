using System;
using System.Collections.Generic;
using System.Linq;


namespace UniJSON
{
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
                            .Where(y => y.Value.ValueType == JsonValueType.String)
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
            foreach (var x in Enum.GetValues(t))
            {
                if (excludes == null || !excludes.Contains(x))
                {
                    yield return filter(x.ToString());
                }
            }
        }

        static IEnumerable<int> GetIntValues(Type t, object[] excludes)
        {
            foreach (var x in Enum.GetValues(t))
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
                    return StringEnumValidator.Create(GetStringValues(t, excludes, x => x.ToLower()));

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
                    return IntEnumValidator.Create(values.Select(y => (int)y));
                }
            }

            throw new NotImplementedException();
        }
    }

    public class StringEnumValidator : JsonSchemaValidatorBase
    {
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

        public override bool Validate(object o)
        {
            return true;
        }

        public override void Serialize(JsonFormatter f, object o)
        {
            f.Value((string)o);
        }
    }

    public class IntEnumValidator : JsonSchemaValidatorBase
    {
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

        public override bool Validate(object o)
        {
            return true;
        }

        public override void Serialize(JsonFormatter f, object o)
        {
            f.Value((int)o);
        }
    }
}
