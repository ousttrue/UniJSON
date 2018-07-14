using System;


namespace UniJSON
{
    public class JsonSchemaAttribute : Attribute
    {
        public string Title;
        public bool Required;
        public string Description;
        public object Minimum;
        public EnumSerializationType EnumSerializationType;
        public PropertyExportFlags ExportFlags = PropertyExportFlags.Default;
    }

    public enum EnumSerializationType
    {
        AsString,
        AsInt,
    }
}
