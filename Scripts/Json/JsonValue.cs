using System;


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

        static Utf8String s_true = Utf8String.FromString("true");
        static Utf8String s_false = Utf8String.FromString("false");

        public Boolean GetBoolean()
        {
            if (Segment == s_true)
            {
                return true;
            }
            else if (Segment == s_false)
            {
                return false;
            }
            else
            {
                throw new JsonValueException("invalid boolean: " + Segment.ToString());
            }
        }

        public SByte GetInt8()
        {
            return Segment.ToSByte();
        }
        public Int16 GetInt16()
        {
            return Segment.ToInt16();
        }
        public Int32 GetInt32()
        {
            return Segment.ToInt32();
        }
        public Int64 GetInt64()
        {
            return Segment.ToInt64();
        }

        public Byte GetUInt8()
        {
            return Segment.ToByte();
        }
        public UInt16 GetUInt16()
        {
            return Segment.ToUInt16();
        }
        public UInt32 GetUInt32()
        {
            return Segment.ToUInt32();
        }
        public UInt64 GetUInt64()
        {
            return Segment.ToUInt64();
        }

        public Single GetSingle()
        {
            return Segment.ToSingle();
        }
        public Double GetDouble()
        {
            return Segment.ToDouble();
        }

        public String GetString()
        {
            return JsonString.Unquote(Segment.ToString());
        }
        public Utf8String GetUtf8String()
        {
            return JsonString.Unquote(Segment);
        }
    }
}
