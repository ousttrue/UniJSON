using System;
using System.Collections.Generic;


namespace UniJson
{
    public enum JsonValueType
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

    public struct JsonValue
    {
        public StringSegment Segment;
        public JsonValueType ValueType;

        public Int32 GetInt32()
        {
            return Int32.Parse(Segment.ToString());
        }
    }

    public class JsonParseException : Exception
    {
        public JsonParseException(string msg) : base(msg) { }
    }

    public class JsonParseResult
    {
        public List<JsonValue> Values = new List<JsonValue>();
    }

    public static class JsonParser
    {
        static JsonValueType GetValueType(char c)
        {
            switch (c)
            {
                case '{': return JsonValueType.Object;
                case '[': return JsonValueType.Array;
                case '"': return JsonValueType.String;
                case 't': return JsonValueType.Boolean;
                case 'f': return JsonValueType.Boolean;
                case 'n': return JsonValueType.Unknown;

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
                    return JsonValueType.Number;

                default:
                    throw new JsonParseException(c + " is not valid json start");
            }
        }

        static JsonValue ParsePrimitive(StringSegment segment, JsonValueType valueType)
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
            return new JsonValue
            {
                Segment = segment.Take(i),
                ValueType = valueType,
            };
        }

        static JsonValue ParseString(StringSegment segment)
        {
            int i = 1;
            for (; i < segment.Count; ++i)
            {
                if (segment[i] == '\"')
                {
                    return new JsonValue
                    {
                        Segment = segment.Take(i + 1),
                        ValueType = JsonValueType.String,
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
                            throw new JsonParseException("unknown escape: " + segment.Skip(i));
                    }
                }
            }
            throw new JsonParseException("no close string: " + segment.Skip(i));
        }

        static JsonValue ParseArray(StringSegment segment, List<JsonValue> values)
        {
            var index = values.Count;
            values.Add(new JsonValue()); // placeholder

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
                        throw new JsonParseException("no white space expected");
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
                        throw new JsonParseException("',' expected");
                    }
                    current = current.Skip(keyPos + 1);
                }

                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearch(x => !Char.IsWhiteSpace(x), out nextToken))
                    {
                        throw new JsonParseException("not whitespace expected");
                    }
                    current = current.Skip(nextToken);
                }

                // value
                var value = Parse(current, values);
                current = current.Skip(value.Segment.Count);
                values.Add(value);
            }

            var array = values[index].Segment;
            values[index] = new JsonValue
            {
                Segment = new StringSegment(array.Value, array.Offset, current.Offset - array.Offset),
                ValueType = JsonValueType.Array
            };

            return values[index];
        }

        static JsonValue ParseObject(StringSegment segment, List<JsonValue> values)
        {
            var index = values.Count;
            values.Add(new JsonValue()); // placeholder

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
                        throw new JsonParseException("no white space expected");
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
                        throw new JsonParseException("',' expected");
                    }
                    current = current.Skip(keyPos + 1);
                }

                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearch(x => !Char.IsWhiteSpace(x), out nextToken))
                    {
                        throw new JsonParseException("not whitespace expected");
                    }
                    current = current.Skip(nextToken);
                }

                // key
                var key = Parse(current, values);
                if (key.ValueType != JsonValueType.String)
                {
                    throw new JsonParseException("object key must string: " + key.Segment);
                }
                current = current.Skip(key.Segment.Count);
                values.Add(key);

                // search ':'
                int valuePos;
                if (!current.TrySearch(x => x == ':', out valuePos))
                {
                    throw new JsonParseException(": is not found");
                }
                current = current.Skip(valuePos + 1);

                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearch(x => !Char.IsWhiteSpace(x), out nextToken))
                    {
                        throw new JsonParseException("not whitespace expected");
                    }
                    current = current.Skip(nextToken);
                }

                // value
                var value = Parse(current, values);
                current = current.Skip(value.Segment.Count);
                values.Add(value);
            }

            var obj = values[index].Segment;
            values[index] = new JsonValue
            {
                Segment = new StringSegment(obj.Value, obj.Offset, current.Offset - obj.Offset),
                ValueType = JsonValueType.Array
            };

            return values[index];
        }

        public static JsonValue Parse(StringSegment segment, List<JsonValue> values)
        {
            // skip white space
            int pos;
            if (!segment.TrySearch(x => !char.IsWhiteSpace(x), out pos))
            {
                throw new JsonParseException("only whitespace");
            }
            segment = segment.Skip(pos);

            var valueType = GetValueType(segment[0]);
            switch (valueType)
            {
                case JsonValueType.Boolean:
                case JsonValueType.Number:
                case JsonValueType.Null:
                    return ParsePrimitive(segment, valueType);

                case JsonValueType.String:
                    return ParseString(segment);

                case JsonValueType.Array: // fall through
                    return ParseArray(segment, values);

                case JsonValueType.Object: // fall through
                    return ParseObject(segment, values);

                default:
                    throw new NotImplementedException();
            }
        }

        public static List<JsonValue> Parse(String json)
        {
            var result = new List<JsonValue>();
            var value = Parse(new StringSegment(json), result);
            if (value.ValueType != JsonValueType.Array && value.ValueType != JsonValueType.Object)
            {
                result.Add(value);
            }
            return result;
        }
    }
}
