using System;
using System.Collections.Generic;
using System.Linq;


namespace UniJSON
{
    public struct ListTreeNode<T> : IValueNode<ListTreeNode<T>> where T : struct, ITreeItem, IValue<T>
    {
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ListTreeNode<T>))
            {
                return false;
            }

            var rhs = (ListTreeNode<T>)obj;

            if ((Value.ValueType == ValueNodeType.Integer || Value.ValueType == ValueNodeType.Null)
                && (rhs.Value.ValueType == ValueNodeType.Integer || rhs.Value.ValueType == ValueNodeType.Number))
            {
                // ok
            }
            else if (Value.ValueType != rhs.Value.ValueType)
            {
                return false;
            }

            switch (Value.ValueType)
            {
                case ValueNodeType.Null:
                    return true;

                case ValueNodeType.Boolean:
                    return Value.GetBoolean() == rhs.GetBoolean();

                case ValueNodeType.Integer:
                case ValueNodeType.Number:
                    return Value.GetDouble() == rhs.GetDouble();

                case ValueNodeType.String:
                    return Value.GetString() == rhs.GetString();

                case ValueNodeType.Array:
                    return this.ArrayItems().SequenceEqual(rhs.ArrayItems());

                case ValueNodeType.Object:
                    {
                        //var l = ObjectItems().ToDictionary(x => x.Key, x => x.Value);
                        //var r = rhs.ObjectItems().ToDictionary(x => x.Key, x => x.Value);
                        //return l.Equals(r);
                        return this.ObjectItems().OrderBy(x => x.Key.GetUtf8String()).SequenceEqual(rhs.ObjectItems().OrderBy(x => x.Key.GetUtf8String()));
                    }
            }

            return false;
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
                foreach (var x in this.ArrayItems())
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
                foreach (var kv in this.ObjectItems())
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

        public IEnumerable<JsonDiff<ListTreeNode<T>>> Diff(ListTreeNode<T> rhs, JsonPointer<ListTreeNode<T>> path = default(JsonPointer<ListTreeNode<T>>))
        {
            switch (Value.ValueType)
            {
                case ValueNodeType.Null:
                case ValueNodeType.Boolean:
                case ValueNodeType.Number:
                case ValueNodeType.Integer:
                case ValueNodeType.String:
                    if (!Equals(rhs))
                    {
                        yield return new JsonDiff<ListTreeNode<T>>(this, JsonDiffType.ValueChanged, string.Format("{0} => {1}", Value, rhs.Value));
                    }
                    yield break;
            }

            if (Value.ValueType != rhs.Value.ValueType)
            {
                yield return new JsonDiff<ListTreeNode<T>>(this, JsonDiffType.ValueChanged, string.Format("{0} => {1}", Value.ValueType, rhs.Value));
                yield break;
            }

            if (Value.ValueType == ValueNodeType.Object)
            {

                var l = this.ObjectItems().ToDictionary(x => x.Key, x => x.Value);
                var r = rhs.ObjectItems().ToDictionary(x => x.Key, x => x.Value);

                foreach (var kv in l)
                {
                    ListTreeNode<T> x;
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
                        yield return new JsonDiff<ListTreeNode<T>>(kv.Value, JsonDiffType.KeyRemoved, kv.Value.Value.ToString());
                    }
                }

                foreach (var kv in r)
                {
                    // Addded
                    yield return new JsonDiff<ListTreeNode<T>>(kv.Value, JsonDiffType.KeyAdded, kv.Value.Value.ToString());
                }
            }
            else if (Value.ValueType == ValueNodeType.Array)
            {
                var ll = this.ArrayItems().GetEnumerator();
                var rr = rhs.ArrayItems().GetEnumerator();
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
                        yield return new JsonDiff<ListTreeNode<T>>(ll.Current, JsonDiffType.KeyRemoved, ll.Current.Value.ToString());
                    }
                    else if (rrr)
                    {
                        yield return new JsonDiff<ListTreeNode<T>>(rr.Current, JsonDiffType.KeyAdded, rr.Current.Value.ToString());
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

        /// <summary>
        /// Whole tree nodes
        /// </summary>
        public readonly List<T> Values;
        public bool IsValid
        {
            get
            {
                return Values != null;
            }
        }

        /// <summary>
        /// This node index
        /// </summary>
        public int ValueIndex
        {
            get;
            private set;
        }

        public T Value
        {
            get
            {
                if (Values == null)
                {
                    return default(T);
                }
                return Values[ValueIndex];
            }
        }

        public ArraySegment<byte> Bytes { get { return Value.Bytes; } }
        public ValueNodeType ValueType { get { return Value.ValueType; } }

        #region Children
        public IEnumerable<ListTreeNode<T>> Children
        {
            get
            {
                for (int i = 0; i < Values.Count; ++i)
                {
                    if (Values[i].ParentIndex == ValueIndex)
                    {
                        yield return new ListTreeNode<T>(Values, i);
                    }
                }
            }
        }

        public ListTreeNode<T> this[String key]
        {
            get
            {
                return this[Utf8String.From(key)];
            }
        }

        public ListTreeNode<T> this[Utf8String key]
        {
            get
            {
                foreach (var kv in this.ObjectItems())
                {
                    if (kv.Key.GetUtf8String() == key)
                    {
                        return kv.Value;
                    }
                }
                throw new KeyNotFoundException();
            }
        }

        public ListTreeNode<T> this[int index]
        {
            get
            {
                int i = 0;
                foreach (var v in this.ArrayItems())
                {
                    if (i++ == index)
                    {
                        return v;
                    }
                }
                throw new KeyNotFoundException();
            }
        }

        public IEnumerable<KeyValuePair<ListTreeNode<T>, ListTreeNode<T>>> ObjectItems
        {
            get
            {
                if (!this.IsMap()) throw new JsonValueException("is not object");
                var it = Children.GetEnumerator();
                while (it.MoveNext())
                {
                    var key = it.Current;

                    it.MoveNext();
                    yield return new KeyValuePair<ListTreeNode<T>, ListTreeNode<T>>(key, it.Current);
                }
            }
        }

        public IEnumerable<ListTreeNode<T>> ArrayItems
        {
            get
            {
                if (!this.IsArray()) throw new JsonValueException("is not object");
                return Children;
            }
        }
        #endregion


        public bool HasParent
        {
            get
            {
                return Value.ParentIndex >= 0 && Value.ParentIndex < Values.Count;
            }
        }
        public ListTreeNode<T> Parent
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
                return new ListTreeNode<T>(Values, Value.ParentIndex);
            }
        }

        public ListTreeNode(List<T> values, int index = 0)
        {
            Values = values;
            ValueIndex = index;
        }

        #region

        public bool GetBoolean() { return Value.GetBoolean(); }
        public string GetString(){return Value.GetString();}
        public Utf8String GetUtf8String(){ return Value.GetUtf8String(); }
        public sbyte GetSByte() { return Value.GetSByte(); }
        public short GetInt16() { return Value.GetInt16(); }
        public int GetInt32() { return Value.GetInt32(); }
        public long GetInt64() { return Value.GetInt64(); }
        public byte GetByte() { return Value.GetByte(); }
        public ushort GetUInt16() { return Value.GetUInt16(); }
        public uint GetUInt32() { return Value.GetUInt32(); }
        public ulong GetUInt64() { return Value.GetUInt64(); }
        public float GetSingle() { return Value.GetSingle(); }
        public double GetDouble() { return Value.GetDouble(); }
        #endregion

        #region JsonPointer
        public void SetValue<U>(Utf8String jsonPointer, U value)
        {
            var f = new JsonFormatter();
            f.Serialize(value);

            foreach (var node in GetNodes(jsonPointer))
            {
                Values[node.ValueIndex] = default(T).New(
                    f.GetStoreBytes(),
                    ValueNodeType.Boolean,
                    node.Value.ParentIndex);
            }
        }

        public void RemoveValue(Utf8String jsonPointer)
        {
            foreach (var node in GetNodes(new JsonPointer<ListTreeNode<T>>(jsonPointer)))
            {
                if (node.Parent.IsMap())
                {
                    Values[node.ValueIndex - 1] = default(T); // remove key
                }
                Values[node.ValueIndex] = default(T); // remove
            }
        }

        public IEnumerable<ListTreeNode<T>> GetNodes(JsonPointer<ListTreeNode<T>> jsonPointer)
        {
            if (jsonPointer.Path.Count == 0)
            {
                yield return this;
                yield break;
            }

            if (Value.ValueType == ValueNodeType.Array)
            {
                // array
                if (jsonPointer[0][0] == '*')
                {
                    // wildcard
                    foreach (var child in this.ArrayItems())
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
            else if (Value.ValueType == ValueNodeType.Object)
            {
                // object
                if (jsonPointer[0][0] == '*')
                {
                    // wildcard
                    foreach (var kv in this.ObjectItems())
                    {
                        foreach (var childChild in kv.Value.GetNodes(jsonPointer.Unshift()))
                        {
                            yield return childChild;
                        }
                    }
                }
                else
                {
                    ListTreeNode<T> child;
                    try
                    {
                        child = this[jsonPointer[0]];
                    }
                    catch (KeyNotFoundException)
                    {
                        // key
                        Values.Add(default(T).Key(jsonPointer[0], ValueIndex));
                        // value
                        Values.Add(default(T).New(default(ArraySegment<byte>), ValueNodeType.Object, ValueIndex));

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

        public IEnumerable<ListTreeNode<T>> GetNodes(Utf8String jsonPointer)
        {
            return GetNodes(new JsonPointer<ListTreeNode<T>>(jsonPointer));
        }
        #endregion
    }
}
