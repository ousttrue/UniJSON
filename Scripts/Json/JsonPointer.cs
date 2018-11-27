using System;
using System.Linq;
using System.Text;


namespace UniJSON
{
    public struct JsonPointer
    {
        public ArraySegment<String> Path
        {
            get;
            private set;
        }

        public string this[int index]
        {
            get
            {
                return Path.Array[Path.Offset + index];
            }
        }

        public JsonPointer Unshift()
        {
            return new JsonPointer
            {
                Path = new ArraySegment<string>(Path.Array, Path.Offset + 1, Path.Count - 1)
            };
        }

        public JsonPointer(IValueNode node)
        {
            Path = new ArraySegment<string>(node.Path().Skip(1).Select(x => GetKeyFromParent(x)).ToArray());
        }

        public JsonPointer(string pointer)
        {
            if (!pointer.StartsWith("/"))
            {
                throw new ArgumentException();
            }
            var splited = pointer.Split('/');
            Path = new ArraySegment<string>(splited, 1, splited.Length - 1);
        }

        public override string ToString()
        {
            if (Path.Count == 0)
            {
                return "/";
            }

            var sb = new StringBuilder();
            var end = Path.Offset + Path.Count;
            for (int i = Path.Offset; i < end; ++i)
            {
                sb.Append('/');
                sb.Append(Path.Array[i]);
            }
            return sb.ToString();
        }

        static string GetKeyFromParent(IValueNode json)
        {
            var parent = json.Parent;
            if (parent.IsArray)
            {
                return parent.IndexOf(json).ToString();
            }
            else if (parent.IsMap)
            {
                return parent.KeyOf(json);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
