using System;


namespace UniJSON
{
    public abstract class JsonSchemaValidatorBase
    {
        public abstract void Assign(JsonSchemaValidatorBase obj);

        public abstract bool Parse(IFileSystemAccessor fs, string key, JsonNode value);

        public virtual bool Validate(Object o)
        {
            if (o == null)
            {
                return false;
            }
            return true;
        }

        public abstract void Serialize(JsonFormatter f, Object o);
    }
}
