using System;
using UniJSON;

namespace UniJSON.MsgPack
{
    public class MsgPackFormatter : IFormatter
    {
        public static ArraySegment<Byte> Format<T>(T[] values)
        {
            var f = new MsgPackFormatter();
            f.Value(values);
            return f.GetStore().Bytes;
        }

        public MsgPackFormatter()
        {

        }

        public IStore GetStore()
        {
            throw new NotImplementedException();
        }

        public IFormatter BeginList(int n)
        {
            throw new NotImplementedException();
        }

        public IFormatter EndList()
        {
            throw new NotImplementedException();
        }

        public IFormatter BeginMap(int n)
        {
            throw new NotImplementedException();
        }

        public IFormatter EndMap()
        {
            throw new NotImplementedException();
        }

        public IFormatter Key(string x)
        {
            throw new NotImplementedException();
        }

        public IFormatter Null()
        {
            throw new NotImplementedException();
        }

        public IFormatter Value(string x)
        {
            throw new NotImplementedException();
        }

        public IFormatter Value(bool x)
        {
            throw new NotImplementedException();
        }

        public IFormatter Value(byte x)
        {
            throw new NotImplementedException();
        }

        public IFormatter Value(ushort x)
        {
            throw new NotImplementedException();
        }

        public IFormatter Value(uint x)
        {
            throw new NotImplementedException();
        }

        public IFormatter Value(ulong x)
        {
            throw new NotImplementedException();
        }

        public IFormatter Value(sbyte x)
        {
            throw new NotImplementedException();
        }

        public IFormatter Value(short x)
        {
            throw new NotImplementedException();
        }

        public IFormatter Value(int x)
        {
            throw new NotImplementedException();
        }

        public IFormatter Value(long x)
        {
            throw new NotImplementedException();
        }

        public IFormatter Value(float x)
        {
            throw new NotImplementedException();
        }

        public IFormatter Value(double x)
        {
            throw new NotImplementedException();
        }
    }
}