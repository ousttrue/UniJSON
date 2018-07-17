using System;


namespace UniJSON
{
    public interface IJsonSchemaValidator
    {
        void Assign(IJsonSchemaValidator obj);

        bool Parse(IFileSystemAccessor fs, string key, JsonNode value);

        bool Validate(Object o);

        void Serialize(JsonFormatter f, Object o);
    }
}
