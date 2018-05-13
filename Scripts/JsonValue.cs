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

        //Close, // internal use
    }

    public struct JsonValue
    {
        public StringSegment Segment;
        public JsonValueType ValueType;
        public int ParentIndex;

        public JsonValue(StringSegment segment, JsonValueType valueType, int parentIndex)
        {
            Segment = segment;
            ValueType = valueType;
            ParentIndex = parentIndex;
            //UnityEngine.Debug.LogFormat("{0}", this.ToString());
        }

        public override string ToString()
        {
            return "[" + ParentIndex + "]" + ValueType + ": " + Segment.ToString();
        }

        public Int32 GetInt32()
        {
            return Int32.Parse(Segment.ToString());
        }

        public String GetString()
        {
            return Segment.ToString().Substring(1, Segment.Count - 2);
        }
    }
}
