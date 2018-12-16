using System;


namespace UniJSON
{
    public class JsonParseException : Exception
    {
        public JsonParseException(string msg) : base(msg) { }
    }

    public class JsonFormatException : ArgumentException
    {
        public JsonFormatException(string msg) : base(msg) { }
    }

    public class JsonValueException : Exception
    {
        public JsonValueException(string msg=null) : base(msg) { }
    }

    public class JsonValueTypeException: JsonValueException
    {
        public JsonValueTypeException(ValueType type) : base(type.ToString()) { }
    }
}
