using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniJSON
{
    public static class IValueNodeJsonPointerExtensions
    {
        public static JsonPointer<T> Pointer<T>(this T self) where T : IValueNode<T>
        {
            return new JsonPointer<T>(self);
        }

        public static IEnumerable<T> Path<T>(this T self) where T : IValueNode<T>
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

        public static IEnumerable<T> GetNodes<T>(this T self, JsonPointer<T> jsonPointer) where T : IValueNode<T>
        {
            if (jsonPointer.Path.Count == 0)
            {
                yield return self;
                yield break;
            }

            if (self.IsArray())
            {
                // array
                if (jsonPointer[0][0] == '*')
                {
                    // wildcard
                    foreach (var child in self.ArrayItems())
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
                    var child = self.ArrayItems().Skip(index).First();
                    foreach (var childChild in child.GetNodes(jsonPointer.Unshift()))
                    {
                        yield return childChild;
                    }
                }
            }
            else if (self.IsMap())
            {
                // object
                if (jsonPointer[0][0] == '*')
                {
                    // wildcard
                    foreach (var kv in self.ObjectItems())
                    {
                        foreach (var childChild in kv.Value.GetNodes(jsonPointer.Unshift()))
                        {
                            yield return childChild;
                        }
                    }
                }
                else
                {
                    T child;
                    try
                    {
                        child = self.ObjectItems().First(x => x.Key.GetUtf8String() == jsonPointer[0]).Value;
                    }
                    catch (KeyNotFoundException)
                    {
                        // key
                        self.AddKey(jsonPointer[0]);
                        // value
                        self.AddValue(default(ArraySegment<byte>), ValueNodeType.Object);

                        child = self.ObjectItems().First(x => x.Key.GetUtf8String() == jsonPointer[0]).Value;
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

        public static IEnumerable<T> GetNodes<T>(this T self, Utf8String jsonPointer) where T : IValueNode<T>
        {
            return self.GetNodes(new JsonPointer<T>(jsonPointer));
        }
    }
}
