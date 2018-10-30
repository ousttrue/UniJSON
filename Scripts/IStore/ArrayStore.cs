using System;
using System.Collections.Generic;
using System.Text;


namespace UniJSON
{
    public class ArrayStore : IStore
    {
        Byte[] m_bytes;
        int m_used;

        public ArrayStore() : this(new Byte[64])
        { }

        public ArrayStore(int size) : this(new Byte[size])
        { }

        public ArrayStore(Byte[] bytes)
        {
            m_bytes = bytes;
        }

        public ArraySegment<Byte> Bytes
        {
            get
            {
                return new ArraySegment<Byte>(m_bytes, 0, m_used);
            }
        }

        public void Clear()
        {
            m_used = 0;
        }

        void Ensure(int size)
        {
            if (m_used + size >= m_bytes.Length)
            {
                var tmp = m_bytes;
                m_bytes = new Byte[m_used + size];
                Array.Copy(tmp, m_bytes, m_used);
            }
        }

        public void Write(ArraySegment<byte> bytes)
        {
            Ensure(bytes.Count);
            Array.Copy(bytes.Array, bytes.Offset,
            m_bytes, m_used, bytes.Count);
            m_used += bytes.Count;
        }

        public void Write(params byte[] bytes)
        {
            Write(new ArraySegment<byte>(bytes));
        }

        public void Write(byte value)
        {
            Ensure(1);
            m_bytes[m_used] = value;
            m_used += 1;
        }

        public void Write(sbyte value)
        {
            Write((byte)value);
        }

        public void Write(IEnumerable<char> src)
        {
            throw new NotImplementedException();
        }

        public void Write(Char c)
        {
            throw new NotImplementedException();
        }

        public void Write(string src)
        {
            throw new NotImplementedException();
        }

        #region BigEndian
        public void WriteBigEndian(int value)
        {
            throw new NotImplementedException();
        }

        public void WriteBigEndian(float value)
        {
            throw new NotImplementedException();
        }

        public void WriteBigEndian(double value)
        {
            throw new NotImplementedException();
        }

        public void WriteBigEndian(long value)
        {
            throw new NotImplementedException();
        }

        public void WriteBigEndian(ulong value)
        {
            throw new NotImplementedException();
        }

        public void WriteBigEndian(short value)
        {
            throw new NotImplementedException();
        }

        public void WriteBigEndian(uint value)
        {
            throw new NotImplementedException();
        }

        public void WriteBigEndian(ushort value)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region LittleEndian
        public void WriteLittleEndian(double value)
        {
            throw new NotImplementedException();
        }

        public void WriteLittleEndian(short value)
        {
            throw new NotImplementedException();
        }

        public void WriteLittleEndian(int value)
        {
            throw new NotImplementedException();
        }

        public void WriteLittleEndian(float value)
        {
            throw new NotImplementedException();
        }

        public void WriteLittleEndian(long value)
        {
            throw new NotImplementedException();
        }

        public void WriteLittleEndian(ulong value)
        {
            throw new NotImplementedException();
        }

        public void WriteLittleEndian(uint value)
        {
            throw new NotImplementedException();
        }

        public void WriteLittleEndian(ushort value)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
