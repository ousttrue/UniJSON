using System;


namespace UniJSON
{
    public abstract class JsonSchemaValidatorBase
    {
        public abstract void Assign(JsonSchemaValidatorBase obj);

        public abstract bool Parse(IFileSystemAccessor fs, string key, JsonNode value);

        public abstract bool Validate(Object o);

        public abstract void Serialize(JsonFormatter f, Object o);
    }
}
