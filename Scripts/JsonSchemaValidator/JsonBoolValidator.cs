﻿using System;


namespace UniJSON
{
    public class JsonBoolValidator : IJsonSchemaValidator
    {
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

        public void Merge(IJsonSchemaValidator obj)
        {
            throw new NotImplementedException();
        }

        public bool FromJsonSchema(IFileSystemAccessor fs, string key, ListTreeNode<JsonValue> value)
        {
            return false;
        }

        public void ToJsonScheama(IFormatter f)
        {
            f.Key("type"); f.Value("boolean");
        }

        public JsonSchemaValidationException Validate<T>(JsonSchemaValidationContext c, T value)
        {
            return null;
        }

        public void Serialize<T>(IFormatter f, JsonSchemaValidationContext c, T value)
        {
            f.Serialize(value);
        }

        public void Deserialize<S, T>(S src, ref T dst) where S : IValueNode<S>
        {
            dst = GenericCast<bool, T>.Cast(src.GetBoolean());
        }
    }
}
