﻿using System;


namespace UniJSON
{
    public interface ITreeItem
    {
        int ParentIndex { get; }
    }

    public enum ValueNodeType
    {
        Null,
        Boolean,
        String,
        Binary,
        Integer,
        Number,
        Array,
        Object,
    }

    public interface IValue<T> where T : struct
    {
        T New(ArraySegment<byte> bytes, ValueNodeType valueType, int parentIndex);
        T Key(Utf8String key, int parentIndex);
        ValueNodeType ValueType { get; }
        ArraySegment<Byte> Bytes { get; }
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
        U GetValue<U>();
    }
}
