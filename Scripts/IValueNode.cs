﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace UniJSON
{
    public enum ValueNodeType
    {
        Null,
        Boolean,
        String,
        Binary,
        Integer,
        Float,
        Array,
        Map,
    }

    public interface IValueNode<T> where T: IValueNode<T>
    {
        bool IsValid { get; }

        ArraySegment<Byte> Bytes { get; }

        ValueNodeType ValueType { get; }

        bool HasParent { get; }
        T Parent { get; }
        IEnumerable<T> ArrayItems { get; }
        IEnumerable<KeyValuePair<T, T>> ObjectItems { get; }

        int ValueCount { get; }
        int ValueIndex { get; }

        Boolean GetBoolean();
        String GetString();
        Utf8String GetUtf8String();
        SByte GetSByte();
        Int16 GetInt16();
        Int32 GetInt32();
        Int64 GetInt64();
        Byte GetByte();
        UInt16 GetUInt16();
        UInt32 GetUInt32();
        UInt64 GetUInt64();
        Single GetSingle();
        Double GetDouble();

        void SetValue<U>(Utf8String jsonPointer, U value);
        void RemoveValue(Utf8String jsonPointer);
    }

    public static class IValueNodeExtensions
    {
        public static bool IsNull<T>(this T self) where T : IValueNode<T>
        {
            return self.ValueType == ValueNodeType.Null;
        }

        public static bool IsBoolean<T>(this T self) where T : IValueNode<T>
        {
            return self.ValueType == ValueNodeType.Boolean;
        }

        public static bool IsString<T>(this T self) where T : IValueNode<T>
        {
            return self.ValueType == ValueNodeType.String;
        }

        public static bool IsInteger<T>(this T self) where T : IValueNode<T>
        {
            return self.ValueType == ValueNodeType.Integer;
        }

        public static bool IsFloat<T>(this T self) where T : IValueNode<T>
        {
            return self.ValueType == ValueNodeType.Float;
        }

        public static bool IsArray<T>(this T self) where T : IValueNode<T>
        {
            return self.ValueType == ValueNodeType.Array;
        }

        public static bool IsMap<T>(this T self) where T : IValueNode<T>
        {
            return self.ValueType == ValueNodeType.Map;
        }

        public static JsonPointer<T> Pointer<T>(this T self) where T : IValueNode<T>
        {
            return new JsonPointer<T>(self);
        }

        public static IEnumerable<T> Path<T>(this T self) where T : IValueNode<T>
        {
            if (self.HasParent)
            {
                foreach (var x in self.Parent.Path())
                {
                    yield return x;
                }
            }
            yield return self;
        }

        public static int IndexOf<T>(this T self, T child) where T : IValueNode<T>
        {
            int i = 0;
            foreach (var v in self.ArrayItems)
            {
                if (v.ValueIndex == child.ValueIndex)
                {
                    return i;
                }
                ++i;
            }
            throw new KeyNotFoundException();
        }

        public static bool ContainsKey<T>(this T self, Utf8String key) where T : IValueNode<T>
        {
            return self.ObjectItems.Any(x => x.Key.GetUtf8String() == key);
        }

        public static bool ContainsKey<T>(this T self, String key) where T : IValueNode<T>
        {
            var ukey = Utf8String.From(key);
            return self.ContainsKey(ukey);
        }

        public static Utf8String KeyOf<T>(this T self, T node) where T : IValueNode<T>
        {
            foreach (var kv in self.ObjectItems)
            {
                if (node.ValueIndex == kv.Value.ValueIndex)
                {
                    return kv.Key.GetUtf8String();
                }
            }
            throw new KeyNotFoundException();
        }

        public static IEnumerable<T> Traverse<T>(this T self) where T : IValueNode<T>
        {
            yield return self;
            if (self.IsArray())
            {
                foreach (var x in self.ArrayItems)
                {
                    foreach (var y in x.Traverse())
                    {
                        yield return y;
                    }
                }
            }
            else if (self.IsMap())
            {
                foreach (var kv in self.ObjectItems)
                {
                    foreach (var y in kv.Value.Traverse())
                    {
                        yield return y;
                    }
                }
            }
        }

        public static void SetValue<T>(this T self, String jsonPointer, T value) where T : IValueNode<T>
        {
            self.SetValue<T>(Utf8String.From(jsonPointer), value);
        }

        public static void RemoveValue<T>(this T self, String jsonPointer)where T: IValueNode<T>
        {
            self.RemoveValue(Utf8String.From(jsonPointer));
        }

        #region Deserializer
        struct GenericCreator<S, T> where S : IValueNode<S>
        {
            static U[] ArrayCreator<U>(S src)
            {
                if (!src.IsArray())
                {
                    throw new ArgumentException("value is not array");
                }
                var count = src.ValueCount;
                return new U[count];
            }

            static Func<S, T> GetCreator()
            {
                var t = typeof(T);
                if (t.IsArray)
                {
                    var mi = typeof(GenericCreator<S, T>).GetMethod("ArrayCreator",
                        BindingFlags.NonPublic | BindingFlags.Static);
                    var g = mi.MakeGenericMethod(t.GetElementType());
                    var src = Expression.Parameter(typeof(S), "src");
                    var call = Expression.Call(g, src);
                    var func = Expression.Lambda(call, src);
                    return (Func<S, T>)func.Compile();
                }

                {
                    return _s =>
                    {
                        return Activator.CreateInstance<T>();
                    };
                }
            }

            delegate T Creator(S src);

            static Creator s_creator;

            public T Create(S src)
            {
                if (s_creator == null)
                {
                    var d = GetCreator();
                    s_creator = new Creator(d);
                }
                return s_creator(src);
            }
        }

        static object DictionaryDeserializer<T>(T s)where T: IValueNode<T>
        {
            switch (s.ValueType)
            {
                case ValueNodeType.Map:
                    {
                        var u = new Dictionary<string, object>();
                        foreach (var kv in s.ObjectItems)
                        {
                            //var e = default(object);
                            //kv.Value.Deserialize(ref e);
                            u.Add(kv.Key.GetString(), DictionaryDeserializer(kv.Value));
                        }
                        return u;
                    }

                case ValueNodeType.Null:
                    return null;

                case ValueNodeType.Boolean:
                    return s.GetBoolean();

                case ValueNodeType.Integer:
                    return s.GetInt32();

                case ValueNodeType.Float:
                    return s.GetDouble();

                case ValueNodeType.String:
                    return s.GetString();

                default:
                    throw new NotImplementedException(s.ValueType.ToString());
            }
        }

        static class GenericDeserializer<S, T> where S : IValueNode<S>
        {
            static U[] GenericArrayDeserializer<U>(S s)
            {
                if (!s.IsArray())
                {
                    throw new ArgumentException("not array: " + s.ValueType);
                }
                var u = new U[s.ValueCount];
                int i = 0;
                foreach (var x in s.ArrayItems)
                {
                    x.Deserialize(ref u[i++]);
                }
                return u;
            }

            static List<U> GenericListDeserializer<U>(S s)
            {
                if (!s.IsArray())
                {
                    throw new ArgumentException("not array: " + s.ValueType);
                }
                var u = new List<U>(s.ValueCount);
                foreach (var x in s.ArrayItems)
                {
                    var e = default(U);
                    x.Deserialize(ref e);
                    u.Add(e);
                }
                return u;
            }

            delegate void FieldSetter(S s, object o);
            static FieldSetter GetFieldDeserializer<U>(FieldInfo fi)
            {
                return (s, o) =>
                {
                    var u = default(U);
                    s.Deserialize(ref u);
                    fi.SetValue(o, u);
                };
            }

            static Func<S, T> GetDeserializer()
            {
                // primitive
                {
                    var mi = typeof(S).GetMethods().FirstOrDefault(x =>
                    {
                        if (!x.Name.StartsWith("Get"))
                        {
                            return false;
                        }

                        if (!x.Name.EndsWith(typeof(T).Name))
                        {
                            return false;
                        }

                        var parameters = x.GetParameters();
                        if (parameters.Length != 0)
                        {
                            return false;
                        }

                        if (x.ReturnType != typeof(T))
                        {
                            return false;
                        }

                        return true;
                    });

                    if (mi != null)
                    {
                        var self = Expression.Parameter(typeof(S), "self");
                        var call = Expression.Call(self, mi);
                        var func = Expression.Lambda(call, self);
                        return (Func<S, T>)func.Compile();
                    }
                }

                var target = typeof(T);

                if (target.IsArray)
                {
                    var mi = typeof(GenericDeserializer<S, T>).GetMethod("GenericArrayDeserializer",
                        BindingFlags.Static | BindingFlags.NonPublic);
                    var g = mi.MakeGenericMethod(target.GetElementType());
                    var self = Expression.Parameter(typeof(S), "self");
                    var call = Expression.Call(g, self);
                    var func = Expression.Lambda(call, self);
                    return (Func<S, T>)func.Compile();
                }

                if (target.IsGenericType)
                {
                    if (target.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var mi = typeof(GenericDeserializer<S, T>).GetMethod("GenericListDeserializer",
                            BindingFlags.Static | BindingFlags.NonPublic);
                        var g = mi.MakeGenericMethod(target.GetGenericArguments());
                        var self = Expression.Parameter(typeof(S), "self");
                        var call = Expression.Call(g, self);
                        var func = Expression.Lambda(call, self);
                        return (Func<S, T>)func.Compile();
                    }

                    if (target.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
                        target.GetGenericArguments()[0] == typeof(string))
                    {
                        var mi = typeof(IValueNodeExtensions).GetMethod("DictionaryDeserializer",
                        BindingFlags.Static | BindingFlags.NonPublic);
                        var g = mi.MakeGenericMethod(typeof(S));
                        var self = Expression.Parameter(typeof(S), "self");
                        var call = Expression.Call(g, self);
                        var func = Expression.Lambda(call, self);
                        var d = (Func<S, object>)func.Compile();
                        return (S s) =>
                        {
                            var x = d(s);
                            return (T)x;
                        };
                    }
                }

                {
                    var schema = JsonSchema.FromType<T>();
                    return s =>
                    {
                        var t = default(T);
                        schema.Validator.Deserialize(s, ref t);
                        return t;
                    };
                }

#if false
                if (target.IsEnum)
                {
                    var value = Expression.Parameter(typeof(int), "value");
                    var cast = Expression.Convert(value, target);
                    var func = Expression.Lambda(cast, value);
                    var compiled = (Func<int, T>)func.Compile();
                    return s =>
                    {
                        return compiled(s.GetInt32());
                    };
                }

                {
                    var fields = target.GetFields(BindingFlags.Instance | BindingFlags.Public);
                    var fieldDeserializers = fields.ToDictionary(x => Utf8String.From(x.Name), x =>
                    {
                        var mi = typeof(GenericDeserializer<S, T>).GetMethod("GetFieldDeserializer",
                            BindingFlags.Static|BindingFlags.NonPublic);
                        var g = mi.MakeGenericMethod(x.FieldType);
                        return (FieldSetter)g.Invoke(null, new object[] { x });
                    });
                    
                    return (S s) =>
                    {
                        if (!s.IsMap())
                        {
                            throw new ArgumentException(s.ValueType.ToString());
                        }

                        var t = (object)default(GenericCreator<S, T>).Create(s);
                        foreach(var kv in s.ObjectItems)
                        {
                            FieldSetter setter;
                            if (fieldDeserializers.TryGetValue(kv.Key, out setter))
                            {
                                setter(kv.Value, t);
                            }
                        }
                        return (T)t;
                    };
                }
#endif
            }

            delegate T Deserializer(S node);

            static Deserializer s_deserializer;

            public static void Deserialize(S node, ref T value)
            {
                if (s_deserializer == null)
                {
                    var d = GetDeserializer();
                    s_deserializer = new Deserializer(d);
                }
                value = s_deserializer(node);
            }
        }

        public static void Deserialize<S, T>(this S self, ref T value) where S : IValueNode<S>
        {
            GenericDeserializer<S, T>.Deserialize(self, ref value);
        }

        public static bool GetBoolean<T>(this T self) where T : IValueNode<T>
        {
            var value = default(bool);
            self.Deserialize(ref value);
            return value;
        }
#endregion
    }
}
