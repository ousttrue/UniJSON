using System;
using System.Collections.Generic;


namespace UniJSON
{
    public class JsonSchemaPropertyAttribute : Attribute
    {
        public bool Required;
        public string Description;
        public object Minimum;
        public EnumSerializationType EnumSerializationType;
    }

    public enum EnumSerializationType
    {
        AsString,
        AsInt,
    }

    public class JsonSchemaObjectAttribute : Attribute
    {
        public string Title;
    }
}
