using System;
using System.Collections.Generic;


namespace UniJSON
{
    public static class TomlParser
    {
        static Utf8String s_array_key = Utf8String.From("[[");
        static Utf8String s_table_key = Utf8String.From("[");

        static TomlValue ParseRHS(Utf8String segment, int parentIndex)
        {
            int i = 1;
            for (; i < segment.ByteLength; ++i)
            {
                if (Char.IsWhiteSpace((char)segment[i])
                    || segment[i] == '}'
                    || segment[i] == ']'
                    || segment[i] == ','
                    || segment[i] == ':'
                    )
                {
                    break;
                }
            }
            segment = segment.Subbytes(0, i);

            switch ((char)segment[0])
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    if (segment.IsInt)
                    {
                        return new TomlValue(segment, ValueNodeType.Integer, parentIndex);
                    }
                    else
                    {
                        return new TomlValue(segment, ValueNodeType.Integer, parentIndex);
                    }
            }

            throw new NotImplementedException();
        }

        public static ListTreeNode<TomlValue> Parse(Utf8String segment)
        {
            var values = new List<TomlValue>()
            {
                new TomlValue(segment, ValueNodeType.Object, -1),
            };
            var current = 0;

            while (!segment.IsEmpty)
            {
                var line = segment.GetLine();
                segment = segment.Subbytes(line.ByteLength);

                line = line.TrimStart();
                if (line.IsEmpty)
                {
                    continue;
                }

                if (line.StartsWith(s_array_key))
                {
                    // [[array_name]]
                    throw new NotImplementedException();
                }
                else if (line.StartsWith(s_table_key))
                {
                    // [table_name]
                    int table_end;
                    if (!line.TrySearchByte(x => x == ']', out table_end))
                    {
                        throw new ParserException("] not found");
                    }
                    var table_name = line.Subbytes(1, table_end-1).Trim();
                    if (table_name.IsEmpty)
                    {
                        throw new ParserException("empty table name");
                    }

                    // top level key
                    values.Add(new TomlValue(table_name, ValueNodeType.Object, 0));
                    current = values.Count - 1;
                }
                else
                {
                    // key = value
                    int key_end;
                    if (!line.TrySearchByte(x => x == '=', out key_end))
                    {
                        throw new ParserException("= not found");
                    }
                    var key = line.Subbytes(0, key_end);
                    line = line.Subbytes(key_end + 1);
                    values.Add(new TomlValue(key.Trim(), ValueNodeType.String, current));

                    // skip white space
                    int pos;
                    if (!line.TrySearchByte(x => !char.IsWhiteSpace((char)x), out pos))
                    {
                        break;
                    }
                    line = line.Subbytes(pos);

                    var value = ParseRHS(line, current);
                    values.Add(value);
                }
            }

            return new ListTreeNode<TomlValue>(values);
        }

        public static ListTreeNode<TomlValue> Parse(String Toml)
        {
            return Parse(Utf8String.From(Toml));
        }
    }
}
