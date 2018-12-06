﻿using System;
using System.Linq;
using System.Text;


namespace UniJSON
{
    public static class JsonString
    {
        #region Quote
        public static void Escape(String s, IStore w)
        {
            if (String.IsNullOrEmpty(s))
            {
                return;
            }

            var it = s.ToCharArray().Cast<char>().GetEnumerator();
            while (it.MoveNext())
            {
                switch (it.Current)
                {
                    case '"':
                    case '\\':
                    case '/':
                        // \\ prefix
                        w.Write('\\');
                        w.Write(it.Current);
                        break;

                    case '\b':
                        w.Write('\\');
                        w.Write('b');
                        break;
                    case '\f':
                        w.Write('\\');
                        w.Write('f');
                        break;
                    case '\n':
                        w.Write('\\');
                        w.Write('n');
                        break;
                    case '\r':
                        w.Write('\\');
                        w.Write('r');
                        break;
                    case '\t':
                        w.Write('\\');
                        w.Write('t');
                        break;

                    default:
                        w.Write(it.Current);
                        break;
                }
            }
        }

        public static void Escape(Utf8String s, IStore w)
        {
            if (s.IsEmpty)
            {
                return;
            }

            var it = s.GetIterator();
            while(it.MoveNext())
            {
                var b = it.Current;
                if (b <= 0x7F)
                {
                    switch (b)
                    {
                        case (Byte)'"':
                        case (Byte)'\\':
                        case (Byte)'/':
                            // \\ prefix
                            w.Write((Byte)'\\');
                            w.Write(b);
                            break;

                        case (Byte)'\b':
                            w.Write((Byte)'\\');
                            w.Write((Byte)'b');
                            break;
                        case (Byte)'\f':
                            w.Write((Byte)'\\');
                            w.Write((Byte)'f');
                            break;
                        case (Byte)'\n':
                            w.Write((Byte)'\\');
                            w.Write((Byte)'n');
                            break;
                        case (Byte)'\r':
                            w.Write((Byte)'\\');
                            w.Write((Byte)'r');
                            break;
                        case (Byte)'\t':
                            w.Write((Byte)'\\');
                            w.Write((Byte)'t');
                            break;

                        default:
                            w.Write(b);
                            break;
                    }
                    // ascii
                }
                else if (b <= 0xDF)
                {
                    w.Write(b);
                    w.Write(it.Second);
                }
                else if (b <= 0xEF)
                {
                    w.Write(b);
                    w.Write(it.Second);
                    w.Write(it.Third);
                }
                else if (b <= 0xF7)
                {
                    w.Write(b);
                    w.Write(it.Second);
                    w.Write(it.Third);
                    w.Write(it.Fourth);
                }
                else
                {
                    throw new JsonParseException("invalid utf8");
                }
            }
        }

        public static string Escape(String s)
        {
            var sb = new StringBuilder();
            Escape(s, new StringBuilderStore(sb));
            return sb.ToString();
        }

        public static void Quote(String s, IStore w)
        {
            w.Write('"');
            Escape(s, w);
            w.Write('"');
        }

        public static void Quote(Utf8String s, IStore w)
        {
            w.Write((Byte)'"');
            Escape(s, w);
            w.Write((Byte)'"');
        }

        /// <summary>
        /// Added " and Escape
        /// </summary>
        /// <param name="s"></param>
        /// <param name="w"></param>
        public static string Quote(string s)
        {
            var sb = new StringBuilder();
            Quote(s, new StringBuilderStore(sb));
            return sb.ToString();
        }

        public static Utf8String Quote(Utf8String s)
        {
            var sb = new BytesStore(s.ByteLength);
            Quote(s, sb);
            return new Utf8String(sb.Bytes);
        }
        #endregion

        #region Unquote
        public static int Unescape(string src, IStore w)
        {
            int writeCount = 0;
            Action<Char> Write = c =>
            {
                if (w != null)
                {
                    w.Write(c);
                }
                ++writeCount;
            };

            int i = 0;
            int length = src.Length - 1;
            while (i < length)
            {
                if (src[i] == '\\')
                {
                    var c = src[i + 1];
                    switch (c)
                    {
                        case '\\':
                        case '/':
                        case '"':
                            // remove prefix
                            Write(c);
                            i += 2;
                            continue;

                        case 'b':
                            Write('\b');
                            i += 2;
                            continue;
                        case 'f':
                            Write('\f');
                            i += 2;
                            continue;
                        case 'n':
                            Write('\n');
                            i += 2;
                            continue;
                        case 'r':
                            Write('\r');
                            i += 2;
                            continue;
                        case 't':
                            Write('\t');
                            i += 2;
                            continue;
                    }
                }

                Write(src[i]);
                i += 1;
            }
            while (i <= length)
            {
                Write(src[i++]);
            }

            return writeCount;
        }

        public static int Unescape(Utf8String s, IStore w)
        {
            int writeCount = 0;
            Action<Byte> Write = c =>
            {
                if (w != null)
                {
                    w.Write(c);
                }
                ++writeCount;
            };

            var it = s.GetIterator();
            while(it.MoveNext())
            {
                var b = it.Current;
                if (b <= 0x7F)
                {
                    if (b == (Byte)'\\')
                    {
                        var c = it.Second;
                        switch (c)
                        {
                            case (Byte)'\\':
                            case (Byte)'/':
                            case (Byte)'"':
                                // remove prefix
                                Write(c);
                                it.MoveNext();
                                continue;

                            case (Byte)'b':
                                Write((Byte)'\b');
                                it.MoveNext();
                                continue;
                            case (Byte)'f':
                                Write((Byte)'\f');
                                it.MoveNext();
                                continue;
                            case (Byte)'n':
                                Write((Byte)'\n');
                                it.MoveNext();
                                continue;
                            case (Byte)'r':
                                Write((Byte)'\r');
                                it.MoveNext();
                                continue;
                            case (Byte)'t':
                                Write((Byte)'\t');
                                it.MoveNext();
                                continue;
                        }
                    }

                    Write(b);
                }
                else if (b <= 0xDF)
                {
                    Write(b);
                    Write(it.Second);
                }
                else if (b <= 0xEF)
                {
                    Write(b);
                    Write(it.Second);
                    Write(it.Third);
                }
                else if (b <= 0xF7)
                {
                    Write(b);
                    Write(it.Second);
                    Write(it.Third);
                    Write(it.Fourth);
                }
                else
                {
                    throw new JsonParseException("invalid utf8");
                }
            }

            return writeCount;
        }

        public static string Unescape(string src)
        {
            var sb = new StringBuilder();
            Unescape(src, new StringBuilderStore(sb));
            return sb.ToString();
        }

        public static int Unquote(string src, IStore w)
        {
            return Unescape(src.Substring(1, src.Length - 2), w);
        }

        public static int Unquote(Utf8String src, IStore w)
        {
            return Unescape(src.Subbytes(1, src.ByteLength - 2), w);
        }

        public static string Unquote(string src)
        {
            var count = Unquote(src, null);
            if (count == src.Length - 2)
            {
                return src.Substring(1, src.Length - 2);
            }
            else
            {
                var sb = new StringBuilder(count);
                Unquote(src, new StringBuilderStore(sb));
                var str = sb.ToString();
                return str;
            }
        }

        public static Utf8String Unquote(Utf8String src)
        {
            var count = Unquote(src, null);
            if (count == src.ByteLength - 2)
            {
                return src.Subbytes(1, src.ByteLength - 2);
            }
            else
            {
                var sb = new BytesStore(count);
                Unquote(src, sb);
                return new Utf8String(sb.Bytes);
            }
        }
        #endregion
    }
}
