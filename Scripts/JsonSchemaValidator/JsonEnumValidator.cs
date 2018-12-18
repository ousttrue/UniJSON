using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UniJSON
{
    public static class JsonEnumValidator
    {
        public static IJsonSchemaValidator Create(JsonNode value, EnumSerializationType type)
        {
            foreach (var x in value.ArrayItemsRaw)
            {
                if (x.IsInteger() || x.IsFloat())
                {
                    return JsonIntEnumValidator.Create(value.ArrayItemsRaw
                        .Where(y => y.IsInteger() || y.IsFloat())
                        .Select(y => y.GetInt32())
                        );
                }
                else if (x.IsString())
                {

                    return JsonStringEnumValidator.Create(value.ArrayItemsRaw
                        .Where(y => y.IsString())
                        .Select(y => y.GetString())
                        , type
                        );
                }
                else
                {
                }
            }

            throw new NotImplementedException();
        }

        public static IJsonSchemaValidator Create(IEnumerable<JsonSchema> composition, EnumSerializationType type)
        {
            foreach (var x in composition)
            {
                if (x.Validator is JsonStringEnumValidator)
                {
                    return JsonStringEnumValidator.Create(composition
                        .Select(y => y.Validator as JsonStringEnumValidator)
                        .Where(y => y != null)
                        .SelectMany(y => y.Values),
                        type
                        );
                }
                if (x.Validator is JsonIntEnumValidator)
                {
                    return JsonIntEnumValidator.Create(composition
                        .Select(y => y.Validator as JsonIntEnumValidator)
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

        public static IJsonSchemaValidator Create(Type t, EnumSerializationType serializationType, object[] excludes)
        {
            switch (serializationType)
            {
                case EnumSerializationType.AsInt:
                    return JsonIntEnumValidator.Create(GetIntValues(t, excludes));

                case EnumSerializationType.AsString:
                    return JsonStringEnumValidator.Create(GetStringValues(t, excludes, x => x), serializationType);

                case EnumSerializationType.AsLowerString:
                    return JsonStringEnumValidator.Create(GetStringValues(t, excludes, x => x.ToLower()), serializationType);

                case EnumSerializationType.AsUpperString:
                    return JsonStringEnumValidator.Create(GetStringValues(t, excludes, x => x.ToUpper()), serializationType);

                default:
                    throw new NotImplementedException();
            }
        }

        public static IJsonSchemaValidator Create(object[] values, EnumSerializationType type)
        {
            foreach (var x in values)
            {
                if (x is string)
                {
                    return JsonStringEnumValidator.Create(values.Select(y => (string)y), type);
                }
                if (x is int)
                {
                    return JsonIntEnumValidator.Create(values.Select(y => (int)y));
                }
            }

            throw new NotImplementedException();
        }
    }

    public class JsonStringEnumValidator : IJsonSchemaValidator
    {
        EnumSerializationType SerializationType;

        public String[] Values
        {
            get; set;
        }

        JsonStringEnumValidator(IEnumerable<string> values, EnumSerializationType type)
        {
            SerializationType = type;
            switch (SerializationType)
            {
                case EnumSerializationType.AsString:
                    Values = values.ToArray();
                    break;

                case EnumSerializationType.AsLowerString:
                    Values = values.Select(x => x.ToLower()).ToArray();
                    break;

                case EnumSerializationType.AsUpperString:
                    Values = values.Select(x => x.ToUpper()).ToArray();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public static JsonStringEnumValidator Create(IEnumerable<string> values, EnumSerializationType type)
        {
            return new JsonStringEnumValidator(values, type);
        }

        public override int GetHashCode()
        {
            return 7;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonStringEnumValidator;
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

        public void Merge(IJsonSchemaValidator obj)
        {
            throw new NotImplementedException();
        }

        public bool FromJsonSchema(IFileSystemAccessor fs, string key, JsonNode value)
        {
            throw new NotImplementedException();
        }

        public JsonSchemaValidationException Validate<T>(JsonSchemaValidationContext c, T o)
        {
            if (o == null)
            {
                return new JsonSchemaValidationException(c, "null");
            }

            var t = o.GetType();
            string value = null;
            if (t.IsEnum)
            {
                value = Enum.GetName(t, o);
            }
            else
            {
                value = GenericCast<T, string>.Cast(o);
            }

            if (SerializationType == EnumSerializationType.AsLowerString)
            {
                value = value.ToLower();
            }
            else if (SerializationType == EnumSerializationType.AsUpperString)
            {
                value = value.ToUpper();
            }

            if (Values.Contains(value))
            {
                return null;
            }
            else
            {
                return new JsonSchemaValidationException(c, string.Format("{0} is not valid enum", o));
            }
        }

        public void Serialize(IFormatter f, JsonSchemaValidationContext c, object o)
        {
            var t = o.GetType();

            var value = default(string);
            if (t.IsEnum)
            {
                value = Enum.GetName(t, o);
            }
            else
            {
                value = (string)o;
            }

            if (SerializationType == EnumSerializationType.AsLowerString)
            {
                value = value.ToLower();
            }
            else if (SerializationType == EnumSerializationType.AsUpperString)
            {
                value = value.ToUpper();
            }

            f.Value(value);
        }

        public void ToJsonScheama(IFormatter f)
        {
            f.Key("type"); f.Value("string");
            f.Key("enum");
            f.BeginList(Values.Length);
            foreach (var x in Values)
            {
                f.Value(x);
            }
            f.EndList();
        }

        static class GenericDeserializer<T>
        {
            delegate T Deserializer(IValueNode src);
            static Deserializer s_d;
            public static void Deserialize(IValueNode src, ref T t)
            {
                if (s_d == null)
                {
                    if (typeof(T).IsEnum)
                    {
                        // enum from string
                        var mi = typeof(Enum).GetMethods(BindingFlags.Static | BindingFlags.Public).First(
                            x => x.Name == "Parse" && x.GetParameters().Length == 3
                            );
                        var type = Expression.Constant(typeof(T));
                        var value = Expression.Parameter(typeof(string), "value");
                        var ic = Expression.Constant(true);
                        var call = Expression.Call(mi, type, value, ic);
                        var lambda = Expression.Lambda(call, value);
                        var func = (Func<string, object>)lambda.Compile();
                        s_d = x => GenericCast<object, T>.Cast(func(x.GetString()));
                    }
                    else
                    {
                        s_d = x => GenericCast<string, T>.Cast(x.GetString());
                    }
                }
                t = s_d(src);
            }
        }

        public void Deserialize<T>(IValueNode src, ref T dst)
        {
            GenericDeserializer<T>.Deserialize(src, ref dst);
        }
    }

    public class JsonIntEnumValidator : IJsonSchemaValidator
    {
        public int[] Values
        {
            get; set;
        }

        public static JsonIntEnumValidator Create(IEnumerable<int> values)
        {
            return new JsonIntEnumValidator
            {
                Values = values.ToArray()
            };
        }

        public override int GetHashCode()
        {
            return 7;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonIntEnumValidator;
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

        public void Merge(IJsonSchemaValidator obj)
        {
            throw new NotImplementedException();
        }

        public bool FromJsonSchema(IFileSystemAccessor fs, string key, JsonNode value)
        {
            throw new NotImplementedException();
        }

        public JsonSchemaValidationException Validate<T>(JsonSchemaValidationContext c, T o)
        {
            if (Values.Contains(GenericCast<T, int>.Cast(o)))
            {
                return null;
            }
            else
            {
                return new JsonSchemaValidationException(c, string.Format("{0} is not valid enum", o));
            }
        }

        public void Serialize(IFormatter f, JsonSchemaValidationContext c, object o)
        {
            f.Value((int)o);
        }

        public void ToJsonScheama(IFormatter f)
        {
            f.Key("type"); f.Value("integer");
        }

        static class GenericDeserializer<T>
        {
            delegate T Deserializer(IValueNode src);

            static Deserializer s_d;

            public static void Deserialize(IValueNode src, ref T dst)
            {
                if (s_d == null)
                {
                    // enum from int
                    var value = Expression.Parameter(typeof(int), "value");
                    var cast = Expression.Convert(value, typeof(T));
                    var lambda = Expression.Lambda(cast, value);
                    var func = (Func<int, T>)lambda.Compile();
                    s_d = s => func(s.GetInt32());
                }
                dst = s_d(src);
            }
        }

        public void Deserialize<T>(IValueNode src, ref T dst)
        {
            GenericDeserializer<T>.Deserialize(src, ref dst);
        }
    }
}
