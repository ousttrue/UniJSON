using System;
using System.Linq;
using System.Collections;

namespace UniJSON
{
    /// <summary>
    /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.4
    /// </summary>
    public class JsonArrayValidator : IJsonSchemaValidator
    {
        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.4.1
        /// </summary>
        public JsonSchema Items
        {
            get; set;
        }

        // additionalItems

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.4.3
        /// </summary>
        public int? MaxItems
        {
            get; set;
        }

        /// <summary>
        /// http://json-schema.org/latest/json-schema-validation.html#rfc.section.6.4.4
        /// </summary>
        public int? MinItems
        {
            get; set;
        }

        // uniqueItems

        // contains

        public override int GetHashCode()
        {
            return 5;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as JsonArrayValidator;
            if (rhs == null) return false;

            if (Items != rhs.Items) return false;
            if (MaxItems != rhs.MaxItems) return false;
            if (MinItems != rhs.MinItems) return false;

            return true;
        }

        public void Merge(IJsonSchemaValidator rhs)
        {
            throw new NotImplementedException();
        }

        public bool FromJsonSchema(IFileSystemAccessor fs, string key, JsonNode value)
        {
            switch (key)
            {
                case "items":
                    if (value.IsArray())
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        var sub = new JsonSchema();
                        sub.Parse(fs, value, "items");
                        Items = sub;
                    }
                    return true;

                case "additionalItems":
                    return true;

                case "maxItems":
                    MaxItems = value.GetInt32();
                    return true;

                case "minItems":
                    MinItems = value.GetInt32();
                    return true;

                case "uniqueItems":
                    return true;

                case "contains":
                    return true;
            }

            return false;
        }

        public JsonSchemaValidationException Validate<T>(JsonSchemaValidationContext context, T o)
        {
            if (o == null)
            {
                return new JsonSchemaValidationException(context, "null");
            }

            var count = o.GetCount();
            if (count == 0)
            {
                return new JsonSchemaValidationException(context, "empty");
            }

            if (MaxItems.HasValue && count > MaxItems.Value)
            {
                return new JsonSchemaValidationException(context, "maxOtems");
            }

            if (MinItems.HasValue && count < MinItems.Value)
            {
                return new JsonSchemaValidationException(context, "minItems");
            }

            return null;
        }

        public void Serialize(IFormatter f, JsonSchemaValidationContext c, object o)
        {
            var array = o as IEnumerable;

            f.BeginList(array.GetCount());
            {
                int i = 0;
                foreach (var x in array)
                {
                    using (c.Push(i++))
                    {
                        Items.Validator.Serialize(f, c, x);
                    }
                }
            }
            f.EndList();
        }

        public void ToJsonScheama(IFormatter f)
        {
            f.Key("type"); f.Value("array");

            if (Items != null)
            {
                f.Key("items");
                Items.ToJson(f);
            }
        }

        public void Deserialize<T>(IValueNode src, ref T dst)
        {
            src.Deserialize(ref dst);
        }
    }
}
