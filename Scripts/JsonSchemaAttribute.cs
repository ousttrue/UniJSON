using System;


namespace UniJson
{
    public class JsonSchemaAttribute : Attribute
    {
        public readonly bool Required;
        public readonly string Description;
        public readonly object Minimum;

        public JsonSchemaAttribute(bool required = false, string description = "", object minimum = null)
        {
            Required = required;
            Description = description;
            Minimum = minimum;
        }
    }
}
