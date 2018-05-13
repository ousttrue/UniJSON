using System;


namespace UniJSON
{
    public class JsonSchemaPropertyAttribute : Attribute
    {
        public readonly bool Required;
        public readonly string Description;
        public readonly object Minimum;

        public JsonSchemaPropertyAttribute(bool required = false, string description = "", object minimum = null)
        {
            Required = required;
            Description = description;
            Minimum = minimum;
        }
    }

    public class JsonSchemaObjectAttribute : Attribute
    {
        public readonly string Title;

        public JsonSchemaObjectAttribute(string title)
        {
            Title = title;
        }
    }
}
