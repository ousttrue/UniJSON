using System;
using System.Collections.Generic;


namespace UniJSON
{
    public interface IValueNode<T> where T : IValueNode<T>
    {
        bool IsValid { get; }

        ArraySegment<Byte> Bytes { get; }

        ValueNodeType ValueType { get; }

        bool HasParent { get; }
        T Parent { get; }
        IEnumerable<T> Children { get; }

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
        object GetValue();

        void AddKey(Utf8String key);
        void AddValue(ArraySegment<Byte> bytes, ValueNodeType valueType);
    }

    public static class IValueNodeExtensions
    {
        #region ValueType
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
            return self.ValueType == ValueNodeType.Number;
        }

        public static bool IsArray<T>(this T self) where T : IValueNode<T>
        {
            return self.ValueType == ValueNodeType.Array;
        }

        public static bool IsMap<T>(this T self) where T : IValueNode<T>
        {
            return self.ValueType == ValueNodeType.Object;
        }
        #endregion

        public static IEnumerable<T> Traverse<T>(this T self) where T : IValueNode<T>
        {
            yield return self;
            if (self.IsArray())
            {
                foreach (var x in self.ArrayItems())
                {
                    foreach (var y in x.Traverse())
                    {
                        yield return y;
                    }
                }
            }
            else if (self.IsMap())
            {
                foreach (var kv in self.ObjectItems())
                {
                    foreach (var y in kv.Value.Traverse())
                    {
                        yield return y;
                    }
                }
            }
        }
    }
}
