using System;
using System.Collections.Generic;
using System.IO;


namespace UniJSON
{
    public struct Utf8String
    {
        public static readonly System.Text.Encoding Encoding = new System.Text.UTF8Encoding(false);

        public struct CodePoint
        {
            readonly Utf8String m_all;

            public int Position
            {
                get;
                private set;
            }

            int GetLength()
            {
                var b = m_all[Position];
                if (b <= 0x7F)
                {
                    return 1;
                }
                else if (b <= 0xDF)
                {
                    return 2;
                }
                else if (b <= 0xEF)
                {
                    return 3;
                }
                else if (b <= 0xF7)
                {
                    return 4;
                }
                else
                {
                    throw new Exception("invalid utf8");
                }
            }

            public Utf8String Current
            {
                get { return new Utf8String(m_all.Bytes.Array, m_all.Bytes.Offset + Position, GetLength()); }
            }

            public CodePoint(Utf8String all, int pos = 0)
            {
                m_all = all;
                Position = pos;
            }

            public void Next()
            {
                Position += GetLength();
            }

            public bool IsValid
            {
                get
                {
                    return Position < m_all.ByteLength;
                }
            }
        }

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

        public string ToAscii()
        {
            if (ByteLength == 0) return "";
            return System.Text.Encoding.ASCII.GetString(Bytes.Array, Bytes.Offset, Bytes.Count);
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

        public Utf8String Subbytes(int offset)
        {
            return Subbytes(offset, ByteLength - offset);
        }

        public Utf8String Subbytes(int offset, int count)
        {
            return new Utf8String(Bytes.Array, Bytes.Offset + offset, count);
        }

        static bool IsSpace(Byte b)
        {
            switch (b)
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
            for (; i < ByteLength; ++i)
            {
                if (!IsSpace(this[i]))
                {
                    break;
                }
            }
            return Subbytes(i);
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

        public bool TrySearchByte(Func<byte, bool> pred, out int pos)
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

        public bool TrySearchAscii(Byte target, int start, out int pos)
        {
            for(var p = new CodePoint(this, start); p.IsValid; p.Next())
            {
                var b = p.Current[0];
                if (b <= 0x7F)
                {
                    // ascii
                    if (b == target/*'\"'*/)
                    {
                        // closed
                        pos = p.Position;
                        return true;
                    }
                    else if (b == '\\')
                    {
                        // escaped
                        var next = p;
                        next.Next();

                        switch ((char)next.Current[0])
                        {
                            case '"': // fall through
                            case '\\': // fall through
                            case '/': // fall through
                            case 'b': // fall through
                            case 'f': // fall through
                            case 'n': // fall through
                            case 'r': // fall through
                            case 't': // fall through
                                      // skip next
                                p.Next();
                                break;

                            case 'u': // unicode
                                      // skip next 4
                                p.Next();
                                p.Next();
                                p.Next();
                                p.Next();
                                break;

                            default:
                                // unkonw escape
                                throw new JsonParseException("unknown escape: " + next);
                        }
                    }
                }
            }

            pos = -1;
            return false;
        }

        public IEnumerable<Utf8String> Split(byte delemeter)
        {
            var start = 0;
            var p = new CodePoint(this, start);
            for (; p.IsValid; p.Next())
            {
                if (p.Current[0] == delemeter)
                {
                    if (p.Position - start == 0)
                    {
                        yield return default(Utf8String);
                    }
                    else
                    {
                        yield return Subbytes(start, p.Position - start);
                    }
                    start = p.Position+1;
                }
            }

            if(start < p.Position)
            {
                yield return Subbytes(start, p.Position - start);
            }
        }

        #region atoi
        public SByte ToSByte()
        {
            SByte value = 0;
            var p = new CodePoint(this);
            for (; p.IsValid; p.Next())
            {
                var b = p.Current[0];
                switch (b)
                {
                    case 0x30: value = (SByte)(value * 10); break;
                    case 0x31: value = (SByte)(value * 10 + 1); break;
                    case 0x32: value = (SByte)(value * 10 + 2); break;
                    case 0x33: value = (SByte)(value * 10 + 3); break;
                    case 0x34: value = (SByte)(value * 10 + 4); break;
                    case 0x35: value = (SByte)(value * 10 + 5); break;
                    case 0x36: value = (SByte)(value * 10 + 6); break;
                    case 0x37: value = (SByte)(value * 10 + 7); break;
                    case 0x38: value = (SByte)(value * 10 + 8); break;
                    case 0x39: value = (SByte)(value * 10 + 9); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            return value;
        }
        public Int16 ToInt16()
        {
            Int16 value = 0;
            var p = new CodePoint(this);
            for (; p.IsValid; p.Next())
            {
                var b = p.Current[0];
                switch (b)
                {
                    case 0x30: value = (Int16)(value * 10); break;
                    case 0x31: value = (Int16)(value * 10 + 1); break;
                    case 0x32: value = (Int16)(value * 10 + 2); break;
                    case 0x33: value = (Int16)(value * 10 + 3); break;
                    case 0x34: value = (Int16)(value * 10 + 4); break;
                    case 0x35: value = (Int16)(value * 10 + 5); break;
                    case 0x36: value = (Int16)(value * 10 + 6); break;
                    case 0x37: value = (Int16)(value * 10 + 7); break;
                    case 0x38: value = (Int16)(value * 10 + 8); break;
                    case 0x39: value = (Int16)(value * 10 + 9); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            return value;
        }
        public Int32 ToInt32()
        {
            Int32 value = 0;
            var p = new CodePoint(this);
            for (; p.IsValid; p.Next())
            {
                var b = p.Current[0];
                switch (b)
                {
                    case 0x30: value = value * 10; break;
                    case 0x31: value = value * 10 + 1; break;
                    case 0x32: value = value * 10 + 2; break;
                    case 0x33: value = value * 10 + 3; break;
                    case 0x34: value = value * 10 + 4; break;
                    case 0x35: value = value * 10 + 5; break;
                    case 0x36: value = value * 10 + 6; break;
                    case 0x37: value = value * 10 + 7; break;
                    case 0x38: value = value * 10 + 8; break;
                    case 0x39: value = value * 10 + 9; break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            return value;
        }
        public Int64 ToInt64()
        {
            Int64 value = 0;
            var p = new CodePoint(this);
            for (; p.IsValid; p.Next())
            {
                var b = p.Current[0];
                switch (b)
                {
                    case 0x30: value = (Int64)(value * 10); break;
                    case 0x31: value = (Int64)(value * 10 + 1); break;
                    case 0x32: value = (Int64)(value * 10 + 2); break;
                    case 0x33: value = (Int64)(value * 10 + 3); break;
                    case 0x34: value = (Int64)(value * 10 + 4); break;
                    case 0x35: value = (Int64)(value * 10 + 5); break;
                    case 0x36: value = (Int64)(value * 10 + 6); break;
                    case 0x37: value = (Int64)(value * 10 + 7); break;
                    case 0x38: value = (Int64)(value * 10 + 8); break;
                    case 0x39: value = (Int64)(value * 10 + 9); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            return value;
        }
        public Byte ToByte()
        {
            Byte value = 0;
            var p = new CodePoint(this);
            for (; p.IsValid; p.Next())
            {
                var b = p.Current[0];
                switch (b)
                {
                    case 0x30: value = (Byte)(value * 10); break;
                    case 0x31: value = (Byte)(value * 10 + 1); break;
                    case 0x32: value = (Byte)(value * 10 + 2); break;
                    case 0x33: value = (Byte)(value * 10 + 3); break;
                    case 0x34: value = (Byte)(value * 10 + 4); break;
                    case 0x35: value = (Byte)(value * 10 + 5); break;
                    case 0x36: value = (Byte)(value * 10 + 6); break;
                    case 0x37: value = (Byte)(value * 10 + 7); break;
                    case 0x38: value = (Byte)(value * 10 + 8); break;
                    case 0x39: value = (Byte)(value * 10 + 9); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            return value;
        }
        public UInt16 ToUInt16()
        {
            UInt16 value = 0;
            var p = new CodePoint(this);
            for (; p.IsValid; p.Next())
            {
                var b = p.Current[0];
                switch (b)
                {
                    case 0x30: value = (UInt16)(value * 10); break;
                    case 0x31: value = (UInt16)(value * 10 + 1); break;
                    case 0x32: value = (UInt16)(value * 10 + 2); break;
                    case 0x33: value = (UInt16)(value * 10 + 3); break;
                    case 0x34: value = (UInt16)(value * 10 + 4); break;
                    case 0x35: value = (UInt16)(value * 10 + 5); break;
                    case 0x36: value = (UInt16)(value * 10 + 6); break;
                    case 0x37: value = (UInt16)(value * 10 + 7); break;
                    case 0x38: value = (UInt16)(value * 10 + 8); break;
                    case 0x39: value = (UInt16)(value * 10 + 9); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            return value;
        }
        public UInt32 ToUInt32()
        {
            UInt32 value = 0;
            var p = new CodePoint(this);
            for (; p.IsValid; p.Next())
            {
                var b = p.Current[0];
                switch (b)
                {
                    case 0x30: value = (UInt32)(value * 10); break;
                    case 0x31: value = (UInt32)(value * 10 + 1); break;
                    case 0x32: value = (UInt32)(value * 10 + 2); break;
                    case 0x33: value = (UInt32)(value * 10 + 3); break;
                    case 0x34: value = (UInt32)(value * 10 + 4); break;
                    case 0x35: value = (UInt32)(value * 10 + 5); break;
                    case 0x36: value = (UInt32)(value * 10 + 6); break;
                    case 0x37: value = (UInt32)(value * 10 + 7); break;
                    case 0x38: value = (UInt32)(value * 10 + 8); break;
                    case 0x39: value = (UInt32)(value * 10 + 9); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            return value;
        }
        public UInt64 ToUInt64()
        {
            UInt64 value = 0;
            var p = new CodePoint(this);
            for (; p.IsValid; p.Next())
            {
                var b = p.Current[0];
                switch (b)
                {
                    case 0x30: value = (UInt64)(value * 10); break;
                    case 0x31: value = (UInt64)(value * 10 + 1); break;
                    case 0x32: value = (UInt64)(value * 10 + 2); break;
                    case 0x33: value = (UInt64)(value * 10 + 3); break;
                    case 0x34: value = (UInt64)(value * 10 + 4); break;
                    case 0x35: value = (UInt64)(value * 10 + 5); break;
                    case 0x36: value = (UInt64)(value * 10 + 6); break;
                    case 0x37: value = (UInt64)(value * 10 + 7); break;
                    case 0x38: value = (UInt64)(value * 10 + 8); break;
                    case 0x39: value = (UInt64)(value * 10 + 9); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            return value;
        }
        #endregion

        public float ToSingle()
        {
            return Single.Parse(ToAscii(), System.Globalization.CultureInfo.InvariantCulture);
        }
        public double ToDouble()
        {
            return Double.Parse(ToAscii(), System.Globalization.CultureInfo.InvariantCulture);
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
