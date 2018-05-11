using System;


namespace UniJSON
{
    public class JSONSchemaAttribute : Attribute
    {
        public readonly bool Required;
        public readonly string Description;
        public readonly object Minimum;

        public JSONSchemaAttribute(bool required = false, string description = "", object minimum = null)
        {
            Required = required;
            Description = description;
            Minimum = minimum;
        }
    }
}
