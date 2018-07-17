using System;


namespace UniJSON
{
    public class JsonBoolValidator : JsonSchemaValidatorBase
    {
        public override JsonValueType JsonValueType { get { return JsonValueType.Boolean; } }

        public override void Assign(JsonSchemaValidatorBase obj)
        {
            throw new NotImplementedException();
        }

        public override bool Parse(IFileSystemAccessor fs, string key, JsonNode value)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return 1;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonBoolValidator;
            if (rhs == null) return false;
            return true;
        }

        public override void Serialize(JsonFormatter f, object o)
        {
            f.Value((bool)o);
        }
    }
}
