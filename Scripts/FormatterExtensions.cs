﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace UniJSON
{
    public static class FormatterExtensions
    {
        public static IFormatter Value(this IFormatter f, object x)
        {
            var t = x.GetType();
            if (x == null)
            {
                f.Null();
            }
            else if (t == typeof(Boolean))
            {
                f.Value((Boolean)x);
            }
            else if (t == typeof(SByte))
            {
                f.Value((SByte)x);
            }
            else if (t == typeof(Int16))
            {
                f.Value((Int16)x);
            }
            else if (t == typeof(Int32))
            {
                f.Value((Int32)x);
            }
            else if (t == typeof(Int64))
            {
                f.Value((Int64)x);
            }
            else if (t == typeof(Byte))
            {
                f.Value((Byte)x);
            }
            else if (t == typeof(UInt16))
            {
                f.Value((UInt16)x);
            }
            else if (t == typeof(UInt32))
            {
                f.Value((UInt32)x);
            }
            else if (t == typeof(UInt64))
            {
                f.Value((UInt64)x);
            }
            else if (t == typeof(Single))
            {
                f.Value((Single)x);
            }
            else if (t == typeof(Double))
            {
                f.Value((Double)x);
            }
            else if (t == typeof(String))
            {
                f.Value((String)x);
            }
            else
            {
                throw new NotImplementedException();
            }
            return f;
        }

        public static IFormatter Value(this IFormatter f, object[] a)
        {
            f.BeginList(a.Length);
            foreach (var x in a)
            {
                f.Value(x);
            }
            f.EndList();
            return f;
        }

        public static IFormatter Value(this IFormatter f, List<object> a)
        {
            f.BeginList(a.Count);
            foreach (var x in a)
            {
                f.Value(x);
            }
            f.EndList();
            return f;
        }

        public static IFormatter Value(this IFormatter f, Byte[] value)
        {
            return f.Value(new ArraySegment<Byte>(value));
        }

        public static IFormatter Value(this IFormatter f, Vector3 v)
        {
            //CommaCheck();
            f.BeginMap(3);
            f.Key("x"); f.Value(v.x);
            f.Key("y"); f.Value(v.y);
            f.Key("z"); f.Value(v.z);
            f.EndMap();
            return f;
        }

        #region KeyValue
        static System.Reflection.MethodInfo GetMethod<T>(Expression<Func<T>> expression)
        {
            var formatterType = typeof(IFormatter);
            var method = formatterType.GetMethod("Value", new Type[] { typeof(T) });
            return method;
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

                var method = GetMethod(expression);
                method.Invoke(f, new object[] { value });
            }
        }
        #endregion

        public static ActionDisposer BeginListDisposable(this JsonFormatter f)
        {
            f.BeginList();
            return new ActionDisposer(() => f.EndList());
        }

        public static ActionDisposer BeginMapDisposable(this JsonFormatter f)
        {
            f.BeginMap();
            return new ActionDisposer(() => f.EndMap());
        }
    }
}
