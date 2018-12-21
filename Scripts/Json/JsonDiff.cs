using System;

namespace UniJSON
{
    public enum JsonDiffType
    {
        KeyAdded,
        KeyRemoved,
        ValueChanged,
    }

    public struct JsonDiff<T> where T : IValueNode<T>, new()
    {
        public JsonPointer<T> Path;
        public JsonDiffType DiffType;
        public string Msg;

        public JsonDiff(T node, JsonDiffType diffType, string msg)
        {
            Path = new JsonPointer<T>(node);
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
