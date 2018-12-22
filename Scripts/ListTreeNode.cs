using System.Collections.Generic;


namespace UniJSON
{
    public struct ListTreeNode<T> where T : ITreeItem
    {
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
        public readonly int ValueIndex;

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

        public IEnumerable<int> Children
        {
            get
            {
                for (int i = 0; i < Values.Count; ++i)
                {
                    if (Values[i].ParentIndex == ValueIndex)
                    {
                        yield return i;
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

        public ListTreeNode(List<T> values, int index = 0)
        {
            Values = values;
            ValueIndex = index;
        }
    }
}
