using System;
using System.Collections.Generic;

namespace UniJSON.MsgPack
{
    public struct MsgPackNode : IValueNode
    {
        public readonly List<MsgPackValue> Values;
        int m_index;
        public MsgPackValue Value
        {
            get { return Values[m_index]; }
        }
        public IEnumerable<MsgPackNode> Children
        {
            get
            {
                for (int i = 0; i < Values.Count; ++i)
                {
                    if (Values[i].ParentIndex == m_index)
                    {
                        yield return new MsgPackNode(Values, i);
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
        public MsgPackNode Parent
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
                return new MsgPackNode(Values, Value.ParentIndex);
            }
        }

        public MsgPackNode(List<MsgPackValue> values, int index = 0)
        {
            Values = values;
            m_index = index;
        }

        public ArraySegment<Byte> GetBody()
        {
            throw new NotImplementedException();
        }

        public object GetValue()
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean()
        {
            throw new NotImplementedException();
        }

        #region  Collection
        public int Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsArray
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsMap
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsNull
        {
            get
            {
                return Value.Format == MsgPackType.NIL;
            }
        }

        public MsgPackNode this[int i]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public MsgPackNode this[string i]
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        #endregion

    }
}
