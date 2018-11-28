using System;
using System.Globalization;


namespace UniJSON
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

        Integer, // JsonSchema

        //Close, // internal use
    }

    public struct JsonValue
    {
        public Utf8String Segment;
        public JsonValueType ValueType;
        public int ParentIndex;

        public JsonValue(Utf8String segment, JsonValueType valueType, int parentIndex)
        {
            Segment = segment;
            ValueType = valueType;
            ParentIndex = parentIndex;
            //UnityEngine.Debug.LogFormat("{0}", this.ToString());
        }

        public static readonly JsonValue Empty = new JsonValue
        {
            ParentIndex = -1
        };

        public override string ToString()
        {
            //return "[" + ParentIndex + "]" + ValueType + ": " + Segment.ToString();
            switch (ValueType)
            {
                case JsonValueType.Null:
                case JsonValueType.Boolean:
                case JsonValueType.Integer:
                case JsonValueType.Number:
                case JsonValueType.Array:
                case JsonValueType.Object:
                    return Segment.ToString();

                case JsonValueType.String:
                    return GetString();

                default:
                    throw new NotImplementedException();
            }
        }

        public Boolean GetBoolean()
        {
            var s = Segment.ToString();
            if (s == "true")
            {
                return true;
            }
            else if (s == "false")
            {
                return false;
            }
            else
            {
                throw new JsonValueException("invalid boolean: " + s);
            }
        }

        public SByte GetInt8()
        {
            return SByte.Parse(Segment.ToString());
        }
        public Int16 GetInt16()
        {
            return Int16.Parse(Segment.ToString());
        }
        public Int32 GetInt32()
        {
            return Int32.Parse(Segment.ToString());
        }
        public Int64 GetInt64()
        {
            return Int64.Parse(Segment.ToString());
        }

        public Byte GetUInt8()
        {
            return Byte.Parse(Segment.ToString());
        }
        public UInt16 GetUInt16()
        {
            return UInt16.Parse(Segment.ToString());
        }
        public UInt32 GetUInt32()
        {
            return UInt32.Parse(Segment.ToString());
        }
        public UInt64 GetUInt64()
        {
            return UInt64.Parse(Segment.ToString());
        }

        public Single GetSingle()
        {
            return Single.Parse(Segment.ToString(), CultureInfo.InvariantCulture);
        }
        public Double GetDouble()
        {
            return Double.Parse(Segment.ToString(), CultureInfo.InvariantCulture);
        }

        public String GetString()
        {
            var quoted = Segment.ToString();
            return JsonString.Unquote(quoted);
        }

        public Utf8String GetUtf8String()
        {
            return JsonString.Unquote(Segment);
        }
    }
}
