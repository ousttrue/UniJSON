﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UniJSON
{
    public static class FormatterExtensionsSerializer
    {
        static Type SelfType = typeof(FormatterExtensionsSerializer);

        static class GenericSerializer<T>
        {
            delegate void Serializer(IFormatter f, T t);

            static Action<IFormatter, T> GetSerializer(Type t)
            {
                // object
                if(typeof(T)==typeof(object) && t.GetType() != typeof(object))
                {
                    var self = Expression.Parameter(typeof(IFormatter), "f");
                    var arg = Expression.Parameter(t, "value");
                    var call = Expression.Call(SelfType, "SerializeObject",
                        new Type[] { },
                        self, arg);
                    var lambda = Expression.Lambda(call, self, arg);
                    return (Action<IFormatter, T>)lambda.Compile();
                }

                try
                {
                    // primitive
                    var mi = typeof(IFormatter).GetMethod("Value", new Type[] { t });
                    if (mi != null)
                    {
                        // premitives
                        var self = Expression.Parameter(typeof(IFormatter), "f");
                        var arg = Expression.Parameter(t, "value");
                        var call = Expression.Call(self, mi, arg);

                        var lambda = Expression.Lambda(call, self, arg);
                        return (Action<IFormatter, T>)lambda.Compile();
                    }
                }
                catch (AmbiguousMatchException)
                {
                    // do nothing
                }

                {
                    // dictionary
                    var idictionary = t.GetInterfaces().FirstOrDefault(x =>
                    x.IsGenericType
                    && x.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                    && x.GetGenericArguments()[0] == typeof(string)
                    );
                    if (idictionary != null)
                    {
                        //var mi = typeof(IFormatter).GetMethod("SerializeDictionary", new Type[] { t });
                        var self = Expression.Parameter(typeof(IFormatter), "f");
                        var arg = Expression.Parameter(t, "value");
                        var call = Expression.Call(SelfType, "SerializeDictionary",
                            new Type[] { },
                            self, arg);
                        var lambda = Expression.Lambda(call, self, arg);
                        return (Action<IFormatter, T>)lambda.Compile();
                    }
                }

                {
                    // object[]
                    if (t == typeof(object[]))
                    {
                        var self = Expression.Parameter(typeof(IFormatter), "f");
                        var arg = Expression.Parameter(t, "value");
                        var call = Expression.Call(SelfType, "SerializeObjectArray",
                            new Type[] { },
                            self, arg);
                        var lambda = Expression.Lambda(call, self, arg);
                        return (Action<IFormatter, T>)lambda.Compile();
                    }
                }

                {
                    // list
                    var ienumerable = t.GetInterfaces().FirstOrDefault(x =>
                    x.IsGenericType
                    && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    );
                    if (ienumerable != null)
                    {
                        var self = Expression.Parameter(typeof(IFormatter), "f");
                        var arg = Expression.Parameter(t, "value");
                        var call = Expression.Call(SelfType, "SerializeArray",
                            ienumerable.GetGenericArguments(),
                            self, arg);
                        var lambda = Expression.Lambda(call, self, arg);
                        return (Action<IFormatter, T>)lambda.Compile();
                    }
                }

                {
                    // reflection
                    var schema = JsonSchema.FromType<T>();
                    return (IFormatter f, T value) => schema.Serialize(f, value);
                }

                //throw new NotImplementedException();
            }

            static Serializer s_serializer;

            public static void Set(Action<IFormatter, T> serializer)
            {
                s_serializer = new Serializer(serializer);
            }

            public static void Serialize(IFormatter f, T t)
            {
                if (s_serializer == null)
                {
                    s_serializer = new Serializer(GetSerializer(typeof(T)));
                }
                s_serializer(f, t);
            }
        }

        public static void SerializeDictionary(this IFormatter f, IDictionary<string, object> dictionary)
        {
            f.BeginMap(dictionary.Count);
            foreach (var kv in dictionary)
            {
                f.Key(kv.Key);
                f.SerializeObject(kv.Value);
            }
            f.EndMap();
        }

        public static void SerializeArray<T>(this IFormatter f, IEnumerable<T> values)
        {
            f.BeginList(values.Count());
            foreach (var value in values)
            {
                f.Serialize(value);
            }
            f.EndList();
        }

        public static void SerializeObjectArray(this IFormatter f, object[] array)
        {
            f.BeginList(array.Length);
            foreach (var x in array)
            {
                f.SerializeObject(x);
            }
            f.EndList();
        }

        public static void SerializeObject(this IFormatter f, object value)
        {
            if (value == null)
            {
                f.Null();
            }
            else
            {
                SelfType.GetMethod("Serialize").MakeGenericMethod(value.GetType()).Invoke(null, new object[] { f, value });
            }
        }

        public static void Serialize<T>(this IFormatter f, T arg)
        {
            if (arg == null)
            {
                f.Null();
                return;
            }

            GenericSerializer<T>.Serialize(f, arg);
        }

        public static void SetCustomSerializer<T>(Action<IFormatter, T> serializer)
        {
            GenericSerializer<T>.Set(serializer);
        }
    }
}
