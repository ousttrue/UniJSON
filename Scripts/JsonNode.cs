﻿using System.Collections.Generic;
using System.Linq;


namespace UniJSON
{
    public struct JsonNode
    {
        public readonly JsonValue[] Values;
        int m_index;
        public JsonValue Value
        {
            get { return Values[m_index]; }
        }
        public IEnumerable<JsonNode> Children
        {
            get
            {
                for (int i = 0; i < Values.Length; ++i)
                {
                    if (Values[i].ParentIndex == m_index)
                    {
                        yield return new JsonNode(Values, i);
                    }
                }
            }
        }

        public JsonNode(JsonValue[] values, int index = 0)
        {
            Values = values;
            m_index = index;
        }

        public JsonNode this[string key]
        {
            get
            {
                foreach (var kv in ObjectItems)
                {
                    if (kv.Key == key)
                    {
                        return kv.Value;
                    }
                }
                throw new KeyNotFoundException();
            }
        }
        public bool ContainsKey(string key)
        {
            return ObjectItems.Any(x => x.Key == key);
        }
        public IEnumerable<KeyValuePair<string, JsonNode>> ObjectItems
        {
            get
            {
                if (this.GetValueType() != JsonValueType.Object) throw new JsonValueException("is not object");
                var it = Children.GetEnumerator();
                while (it.MoveNext())
                {
                    var key = it.Current.GetString();

                    it.MoveNext();
                    yield return new KeyValuePair<string, JsonNode>(key, it.Current);
                }
            }
        }
    }

    public static class JsonNodeExtensions
    {
        public static JsonValueType GetValueType(this JsonNode self)
        {
            return self.Value.ValueType;
        }

        public static string GetString(this JsonNode self)
        {
            return self.Value.GetString();
        }
    }
}
