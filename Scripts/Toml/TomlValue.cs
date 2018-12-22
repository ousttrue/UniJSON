using System;


namespace UniJSON
{
    public struct TomlValue : ITreeItem, IValue<TomlValue>
    {
        public override string ToString()
        {
            return m_segment.ToString();
        }

        public int ParentIndex { get; private set; }

        public ValueNodeType ValueType { get; private set; }

        Utf8String m_segment;
        public ArraySegment<byte> Bytes { get { return m_segment.Bytes; } }

        public TomlValue(Utf8String segment, ValueNodeType valueType, int parentIndex)
        {
            ParentIndex = parentIndex;
            ValueType = valueType;
            m_segment = segment;
        }

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
            return m_segment.ToInt32();
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
            return m_segment;
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
