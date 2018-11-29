using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace UniJSON
{
    public static partial class FormatterExtensions
    {
        public static void Value(this IFormatter f, Byte[] bytes)
        {
            f.Value(new ArraySegment<Byte>(bytes));
        }

        #region Serialize
        struct GenericSerializer<T>
        {
            delegate void Serializer(IFormatter f, T t);

            static Action<IFormatter, T> GetSerializer(Type t)
            {
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
                        var call = Expression.Call(typeof(FormatterExtensions), "SerializeDictionary",
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
                        var call = Expression.Call(typeof(FormatterExtensions), "SerializeObjectArray",
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
                        var call = Expression.Call(typeof(FormatterExtensions), "SerializeArray",
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

            static Serializer s_serializer
#if UNITY_EDITOR
#else
             = new Serializer(GetSerializer(typeof(T)))
#endif
            ;

            public void Serialize(IFormatter f, T t)
            {
#if UNITY_EDITOR
                if (s_serializer == null)
                {
                    s_serializer = new Serializer(GetSerializer(typeof(T)));
                }
#endif
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
                typeof(FormatterExtensions).GetMethod("Serialize").MakeGenericMethod(value.GetType()).Invoke(null, new object[] { f, value });
            }
        }

        public static void Serialize<T>(this IFormatter f, T arg)
        {
            if (arg == null)
            {
                f.Null();
                return;
            }

            (default(GenericSerializer<T>)).Serialize(f, arg);
        }
        #endregion

        static MethodInfo GetMethod<T>(Expression<Func<T>> expression)
        {
            var method = typeof(FormatterExtensions).GetMethod("Serialize");
            return method.MakeGenericMethod(typeof(T));
        }

        public static void KeyValue<T>(this IFormatter f, Expression<Func<T>> expression)
        {
            var func = expression.Compile();
            var value = func();
            if (value != null)
            {
                var body = expression.Body as MemberExpression;
                if (body == null)
                {
                    body = ((UnaryExpression)expression.Body).Operand as MemberExpression;
                }
                f.Key(body.Member.Name);
                f.Serialize(expression.Compile()());
                //var method = GetMethod(expression);
                //method.Invoke(this, new object[] { value });
            }
        }
    }
}
