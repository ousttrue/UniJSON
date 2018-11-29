using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;


namespace UniJSON
{
    public interface IValueNode
    {
        ArraySegment<Byte> Bytes { get; }

        bool IsNull { get; }
        bool IsBoolean { get; }
        bool IsString { get; }
        bool IsInteger { get; }
        bool IsFloat { get; }

        bool HasParent { get; }
        IValueNode Parent { get; }

        bool IsArray { get; }
        IEnumerable<IValueNode> ArrayItems { get; }

        bool IsMap { get; }
        IEnumerable<KeyValuePair<string, IValueNode>> ObjectItems { get; }

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
    }

    public static class IValueNodeExtensions
    {
        public static IEnumerable<IValueNode> Path<T>(this T self) where T : IValueNode
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

        public static int IndexOf<T>(this T self, T child) where T : IValueNode
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

        public static string KeyOf<T>(this T self, T node) where T : IValueNode
        {
            foreach (var kv in self.ObjectItems)
            {
                if (node.ValueIndex == kv.Value.ValueIndex)
                {
                    return kv.Key;
                }
            }
            throw new KeyNotFoundException();
        }

        public static IEnumerable<IValueNode> Traverse<T>(this T self) where T : IValueNode
        {
            yield return self;
            if (self.IsArray)
            {
                foreach (var x in self.ArrayItems)
                {
                    foreach (var y in x.Traverse())
                    {
                        yield return y;
                    }
                }
            }
            else if (self.IsMap)
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

        #region Deserializer
        static Delegate GetDeserializer<S, T>() where S : IValueNode
        {
            var m = typeof(S).GetMethods();

            // primitive
            var mi = typeof(S).GetMethods().FirstOrDefault(x =>
            {
                if (!x.Name.StartsWith("Get"))
                {
                    return false;
                }
                var t = typeof(T);
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
                return func.Compile();
            }

            throw new NotImplementedException();
        }

        struct GenericDeserializer<S, T> where S : IValueNode
        {
            delegate T Deserializer(S node);

            static Deserializer s_deserializer;

            public void Deserialize(S node, ref T value)
            {
                if (s_deserializer == null)
                {
                    var d = (Func<S, T>)GetDeserializer<S, T>();
                    s_deserializer = new Deserializer(d);
                }
                value = s_deserializer(node);
            }
        }

        public static void Deserialize<S, T>(this S self, ref T value) where S : IValueNode
        {
            (default(GenericDeserializer<S, T>)).Deserialize(self, ref value);
        }

        public static bool GetBoolean<T>(this T self) where T : IValueNode
        {
            var value = default(bool);
            self.Deserialize(ref value);
            return value;
        }
        #endregion
    }
}
