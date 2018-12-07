using System;

namespace UniJSON
{
    public struct MsgPackValue
    {
        public ArraySegment<Byte> Segment;
        public int ParentIndex;

        public MsgPackType Format
        {
            get
            {
                return (MsgPackType)Segment.Get(0);
            }
        }

        public MsgPackValue(ArraySegment<Byte> segment, int parentIndex)
        {
            Segment = segment;
            ParentIndex = parentIndex;
        }
    }
}
