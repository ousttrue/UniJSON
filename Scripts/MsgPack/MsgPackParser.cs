using System;

namespace UniJSON.MsgPack
{
    public static class MsgPackParser
    {
        public static MsgPackNode Parse(ArraySegment<Byte> bytes)
        {
            throw new NotImplementedException();
        }

        public static MsgPackNode Parse(Byte[] bytes)
        {
            return Parse(new ArraySegment<byte>(bytes));
        }
    }
}