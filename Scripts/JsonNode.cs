using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniJSON
{
    public struct JsonPath
    {
        String[] m_path;
        public JsonPath(JsonNode node)
        {
            m_path = node.Path().Select(x => GetKeyFromParent(x)).ToArray();
        }

        public override string ToString()
        {
            return String.Join("", m_path);
        }

        static string GetKeyFromParent(JsonNode json)
        {
            if (!json.HasParent)
            {
                return "$";
            }

            var parent = json.Parent;
            switch (parent.Value.ValueType)
            {
                case JsonValueType.Array:
                    {
                        return "[" + parent.IndexOf(json) + "]";
                    }

                case JsonValueType.Object:
                    {
                        return "." + parent.KeyOf(json);
                    }

                default:
                    throw new NotImplementedException();
            }           
        }
    }

    public enum JsonDiff
    {
        KeyAdded,
        KeyRemoved,
        ValueChanged,
    }

    public struct JsonNode
    {
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is JsonNode))
            {
                return false;
            }

            var rhs = (JsonNode)obj;

            if (Value.ValueType != rhs.Value.ValueType)
            {
                return false;
            }

            switch (Value.ValueType)
            {
                case JsonValueType.Null:
                    return true;

                case JsonValueType.Boolean:
                    return Value.GetBoolean() == rhs.GetBoolean();

                case JsonValueType.Integer:
                    return Value.GetInt32() == rhs.GetInt32();

                case JsonValueType.Number:
                    return Value.GetDouble() == rhs.GetDouble();

                case JsonValueType.String:
                    return Value.GetString() == rhs.GetString();

                case JsonValueType.Array:
                    return ArrayItems.SequenceEqual(rhs.ArrayItems);

                case JsonValueType.Object:
                    {
                        var l = ObjectItems.ToDictionary(x => x.Key, x => x.Value);
                        var r = rhs.ObjectItems.ToDictionary(x => x.Key, x => x.Value);
                        l.Equals(r);
                        return ObjectItems.OrderBy(x => x.Key).SequenceEqual(rhs.ObjectItems.OrderBy(x => x.Key));
                    }
            }

            return false;
        }

        public IEnumerable<KeyValuePair<JsonPath, JsonDiff>> Diff(JsonNode rhs, JsonPath path = default(JsonPath))
        {
            switch (Value.ValueType)
            {
                case JsonValueType.Null:
                case JsonValueType.Boolean:
                case JsonValueType.Number:
                case JsonValueType.Integer:
                case JsonValueType.String:
                case JsonValueType.Array:
                    if (!Equals(rhs))
                    {
                        yield return new KeyValuePair<JsonPath, JsonDiff>(new JsonPath(this), JsonDiff.ValueChanged);
                    }
                    yield break;
            }

            if (rhs.Value.ValueType != JsonValueType.Object)
            {
                yield return new KeyValuePair<JsonPath, JsonDiff>(new JsonPath(this), JsonDiff.ValueChanged);
                yield break;
            }

            var l = ObjectItems.ToDictionary(x => x.Key, x => x.Value);
            var r = rhs.ObjectItems.ToDictionary(x => x.Key, x => x.Value);

            foreach (var kv in l)
            {
                JsonNode x;
                if (r.TryGetValue(kv.Key, out x))
                {
                    // Found
                    foreach(var y in kv.Value.Diff(x))
                    {
                        yield return y;
                    }
                }
                else
                {
                    // Removed
                    yield return new KeyValuePair<JsonPath, JsonDiff>(new JsonPath(kv.Value), JsonDiff.KeyRemoved);
                }
            }

            foreach(var kv in r)
            {
                // Addded
                yield return new KeyValuePair<JsonPath, JsonDiff>(new JsonPath(kv.Value), JsonDiff.KeyAdded);
            }
        }

        public readonly List<JsonValue> Values;
        int m_index;
        public JsonValue Value
        {
            get { return Values[m_index]; }
        }
        public IEnumerable<JsonNode> Children
        {
            get
            {
                for (int i = 0; i < Values.Count; ++i)
                {
                    if (Values[i].ParentIndex == m_index)
                    {
                        yield return new JsonNode(Values, i);
                    }
                }
            }
        }
        public bool HasParent
        {
            get
            {
                return Value.ParentIndex >= 0 && Value.ParentIndex < Values.Count;
            }
        }
        public JsonNode Parent
        {
            get
            {
                if (Value.ParentIndex < 0)
                {
                    throw new Exception("no parent");
                }
                if (Value.ParentIndex >= Values.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                return new JsonNode(Values, Value.ParentIndex);
            }
        }

        public JsonNode(List<JsonValue> values, int index = 0)
        {
            Values = values;
            m_index = index;
        }

        #region object interface
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
                if (this.Value.ValueType != JsonValueType.Object) throw new JsonValueException("is not object");
                var it = Children.GetEnumerator();
                while (it.MoveNext())
                {
                    var key = it.Current.GetString();

                    it.MoveNext();
                    yield return new KeyValuePair<string, JsonNode>(key, it.Current);
                }
            }
        }
        public string KeyOf(JsonNode node)
        {
            foreach (var kv in ObjectItems)
            {
                if (node.m_index == kv.Value.m_index)
                {
                    return kv.Key;
                }
            }
            throw new KeyNotFoundException();
        }
        #endregion

        #region array interface
        public JsonNode this[int index]
        {
            get
            {
                int i = 0;
                foreach (var v in ArrayItems)
                {
                    if (i++ == index)
                    {
                        return v;
                    }
                }
                throw new KeyNotFoundException();
            }
        }
        public IEnumerable<JsonNode> ArrayItems
        {
            get
            {
                if (this.Value.ValueType != JsonValueType.Array) throw new JsonValueException("is not object");
                return Children;
            }
        }
        public int IndexOf(JsonNode child)
        {
            int i = 0;
            foreach (var v in ArrayItems)
            {
                if (v.m_index==child.m_index)
                {
                    return i;
                }
                ++i;
            }
            throw new KeyNotFoundException();
        }
        #endregion

        public void RemoveKey(string key)
        {
            if (Value.ValueType != JsonValueType.Object)
            {
                throw new Exception("is not object");
            }

            var parentIndex = m_index;
            var indices = Values
                .Select((value, index) => new { value, index })
                .Where(x => x.value.ParentIndex == parentIndex)
                .ToArray();

            for (int i = 0; i < indices.Length; i += 2)
            {
                if (indices[i].value.GetString() == key)
                {
                    Values[indices[i].index] = JsonValue.Empty; // remove
                    Values[indices[i + 1].index] = JsonValue.Empty; // remove
                }
            }
        }

        public void AddNode(string key, JsonNode node)
        {
            if (Value.ValueType != JsonValueType.Object)
            {
                throw new InvalidOperationException();
            }

            switch (key)
            {
                case "title":
                case "$schema":
                    // skip
                    return;
            }

            Values.Add(new JsonValue(new StringSegment("\"" + key + "\""), JsonValueType.String, m_index));
            AddNode(node);
        }

        private void AddNode(JsonNode node)
        {
            var index = Values.Count;
            Values.Add(new JsonValue(node.Value.Segment, node.Value.ValueType, m_index));

            var parent = new JsonNode(Values, index);
            if (node.Value.ValueType == JsonValueType.Array)
            {
                foreach (var value in node.ArrayItems)
                {
                    parent.AddNode(value);
                }
            }
            else if (node.Value.ValueType == JsonValueType.Object)
            {
                foreach (var kv in node.ObjectItems)
                {
                    parent.AddNode(kv.Key, kv.Value);
                }
            }
        }
    }

    public static class JsonNodeExtensions
    {
        public static Boolean GetBoolean(this JsonNode self)
        {
            return self.Value.GetBoolean();
        }

        public static Int32 GetInt32(this JsonNode self)
        {
            return self.Value.GetInt32();
        }

        public static Double GetDouble(this JsonNode self)
        {
            return self.Value.GetDouble();
        }

        public static string GetString(this JsonNode self)
        {
            return self.Value.GetString();
        }

        /*
        public static IEnumerable<KeyValuePair<string, JsonNode>> TraverseObjects(this JsonNode self)
        {
            foreach (var kv in self.ObjectItems)
            {
                yield return kv;

                if (kv.Value.Value.ValueType == JsonValueType.Object)
                {
                    foreach (var _kv in kv.Value.TraverseObjects())
                    {
                        yield return _kv;
                    }
                }
            }
        }
        */
        public static IEnumerable<JsonNode> Traverse(this JsonNode self)
        {
            yield return self;
            if (self.Value.ValueType == JsonValueType.Array)
            {
                foreach (var x in self.ArrayItems)
                {
                    foreach(var y in x.Traverse())
                    {
                        yield return y;
                    }
                }
            }
            else if (self.Value.ValueType == JsonValueType.Object)
            {
                foreach (var kv in self.ObjectItems)
                {
                    foreach (var y in kv.Value.Traverse())
                    {
                        yield return y;
                    }
                }
            }
        }

        public static IEnumerable<JsonNode> Path(this JsonNode self)
        {
            if (self.HasParent)
            {
                foreach(var x in self.Parent.Path())
                {
                    yield return x;
                }
            }

            yield return self;
        }
    }
}
