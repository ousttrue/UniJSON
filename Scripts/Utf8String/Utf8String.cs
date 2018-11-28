using System;
using System.IO;


namespace UniJSON
{
    public struct Utf8String
    {
        public static readonly System.Text.Encoding Encoding = new System.Text.UTF8Encoding(false);

        public readonly ArraySegment<Byte> Bytes;
        public int ByteLength
        {
            get { return Bytes.Count; }
        }

        public Byte this[int i]
        {
            get { return Bytes.Array[Bytes.Offset + i]; }
        }

        public Utf8String(ArraySegment<Byte> bytes)
        {
            Bytes = bytes;
        }

        public Utf8String(Byte[] bytes, int offset, int count) : this(new ArraySegment<Byte>(bytes, offset, count))
        {
        }

        public Utf8String(Byte[] bytes) : this(bytes, 0, bytes.Length)
        {
        }

        public static Utf8String FromString(string src)
        {
            return new Utf8String(Encoding.GetBytes(src));
        }

        public Utf8String Concat(Utf8String rhs)
        {
            var bytes = new Byte[ByteLength + rhs.ByteLength];
            Buffer.BlockCopy(Bytes.Array, Bytes.Offset, bytes, 0, ByteLength);
            Buffer.BlockCopy(rhs.Bytes.Array, rhs.Bytes.Offset, bytes, ByteLength, rhs.ByteLength);
            return new Utf8String(bytes);
        }

        public override string ToString()
        {
            if (ByteLength == 0) return "";
            return Encoding.GetString(Bytes.Array, Bytes.Offset, Bytes.Count);
        }

        public bool IsEmpty
        {
            get
            {
                return ByteLength == 0;
            }
        }

        public bool StartsWith(Utf8String rhs)
        {
            if (rhs.ByteLength > ByteLength)
            {
                return false;
            }

            for (int i = 0; i < rhs.ByteLength; ++i)
            {
                if (this[i] != rhs[i])
                {
                    return false;
                }
            }

            return true;
        }

        public bool EndsWith(Utf8String rhs)
        {
            if (rhs.ByteLength > ByteLength)
            {
                return false;
            }

            for (int i = 1; i <= rhs.ByteLength; ++i)
            {
                if (this[ByteLength - i] != rhs[rhs.ByteLength - i])
                {
                    return false;
                }
            }

            return true;
        }

        public int IndexOf(Byte code)
        {
            return IndexOf(0, code);
        }

        public int IndexOf(int offset, Byte code)
        {
            var pos = offset + Bytes.Offset;
            for (int i = 0; i < Bytes.Count; ++i, ++pos)
            {
                if (Bytes.Array[pos] == code)
                {
                    return pos - Bytes.Offset;
                }
            }
            return -1;
        }

        public Utf8String SubString(int offset)
        {
            return SubString(offset, ByteLength - offset);
        }

        public Utf8String SubString(int offset, int count)
        {
            return new Utf8String(Bytes.Array, Bytes.Offset + offset, count);
        }

        static bool IsSpace(Byte b)
        {
            switch(b)
            {
                case 0x20:
                case 0x0a:
                case 0x0b:
                case 0x0c:
                case 0x0d:
                case 0x09:
                    return true;
            }

            return false;
        }

        public Utf8String TrimStart()
        {
            var i = 0;
            for(; i<ByteLength; ++i)
            {
                if (!IsSpace(this[i]))
                {
                    break;
                }
            }
            return SubString(i);
        }

        public override bool Equals(Object obj)
        {
            return obj is Utf8String && Equals((Utf8String)obj);
        }

        public static bool operator ==(Utf8String x, Utf8String y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Utf8String x, Utf8String y)
        {
            return !(x == y);
        }

        public bool Equals(Utf8String other)
        {
            if (ByteLength != other.ByteLength)
            {
                return false;
            }

            for (int i = 0; i < ByteLength; ++i)
            {
                if (this[i] != other[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return ByteLength.GetHashCode();
        }

        public static Utf8String operator +(Utf8String l, Utf8String r)
        {
            return new Utf8String(l.Bytes.Concat(r.Bytes));
        }

        public bool IsInt
        {
            get
            {
                bool isInt = false;
                for (int i = 0; i < ByteLength; ++i)
                {
                    var c = this[i];
                    if (c == '0'
                        || c == '1'
                        || c == '2'
                        || c == '3'
                        || c == '4'
                        || c == '5'
                        || c == '6'
                        || c == '7'
                        || c == '8'
                        || c == '9'
                        )
                    {
                        // ok
                        isInt = true;
                    }
                    else if (i == 0 && c == '-')
                    {
                        // ok
                    }
                    else if (c == '.' || c == 'e')
                    {
                        return false;
                    }
                    else
                    {
                        return isInt;
                    }
                }
                return true;
            }
        }

        public bool TrySearch(Func<byte, bool> pred, out int pos)
        {
            pos = 0;
            for (; pos < ByteLength; ++pos)
            {
                if (pred(this[pos]))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static class Utf8StringExtensions
    {
        public static void WriteTo(this Utf8String src, Stream dst)
        {
            dst.Write(src.Bytes.Array, src.Bytes.Offset, src.Bytes.Count);
        }
    }
}
