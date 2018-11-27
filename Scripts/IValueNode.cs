using System.Collections.Generic;


namespace UniJSON
{
    public interface IValueNode
    {
        bool IsNull { get; }

        bool HasParent { get; }
        IValueNode Parent { get; }

        bool IsArray { get; }
        IEnumerable<IValueNode> ArrayItems { get; }

        bool IsMap { get; }
        IEnumerable<KeyValuePair<string, IValueNode>> ObjectItems { get; }

        int Count { get; }
        int ValueIndex { get; }

        //object GetValue();
        bool GetBoolean();
    }

    public static class IValueNodeExtensions
    {
        public static IEnumerable<IValueNode> Path<T>(this T self) where T : IValueNode
        {
            if (self.HasParent)
            {
                foreach (var x in self.Parent.Path())
                {
                    yield return x;
                }
            }
            yield return self;
        }

        public static int IndexOf<T>(this T self, T child) where T : IValueNode
        {
            int i = 0;
            foreach (var v in self.ArrayItems)
            {
                if (v.ValueIndex == child.ValueIndex)
                {
                    return i;
                }
                ++i;
            }
            throw new KeyNotFoundException();
        }

        public static string KeyOf<T>(this T self, T node) where T : IValueNode
        {
            foreach (var kv in self.ObjectItems)
            {
                if (node.ValueIndex == kv.Value.ValueIndex)
                {
                    return kv.Key;
                }
            }
            throw new KeyNotFoundException();
        }
    }
}
