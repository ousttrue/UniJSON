using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace UniJSON
{
    public struct ListTreeNode<T> : ITreeNode<ListTreeNode<T>, T>
        where T : IListTreeItem, IValue<T>
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
            if (this.IsArray())
            {
                var sb = new StringBuilder();
                bool isFirst = true;
                sb.Append("[");
                foreach (var x in this.ArrayItems())
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }
                    sb.Append(x.ToString());
                }
                sb.Append("]");
                return sb.ToString();
            }
            else if (this.IsMap())
            {
                var sb = new StringBuilder();
                bool isFirst = true;
                sb.Append("{");
                foreach (var x in this.ObjectItems())
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }
                    sb.Append(x.ToString());
                }
                sb.Append("}");
                return sb.ToString();
            }
            else
            {
                return Value.ToString();
            }
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

        public IEnumerable<JsonDiff> Diff(ListTreeNode<T> rhs, JsonPointer path = default(JsonPointer))
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
                        yield return JsonDiff.Create(this, JsonDiffType.ValueChanged, string.Format("{0} => {1}", Value, rhs.Value));
                    }
                    yield break;
            }

            if (Value.ValueType != rhs.Value.ValueType)
            {
                yield return JsonDiff.Create(this, JsonDiffType.ValueChanged, string.Format("{0} => {1}", Value.ValueType, rhs.Value));
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
                        yield return JsonDiff.Create(kv.Value, JsonDiffType.KeyRemoved, kv.Value.Value.ToString());
                    }
                }

                foreach (var kv in r)
                {
                    // Addded
                    yield return JsonDiff.Create(kv.Value, JsonDiffType.KeyAdded, kv.Value.Value.ToString());
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
                        yield return JsonDiff.Create(ll.Current, JsonDiffType.KeyRemoved, ll.Current.Value.ToString());
                    }
                    else if (rrr)
                    {
                        yield return JsonDiff.Create(rr.Current, JsonDiffType.KeyAdded, rr.Current.Value.ToString());
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
                return this.GetObjectItem(key);
            }
        }

        public ListTreeNode<T> this[int index]
        {
            get
            {
                return this.GetArrrayItem(index);
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

        #region JsonPointer
        public void SetValue(Utf8String jsonPointer, ArraySegment<Byte> bytes)
        {
            foreach (var node in this.GetNodes(jsonPointer))
            {
                Values[node.ValueIndex] = default(T).New(
                    bytes,
                    ValueNodeType.Boolean,
                    node.Value.ParentIndex);
            }
        }

        public void RemoveValue(Utf8String jsonPointer)
        {
            foreach (var node in this.GetNodes(new JsonPointer(jsonPointer)))
            {
                if (node.Parent.IsMap())
                {
                    Values[node.ValueIndex - 1] = default(T); // remove key
                }
                Values[node.ValueIndex] = default(T); // remove
            }
        }

        public void AddKey(Utf8String key)
        {
            Values.Add(default(T).Key(key, ValueIndex));
        }

        public void AddValue(ArraySegment<byte> bytes, ValueNodeType valueType)
        {
            Values.Add(default(T).New(bytes, valueType, ValueIndex));
        }
        #endregion
    }

    public static class ListTreeNodeExtensions
    {
        #region IValue
        public static bool IsNull<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            return self.Value.ValueType == ValueNodeType.Null;
        }

        public static bool IsBoolean<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            return self.Value.ValueType == ValueNodeType.Boolean;
        }

        public static bool IsString<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            return self.Value.ValueType == ValueNodeType.String;
        }

        public static bool IsInteger<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            return self.Value.ValueType == ValueNodeType.Integer;
        }

        public static bool IsFloat<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            return self.Value.ValueType == ValueNodeType.Number;
        }

        public static bool IsArray<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            return self.Value.ValueType == ValueNodeType.Array;
        }

        public static bool IsMap<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            return self.Value.ValueType == ValueNodeType.Object;
        }

        public static bool GetBoolean<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetBoolean(); }
        public static string GetString<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetString(); }
        public static Utf8String GetUtf8String<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetUtf8String(); }
        public static sbyte GetSByte<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetSByte(); }
        public static short GetInt16<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetInt16(); }
        public static int GetInt32<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetInt32(); }
        public static long GetInt64<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetInt64(); }
        public static byte GetByte<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetByte(); }
        public static ushort GetUInt16<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetUInt16(); }
        public static uint GetUInt32<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetUInt32(); }
        public static ulong GetUInt64<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetUInt64(); }
        public static float GetSingle<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetSingle(); }
        public static double GetDouble<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T> { return self.Value.GetDouble(); }

        /// <summary>
        /// for UnitTest. Use explicit GetT() or Deserialize(ref T)
        /// </summary>
        /// <returns></returns>
        public static object GetValue<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            return self.Value.GetValue<object>();
        }
        #endregion

        public static IEnumerable<ListTreeNode<T>> Traverse<T>(this ListTreeNode<T> self) where T : IListTreeItem, IValue<T>
        {
            yield return self;
            if (self.IsArray())
            {
                foreach (var x in self.ArrayItems())
                {
                    foreach (var y in x.Traverse())
                    {
                        yield return y;
                    }
                }
            }
            else if (self.IsMap())
            {
                foreach (var kv in self.ObjectItems())
                {
                    foreach (var y in kv.Value.Traverse())
                    {
                        yield return y;
                    }
                }
            }
        }
    }
}
