using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace UniJSON
{
    public struct TomlValue : ITreeItem, IValue<TomlValue>
    {
        public int ParentIndex => throw new NotImplementedException();

        public ValueNodeType ValueType => throw new NotImplementedException();

        public ArraySegment<byte> Bytes => throw new NotImplementedException();

        public bool GetBoolean()
        {
            throw new NotImplementedException();
        }

        public byte GetByte()
        {
            throw new NotImplementedException();
        }

        public double GetDouble()
        {
            throw new NotImplementedException();
        }

        public short GetInt16()
        {
            throw new NotImplementedException();
        }

        public int GetInt32()
        {
            throw new NotImplementedException();
        }

        public long GetInt64()
        {
            throw new NotImplementedException();
        }

        public sbyte GetSByte()
        {
            throw new NotImplementedException();
        }

        public float GetSingle()
        {
            throw new NotImplementedException();
        }

        public string GetString()
        {
            throw new NotImplementedException();
        }

        public ushort GetUInt16()
        {
            throw new NotImplementedException();
        }

        public uint GetUInt32()
        {
            throw new NotImplementedException();
        }

        public ulong GetUInt64()
        {
            throw new NotImplementedException();
        }

        public Utf8String GetUtf8String()
        {
            throw new NotImplementedException();
        }

        public U GetValue<U>()
        {
            throw new NotImplementedException();
        }

        public TomlValue Key(Utf8String key, int parentIndex)
        {
            throw new NotImplementedException();
        }

        public TomlValue New(ArraySegment<byte> bytes, ValueNodeType valueType, int parentIndex)
        {
            throw new NotImplementedException();
        }
    }
}
