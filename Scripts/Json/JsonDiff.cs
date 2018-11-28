using System;

namespace UniJSON
{
    public enum JsonDiffType
    {
        KeyAdded,
        KeyRemoved,
        ValueChanged,
    }

    public struct JsonDiff<T> where T : IValueNode, new()
    {
        public JsonPointer Path;
        public JsonDiffType DiffType;
        public string Msg;

        public JsonDiff(T node, JsonDiffType diffType, string msg)
        {
            Path = new JsonPointer(node);
            DiffType = diffType;
            Msg = msg;
        }

        public override string ToString()
        {
            switch (DiffType)
            {
                case JsonDiffType.KeyAdded:
                    return string.Format("+ {0}: {1}", Path, Msg);
                case JsonDiffType.KeyRemoved:
                    return string.Format("- {0}: {1}", Path, Msg);
                case JsonDiffType.ValueChanged:
                    return string.Format("= {0}: {1}", Path, Msg);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
