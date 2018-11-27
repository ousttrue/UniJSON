﻿using System;


namespace UniJSON
{
    public interface IFormatter
    {
        IStore GetStore();

        void BeginList(int n);

        void EndList();

        void BeginMap(int n);

        void EndMap();

        void Key(string x);


        void Null();

        void Value(String x);

        void Value(ArraySegment<Byte> bytes);

        void Value(Boolean x);

        void Value(Byte x);
        void Value(UInt16 x);
        void Value(UInt32 x);
        void Value(UInt64 x);

        void Value(SByte x);
        void Value(Int16 x);
        void Value(Int32 x);
        void Value(Int64 x);

        void Value(Single x);
        void Value(Double x);

        void Serialize<T>(T value);
    }
}
