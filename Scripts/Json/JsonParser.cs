using System;
using System.Collections.Generic;


namespace UniJSON
{
    public class JsonParseResult
    {
        public List<JsonValue> Values = new List<JsonValue>();
    }

    public static class JsonParser
    {
        static JsonValueType GetValueType(Utf8String segment)
        {
            switch ((char)segment[0])
            {
                case '{': return JsonValueType.Object;
                case '[': return JsonValueType.Array;
                case '"': return JsonValueType.String;
                case 't': return JsonValueType.Boolean;
                case 'f': return JsonValueType.Boolean;
                case 'n': return JsonValueType.Null;

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
                    {
                        if (segment.IsInt)
                        {
                            return JsonValueType.Integer;
                        }
                        else
                        {
                            return JsonValueType.Number;
                        }
                    }

                default:
                    throw new JsonParseException(segment + " is not valid json start");
            }
        }

        /// <summary>
        /// Expected null, boolean, integer, number
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="valueType"></param>
        /// <param name="parentIndex"></param>
        /// <returns></returns>
        static JsonValue ParsePrimitive(Utf8String segment, JsonValueType valueType, int parentIndex)
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
            return new JsonValue(segment.SubString(0, i), valueType, parentIndex);
        }

        static JsonValue ParseString(Utf8String segment, int parentIndex)
        {
            int i = 1;
            while(i < segment.ByteLength)
            {
                var b = segment[i];
                if (b <= 0x7F)
                {
                    // ascii
                    if (b == '\"')
                    {
                        // closed
                        return new JsonValue(segment.SubString(0, i + 1), JsonValueType.String, parentIndex);
                    }
                    else if (b == '\\')
                    {
                        // escaped
                        switch ((char)segment[i + 1])
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
                                throw new JsonParseException("unknown escape: " + segment.SubString(i));
                        }
                    }

                    ++i;
                }
                else if(b <= 0xDF)
                {
                    i += 2;
                }
                else if(b <= 0xEF)
                {
                    i += 3;
                }
                else if(b <= 0xF7)
                {
                    i += 4;
                }
                else
                {
                    throw new JsonParseException("invalid utf8");
                }
            }
            throw new JsonParseException("no close string: " + segment.SubString(i));
        }

        static Utf8String ParseArray(Utf8String segment, List<JsonValue> values, int parentIndex)
        {
            var closeChar = ']';
            bool isFirst = true;
            var current = segment.SubString(1);
            while (true)
            {
                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearch(x => !Char.IsWhiteSpace((char)x), out nextToken))
                    {
                        throw new JsonParseException("no white space expected");
                    }
                    current = current.SubString(nextToken);
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
                    current = current.SubString(keyPos + 1);
                }

                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearch(x => !Char.IsWhiteSpace((char)x), out nextToken))
                    {
                        throw new JsonParseException("not whitespace expected");
                    }
                    current = current.SubString(nextToken);
                }

                // value
                var value = Parse(current, values, parentIndex);
                current = current.SubString(value.Segment.ByteLength);
            }

            return current;
        }

        static Utf8String ParseObject(Utf8String segment, List<JsonValue> values, int parentIndex)
        {
            var closeChar = '}';
            bool isFirst = true;
            var current = segment.SubString(1);
            while (true)
            {
                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearch(x => !Char.IsWhiteSpace((char)x), out nextToken))
                    {
                        throw new JsonParseException("no white space expected");
                    }
                    current = current.SubString(nextToken);
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
                    current = current.SubString(keyPos + 1);
                }

                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearch(x => !Char.IsWhiteSpace((char)x), out nextToken))
                    {
                        throw new JsonParseException("not whitespace expected");
                    }
                    current = current.SubString(nextToken);
                }

                // key
                var key = Parse(current, values, parentIndex);
                if (key.ValueType != JsonValueType.String)
                {
                    throw new JsonParseException("object key must string: " + key.Segment);
                }
                current = current.SubString(key.Segment.ByteLength);

                // search ':'
                int valuePos;
                if (!current.TrySearch(x => x == ':', out valuePos))
                {
                    throw new JsonParseException(": is not found");
                }
                current = current.SubString(valuePos + 1);

                {
                    // skip white space
                    int nextToken;
                    if (!current.TrySearch(x => !Char.IsWhiteSpace((char)x), out nextToken))
                    {
                        throw new JsonParseException("not whitespace expected");
                    }
                    current = current.SubString(nextToken);
                }

                // value
                var value = Parse(current, values, parentIndex);
                current = current.SubString(value.Segment.ByteLength);
            }

            return current;
        }

        static JsonValue Parse(Utf8String segment, List<JsonValue> values, int parentIndex)
        {
            // skip white space
            int pos;
            if (!segment.TrySearch(x => !char.IsWhiteSpace((char)x), out pos))
            {
                throw new JsonParseException("only whitespace");
            }
            segment = segment.SubString(pos);

            var valueType = GetValueType(segment);
            switch (valueType)
            {
                case JsonValueType.Boolean:
                case JsonValueType.Integer:
                case JsonValueType.Number:
                case JsonValueType.Null:
                    {
                        var value= ParsePrimitive(segment, valueType, parentIndex);
                        values.Add(value);
                        return value;
                    }

                case JsonValueType.String:
                    {
                        var value= ParseString(segment, parentIndex);
                        values.Add(value);
                        return value;
                    }

                case JsonValueType.Array: // fall through
                    {
                        var index = values.Count;
                        values.Add(new JsonValue()); // placeholder
                        var current = ParseArray(segment, values, index);
                        values[index] = new JsonValue(segment.SubString(0, current.Bytes.Offset + 1 - segment.Bytes.Offset),
                            JsonValueType.Array, parentIndex);
                        return values[index];
                    }

                case JsonValueType.Object: // fall through
                    {
                        var index = values.Count;
                        values.Add(new JsonValue()); // placeholder
                        var current=ParseObject(segment, values, index);
                        values[index] = new JsonValue(segment.SubString(0, current.Bytes.Offset + 1 - segment.Bytes.Offset),
                            JsonValueType.Object, parentIndex);
                        return values[index];
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        public static JsonNode Parse(String json)
        {
            return Parse(Utf8String.FromString(json));
        }

        public static JsonNode Parse(Utf8String json)
        {
            var result = new List<JsonValue>();
            var value = Parse(json, result, -1);
            if (value.ValueType != JsonValueType.Array && value.ValueType != JsonValueType.Object)
            {
                result.Add(value);
                return new JsonNode(result);
            }
            else
            {
                return new JsonNode(result);
            }
        }
    }
}
