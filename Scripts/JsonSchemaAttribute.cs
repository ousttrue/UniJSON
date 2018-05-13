using System;


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
        AsInt,
        AsString,
    }

    public class JsonSchemaObjectAttribute : Attribute
    {
        public string Title;
    }
}
