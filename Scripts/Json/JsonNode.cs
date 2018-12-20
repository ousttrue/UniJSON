﻿using System;
using System.Collections.Generic;
using System.Linq;


namespace UniJSON
{
    public struct JsonNode : IValueNode
    {
        public ValueNodeType ValueType
        {
            get
            {
                switch (Value.ValueType)
                {
                    case JsonValueType.Array: return ValueNodeType.Array;
                    case JsonValueType.Object: return ValueNodeType.Map;
                    case JsonValueType.Null: return ValueNodeType.Null;
                    case JsonValueType.Boolean: return ValueNodeType.Boolean;
                    case JsonValueType.Integer: return ValueNodeType.Integer;
                    case JsonValueType.Number: return ValueNodeType.Float;
                    case JsonValueType.String: return ValueNodeType.String;
                    default: throw new NotImplementedException();
                }
            }
        }

        public int ValueCount
        {
            get
            {
                switch (Value.ValueType)
                {
                    case JsonValueType.Array: return Children.Count();
                    case JsonValueType.Object: return Children.Count() / 2;
                    default: throw new NotImplementedException();
                }
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        IEnumerable<string> ToString(string indent, int level, bool value = false)
        {
            if (this.IsArray())
            {
                if (!value) for (int i = 0; i < level; ++i) yield return indent;
                yield return "[\n";

                var isFirst = true;
                var childLevel = level + 1;
                foreach (var x in ArrayItemsRaw)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        yield return ",\n";
                    }

                    foreach (var y in x.ToString(indent, childLevel))
                    {
                        yield return y;
                    }
                }
                if (!isFirst)
                {
                    yield return "\n";
                }

                for (int i = 0; i < level; ++i) yield return indent;
                yield return "]";
            }
            else if (this.IsMap())
            {
                if (!value) for (int i = 0; i < level; ++i) yield return indent;
                yield return "{\n";

                var isFirst = true;
                var childLevel = level + 1;
                foreach (var kv in ObjectItemsRaw)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        yield return ",\n";
                    }

                    // key
                    for (int i = 0; i < childLevel; ++i) yield return indent;
                    yield return kv.Key.ToString();
                    yield return ": ";

                    foreach (var y in kv.Value.ToString(indent, childLevel, true))
                    {
                        yield return y;
                    }
                }
                if (!isFirst)
                {
                    yield return "\n";
                }

                for (int i = 0; i < level; ++i) yield return indent;
                yield return "}";
            }
            else
            {
                if (!value) for (int i = 0; i < level; ++i) yield return indent;
                yield return Value.ToString();
            }
        }

        public string ToString(string indent)
        {
            return string.Join("", ToString(indent, 0).ToArray());
        }

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

            if ((Value.ValueType == JsonValueType.Integer || Value.ValueType == JsonValueType.Null)
                && (rhs.Value.ValueType == JsonValueType.Integer || rhs.Value.ValueType == JsonValueType.Number))
            {
                // ok
            }
            else if (Value.ValueType != rhs.Value.ValueType)
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
                case JsonValueType.Number:
                    return Value.GetDouble() == rhs.GetDouble();

                case JsonValueType.String:
                    return Value.GetString() == rhs.GetString();

                case JsonValueType.Array:
                    return ArrayItemsRaw.SequenceEqual(rhs.ArrayItemsRaw);

                case JsonValueType.Object:
                    {
                        //var l = ObjectItemsRaw.ToDictionary(x => x.Key, x => x.Value);
                        //var r = rhs.ObjectItemsRaw.ToDictionary(x => x.Key, x => x.Value);
                        //return l.Equals(r);
                        return ObjectItemsRaw.OrderBy(x => x.Key.GetUtf8String()).SequenceEqual(rhs.ObjectItemsRaw.OrderBy(x => x.Key.GetUtf8String()));
                    }
            }

            return false;
        }

        public IEnumerable<JsonDiff<JsonNode>> Diff(JsonNode rhs, JsonPointer path = default(JsonPointer))
        {
            switch (Value.ValueType)
            {
                case JsonValueType.Null:
                case JsonValueType.Boolean:
                case JsonValueType.Number:
                case JsonValueType.Integer:
                case JsonValueType.String:
                    if (!Equals(rhs))
                    {
                        yield return new JsonDiff<JsonNode>(this, JsonDiffType.ValueChanged, string.Format("{0} => {1}", Value, rhs.Value));
                    }
                    yield break;
            }

            if (Value.ValueType != rhs.Value.ValueType)
            {
                yield return new JsonDiff<JsonNode>(this, JsonDiffType.ValueChanged, string.Format("{0} => {1}", Value.ValueType, rhs.Value));
                yield break;
            }

            if (Value.ValueType == JsonValueType.Object)
            {

                var l = ObjectItemsRaw.ToDictionary(x => x.Key, x => x.Value);
                var r = rhs.ObjectItemsRaw.ToDictionary(x => x.Key, x => x.Value);

                foreach (var kv in l)
                {
                    JsonNode x;
                    if (r.TryGetValue(kv.Key, out x))
                    {
                        r.Remove(kv.Key);
                        // Found
                        foreach (var y in kv.Value.Diff(x))
                        {
                            yield return y;
                        }
                    }
                    else
                    {
                        // Removed
                        yield return new JsonDiff<JsonNode>(kv.Value, JsonDiffType.KeyRemoved, kv.Value.Value.ToString());
                    }
                }

                foreach (var kv in r)
                {
                    // Addded
                    yield return new JsonDiff<JsonNode>(kv.Value, JsonDiffType.KeyAdded, kv.Value.Value.ToString());
                }
            }
            else if (Value.ValueType == JsonValueType.Array)
            {
                var ll = ArrayItemsRaw.GetEnumerator();
                var rr = rhs.ArrayItemsRaw.GetEnumerator();
                while (true)
                {
                    var lll = ll.MoveNext();
                    var rrr = rr.MoveNext();
                    if (lll && rrr)
                    {
                        // found
                        foreach (var y in ll.Current.Diff(rr.Current))
                        {
                            yield return y;
                        }
                    }
                    else if (lll)
                    {
                        yield return new JsonDiff<JsonNode>(ll.Current, JsonDiffType.KeyRemoved, ll.Current.Value.ToString());
                    }
                    else if (rrr)
                    {
                        yield return new JsonDiff<JsonNode>(rr.Current, JsonDiffType.KeyAdded, rr.Current.Value.ToString());
                    }
                    else
                    {
                        // end
                        break;
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public readonly List<JsonValue> Values;
        int m_index;
        public int ValueIndex
        {
            get
            {
                return m_index;
            }
        }
        JsonValue Value
        {
            get {
                if (Values == null)
                {
                    return default(JsonValue);
                }
                return Values[m_index];
            }
        }
        public ArraySegment<byte> Bytes
        {
            get
            {
                return Value.Segment.Bytes;
            }
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
        public IValueNode Parent
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
        public JsonNode this[String key]
        {
            get
            {
                return this[Utf8String.From(key)];
            }
        }

        public JsonNode this[Utf8String key]
        {
            get
            {
                foreach (var kv in ObjectItemsRaw)
                {
                    if (kv.Key.GetUtf8String() == key)
                    {
                        return kv.Value;
                    }
                }
                throw new KeyNotFoundException();
            }
        }

        public IEnumerable<KeyValuePair<Utf8String, IValueNode>> ObjectItems
        {
            get
            {
                return ObjectItemsRaw.Select(x => new KeyValuePair<Utf8String, IValueNode>(x.Key.GetUtf8String(), x.Value as IValueNode));
            }
        }
        public IEnumerable<KeyValuePair<JsonNode, JsonNode>> ObjectItemsRaw
        {
            get
            {
                if (this.Value.ValueType != JsonValueType.Object) throw new JsonValueException("is not object");
                var it = Children.GetEnumerator();
                while (it.MoveNext())
                {
                    var key = it.Current;

                    it.MoveNext();
                    yield return new KeyValuePair<JsonNode, JsonNode>(key, it.Current);
                }
            }
        }
        #endregion

        #region array interface
        public JsonNode this[int index]
        {
            get
            {
                int i = 0;
                foreach (var v in ArrayItemsRaw)
                {
                    if (i++ == index)
                    {
                        return v;
                    }
                }
                throw new KeyNotFoundException();
            }
        }
        public IEnumerable<IValueNode> ArrayItems
        {
            get
            {
                return ArrayItemsRaw.Select(x => x as IValueNode);
            }
        }
        public IEnumerable<JsonNode> ArrayItemsRaw
        {
            get
            {
                if (this.Value.ValueType != JsonValueType.Array) throw new JsonValueException("is not object");
                return Children;
            }
        }
        #endregion

        #region JsonPointer
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

        public void AddNode(Utf8String key, JsonNode node)
        {
            if (Value.ValueType != JsonValueType.Object)
            {
                throw new InvalidOperationException();
            }

            Values.Add(new JsonValue(Utf8String.From("\"" + key + "\""), JsonValueType.String, m_index));
            AddNode(node);
        }

        private void AddNode(JsonNode node)
        {
            var index = Values.Count;
            Values.Add(new JsonValue(node.Value.Segment, node.Value.ValueType, m_index));

            var parent = new JsonNode(Values, index);
            if (node.Value.ValueType == JsonValueType.Array)
            {
                foreach (var value in node.ArrayItemsRaw)
                {
                    parent.AddNode(value);
                }
            }
            else if (node.Value.ValueType == JsonValueType.Object)
            {
                foreach (var kv in node.ObjectItemsRaw)
                {
                    parent.AddNode(kv.Key.GetUtf8String(), kv.Value);
                }
            }
        }

        public IEnumerable<JsonNode> GetNodes(JsonPointer jsonPointer)
        {
            if (jsonPointer.Path.Count == 0)
            {
                yield return this;
                yield break;
            }

            if (Value.ValueType == JsonValueType.Array)
            {
                // array
                if (jsonPointer[0][0] == '*')
                {
                    // wildcard
                    foreach (var child in ArrayItemsRaw)
                    {
                        foreach (var childChild in child.GetNodes(jsonPointer.Unshift()))
                        {
                            yield return childChild;
                        }
                    }
                }
                else
                {
                    int index = jsonPointer[0].ToInt32();
                    var child = this[index];
                    foreach (var childChild in child.GetNodes(jsonPointer.Unshift()))
                    {
                        yield return childChild;
                    }
                }
            }
            else if (Value.ValueType == JsonValueType.Object)
            {
                // object
                if (jsonPointer[0][0] == '*')
                {
                    // wildcard
                    foreach (var kv in ObjectItemsRaw)
                    {
                        foreach (var childChild in kv.Value.GetNodes(jsonPointer.Unshift()))
                        {
                            yield return childChild;
                        }
                    }
                }
                else
                {
                    JsonNode child;
                    try
                    {
                        child = this[jsonPointer[0]];
                    }
                    catch (KeyNotFoundException)
                    {
                        // key
                        Values.Add(new JsonValue(JsonString.Quote(jsonPointer[0]), JsonValueType.String, m_index));
                        // value
                        Values.Add(new JsonValue(default(Utf8String), JsonValueType.Object, m_index));

                        child = this[jsonPointer[0]];
                    }
                    foreach (var childChild in child.GetNodes(jsonPointer.Unshift()))
                    {
                        yield return childChild;
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<JsonNode> GetNodes(Utf8String jsonPointer)
        {
            return GetNodes(new JsonPointer(jsonPointer));
        }

        public void SetValue<T>(Utf8String jsonPointer, T value)
        {
            var f = new JsonFormatter();
            f.Serialize(value);

            foreach (var node in GetNodes(jsonPointer))
            {
                Values[node.m_index] = new JsonValue
                {
                    ParentIndex = node.Value.ParentIndex,
                    Segment = new Utf8String(f.GetStoreBytes()),
                    ValueType = JsonValueType.Boolean
                };
            }
        }

        public void RemoveValue(Utf8String jsonPointer)
        {
            foreach (var node in GetNodes(new JsonPointer(jsonPointer)))
            {
                if (node.Parent.IsMap())
                {
                    Values[node.m_index - 1] = JsonValue.Empty; // remove key
                }
                Values[node.m_index] = JsonValue.Empty; // remove
            }
        }
        #endregion

        #region Getter
        public bool GetBoolean()
        {
            if(Value.ValueType!=JsonValueType.Boolean) throw new JsonValueTypeException(Value.ValueType);
            return Value.GetBoolean();
        }

        public string GetString()
        {
            if (Value.ValueType != JsonValueType.String) throw new JsonValueTypeException(Value.ValueType);
            return Value.GetString();
        }

        public Utf8String GetUtf8String()
        {
            if (Value.ValueType != JsonValueType.String) throw new JsonValueTypeException(Value.ValueType);
            return Value.GetUtf8String();
        }

        public SByte GetSByte()
        {
            if (Value.ValueType == JsonValueType.Integer)
            {
                return Value.GetInt8();
            }
            else if(Value.ValueType == JsonValueType.Number)
            {
                return (sbyte)Value.GetDouble();
            }
            else
            {
                throw new JsonValueTypeException(Value.ValueType);
            }
        }

        public Int16 GetInt16()
        {
            if (Value.ValueType == JsonValueType.Integer)
            {
                return Value.GetInt16();
            }
            else if (Value.ValueType == JsonValueType.Number)
            {
                return (short)Value.GetDouble();
            }
            else
            {
                throw new JsonValueTypeException(Value.ValueType);
            }
        }

        public Int32 GetInt32()
        {
            if (Value.ValueType == JsonValueType.Integer)
            {
                return Value.GetInt32();
            }
            else if (Value.ValueType == JsonValueType.Number)
            {
                return (int)Value.GetDouble();
            }
            else
            {
                throw new JsonValueTypeException(Value.ValueType);
            }
        }

        public Int64 GetInt64()
        {
            if (Value.ValueType == JsonValueType.Integer)
            {
                return Value.GetInt64();
            }
            else if (Value.ValueType == JsonValueType.Number)
            {
                return (long)Value.GetDouble();
            }
            else
            {
                throw new JsonValueTypeException(Value.ValueType);
            }
        }

        public Byte GetByte()
        {
            if (Value.ValueType == JsonValueType.Integer)
            {
                return Value.GetUInt8();
            }
            else if (Value.ValueType == JsonValueType.Number)
            {
                return (byte)Value.GetDouble();
            }
            else
            {
                throw new JsonValueTypeException(Value.ValueType);
            }
        }

        public UInt16 GetUInt16()
        {
            if (Value.ValueType == JsonValueType.Integer)
            {
                return Value.GetUInt16();
            }
            else if (Value.ValueType == JsonValueType.Number)
            {
                return (ushort)Value.GetDouble();
            }
            else
            {
                throw new JsonValueTypeException(Value.ValueType);
            }
        }

        public UInt32 GetUInt32()
        {
            if (Value.ValueType == JsonValueType.Integer)
            {
                return Value.GetUInt32();
            }
            else if (Value.ValueType == JsonValueType.Number)
            {
                return (uint)Value.GetDouble();
            }
            else
            {
                throw new JsonValueTypeException(Value.ValueType);
            }
        }

        public UInt64 GetUInt64()
        {
            if (Value.ValueType == JsonValueType.Integer)
            {
                return Value.GetUInt64();
            }
            else if (Value.ValueType == JsonValueType.Number)
            {
                return (ulong)Value.GetDouble();
            }
            else
            {
                throw new JsonValueTypeException(Value.ValueType);
            }
        }

        public float GetSingle()
        {
            if (Value.ValueType != JsonValueType.Number
                && Value.ValueType != JsonValueType.Integer)
            {
                throw new JsonValueTypeException(Value.ValueType);
            }
            return Value.GetSingle();
        }

        public double GetDouble()
        {
            if (Value.ValueType != JsonValueType.Number
                && Value.ValueType != JsonValueType.Integer)
            {
                throw new JsonValueTypeException(Value.ValueType);
            }
            return Value.GetDouble();
        }
        #endregion
    }
}
