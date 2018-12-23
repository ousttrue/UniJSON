using System;


namespace UniJSON
{
    /// <summary>
    ///Exception failure
    /// </summary>
    public class ParserException : ArgumentException
    {
        public ParserException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Successfully parsed, but fail to getValue
    /// </summary>
    public class DeserializationException : ArgumentException
    {
        public DeserializationException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Formatter exception. key value violation
    /// </summary>
    public class FormatterException : FormatException
    {
        public FormatterException(string msg) : base(msg) { }
    }
}
