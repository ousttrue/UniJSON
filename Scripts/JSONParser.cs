using System;
using System.Collections.Generic;


namespace UniJSON
{
    public enum JSONValueType
    {
        Unknown,

        Null,
        Boolean,
        Number,

        String,

        Object,
        Array,

        //Close, // internal use
    }

    public struct JSONValue
    {
        public StringSegment Segment;
        public JSONValueType ValueType;

        public Int32 GetInt32()
        {
            return Int32.Parse(Segment.ToString());
        }
    }

    public class JSONParseException : Exception
    {
        public JSONParseException(string msg) : base(msg) { }
    }

    public class JSONParseResult
    {
        public List<JSONValue> Values = new List<JSONValue>();
    }

    public static class JSONParser
    {
        static JSONValueType GetValueType(char c)
        {
            switch (c)
            {
                case '{': return JSONValueType.Object;
                case '[': return JSONValueType.Array;
                case '"': return JSONValueType.String;
                case 't': return JSONValueType.Boolean;
                case 'f': return JSONValueType.Boolean;
                case 'n': return JSONValueType.Unknown;

                case '-': // fall through
                case '0': // fall through
                case '1': // fall through
                case '2': // fall through
                case '3': // fall through
                case '4': // fall through
                case '5': // fall through
                case '6': // fall through
                case '7': // fall through
                case '8': // fall through
                case '9': // fall through
                    return JSONValueType.Number;

                default:
                    throw new JSONParseException(c + " is not valid json start");
            }
        }

        static JSONValue ParsePrimitive(StringSegment segment, JSONValueType valueType)
        {
            int i = 1;
            for (; i < segment.Count; ++i)
            {
                if (Char.IsWhiteSpace(segment[i])
                    || segment[i] == '}'
                    || segment[i] == ']'
                    || segment[i] == ','
                    || segment[i] == ':'
                    )
                {
                    break;
                }
            }
            return new JSONValue
            {
                Segment = segment.Take(i),
                ValueType = valueType,
            };
        }

        static JSONValue ParseString(StringSegment segment)
        {
            int i = 1;
            for (; i < segment.Count; ++i)
            {
                if (segment[i] == '\"')
                {
                    return new JSONValue
                    {
                        Segment = segment.Take(i + 1),
                        ValueType = JSONValueType.String,
                    };
                }
                else if (segment[i] == '\\')
                {
                    switch (segment[i + 1])
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
                            i += 1;
                            break;

                        case 'u': // unicode
                                  // skip next 4
                            i += 4;
                            break;

                        default:
                            // unkonw escape
                            throw new JSONParseException("unknown escape: " + segment.Skip(i));
                    }
                }
            }
            throw new JSONParseException("no close string: " + segment.Skip(i));
        }

        static JSONValue ParseArray(StringSegment segment, List<JSONValue> values)
        {
            var index = values.Count;
            values.Add(new JSONValue()); // placeholder

            var closeChar = ']';
            bool isFirst = true;
            var current = segment.Skip(1);
            while (true)
            {
                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearch(x => !Char.IsWhiteSpace(x), out nextToken))
                    {
                        throw new JSONParseException("no white space expected");
                    }
                    current = current.Skip(nextToken);
                }

                {
                    if (current[0] == closeChar)
                    {
                        // end
                        break;
                    }
                }

                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    // search ',' or closeChar
                    int keyPos;
                    if (!current.TrySearch(x => x == ',', out keyPos))
                    {
                        throw new JSONParseException("',' expected");
                    }
                    current = current.Skip(keyPos + 1);
                }

                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearch(x => !Char.IsWhiteSpace(x), out nextToken))
                    {
                        throw new JSONParseException("not whitespace expected");
                    }
                    current = current.Skip(nextToken);
                }

                // value
                var value = Parse(current, values);
                current = current.Skip(value.Segment.Count);
                values.Add(value);
            }

            var array = values[index].Segment;
            values[index] = new JSONValue
            {
                Segment = new StringSegment(array.Value, array.Offset, current.Offset - array.Offset),
                ValueType = JSONValueType.Array
            };

            return values[index];
        }

        static JSONValue ParseObject(StringSegment segment, List<JSONValue> values)
        {
            var index = values.Count;
            values.Add(new JSONValue()); // placeholder

            var closeChar = '}';
            bool isFirst = true;
            var current = segment.Skip(1);
            while (true)
            {
                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearch(x => !Char.IsWhiteSpace(x), out nextToken))
                    {
                        throw new JSONParseException("no white space expected");
                    }
                    current = current.Skip(nextToken);
                }

                {
                    if (current[0] == closeChar)
                    {
                        break;
                    }
                }

                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    // search ',' or closeChar
                    int keyPos;
                    if (!current.TrySearch(x => x == ',', out keyPos))
                    {
                        throw new JSONParseException("',' expected");
                    }
                    current = current.Skip(keyPos + 1);
                }

                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearch(x => !Char.IsWhiteSpace(x), out nextToken))
                    {
                        throw new JSONParseException("not whitespace expected");
                    }
                    current = current.Skip(nextToken);
                }

                // key
                var key = Parse(current, values);
                if (key.ValueType != JSONValueType.String)
                {
                    throw new JSONParseException("object key must string: " + key.Segment);
                }
                current = current.Skip(key.Segment.Count);
                values.Add(key);

                // search ':'
                int valuePos;
                if (!current.TrySearch(x => x == ':', out valuePos))
                {
                    throw new JSONParseException(": is not found");
                }
                current = current.Skip(valuePos + 1);

                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearch(x => !Char.IsWhiteSpace(x), out nextToken))
                    {
                        throw new JSONParseException("not whitespace expected");
                    }
                    current = current.Skip(nextToken);
                }

                // value
                var value = Parse(current, values);
                current = current.Skip(value.Segment.Count);
                values.Add(value);
            }

            var obj = values[index].Segment;
            values[index] = new JSONValue
            {
                Segment = new StringSegment(obj.Value, obj.Offset, current.Offset - obj.Offset),
                ValueType = JSONValueType.Array
            };

            return values[index];
        }

        public static JSONValue Parse(StringSegment segment, List<JSONValue> values)
        {
            // skip white space
            int pos;
            if (!segment.TrySearch(x => !char.IsWhiteSpace(x), out pos))
            {
                throw new JSONParseException("only whitespace");
            }
            segment = segment.Skip(pos);

            var valueType = GetValueType(segment[0]);
            switch (valueType)
            {
                case JSONValueType.Boolean:
                case JSONValueType.Number:
                case JSONValueType.Null:
                    return ParsePrimitive(segment, valueType);

                case JSONValueType.String:
                    return ParseString(segment);

                case JSONValueType.Array: // fall through
                    return ParseArray(segment, values);

                case JSONValueType.Object: // fall through
                    return ParseObject(segment, values);

                default:
                    throw new NotImplementedException();
            }
        }

        public static List<JSONValue> Parse(String json)
        {
            var result = new List<JSONValue>();
            var value = Parse(new StringSegment(json), result);
            if (value.ValueType != JSONValueType.Array && value.ValueType != JSONValueType.Object)
            {
                result.Add(value);
            }
            return result;
        }
    }
}
