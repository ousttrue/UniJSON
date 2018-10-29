using System;

namespace UniJSON.MsgPack
{
    public struct MsgPackNode : IValueNode
    {
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
                throw new NotImplementedException();
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
