using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniJSON
{
    public static class IValueNodeObjectExtensions
    {
        public static IEnumerable<KeyValuePair<T, T>> ObjectItems<T>(this T self) where T : IValueNode<T>
        {
            if (!self.IsMap()) throw new DeserializationException("is not object");
            var it = self.Children.GetEnumerator();
            while (it.MoveNext())
            {
                var key = it.Current;

                it.MoveNext();
                yield return new KeyValuePair<T, T>(key, it.Current);
            }
        }

        public static int GetObjectCount<T>(this T self) where T : IValueNode<T>
        {
            if (!self.IsMap()) throw new DeserializationException("is not object");
            return self.Children.Count() / 2;
        }

        public static T GetObjectItem<T>(this T self, String key) where T : IValueNode<T>
        {
            return self.GetObjectItem(Utf8String.From(key));
        }

        public static T GetObjectItem<T>(this T self, Utf8String key) where T : IValueNode<T>
        {
            foreach (var kv in self.ObjectItems())
            {
                if (kv.Key.GetUtf8String() == key)
                {
                    return kv.Value;
                }
            }
            throw new KeyNotFoundException();
        }

        public static bool ContainsKey<T>(this T self, Utf8String key) where T : IValueNode<T>
        {
            return self.ObjectItems().Any(x => x.Key.GetUtf8String() == key);
        }

        public static bool ContainsKey<T>(this T self, String key) where T : IValueNode<T>
        {
            var ukey = Utf8String.From(key);
            return self.ContainsKey(ukey);
        }

        public static Utf8String KeyOf<T>(this T self, T node) where T : IValueNode<T>
        {
            foreach (var kv in self.ObjectItems())
            {
                if (node.ValueIndex == kv.Value.ValueIndex)
                {
                    return kv.Key.GetUtf8String();
                }
            }
            throw new KeyNotFoundException();
        }
    }
}
