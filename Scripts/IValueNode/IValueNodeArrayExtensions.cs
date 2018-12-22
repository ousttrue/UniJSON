using System.Collections.Generic;
using System.Linq;


namespace UniJSON
{
    public static class IValueNodeArrayExtensions
    {
        public static IEnumerable<T> ArrayItems<T>(this T self) where T : IValueNode<T>
        {
            if (!self.IsArray()) throw new JsonValueException("is not array");
            return self.Children;
        }

        public static T GetArrrayItem<T>(this T self, int index) where T : IValueNode<T>
        {
            int i = 0;
            foreach (var v in self.ArrayItems())
            {
                if (i++ == index)
                {
                    return v;
                }
            }
            throw new KeyNotFoundException();
        }

        public static int GetArrayCount<T>(this T self) where T : IValueNode<T>
        {
            if (!self.IsArray()) throw new JsonValueException("is not array");
            return self.Children.Count();
        }

        public static int IndexOf<T>(this T self, T child) where T : IValueNode<T>
        {
            int i = 0;
            foreach (var v in self.ArrayItems())
            {
                if (v.ValueIndex == child.ValueIndex)
                {
                    return i;
                }
                ++i;
            }
            throw new KeyNotFoundException();
        }
    }
}
