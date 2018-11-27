using System;


namespace UniJSON
{
    public class JsonRpc
    {
        int m_nextRequestId = 1;

        public ArraySegment<Byte> Request<A0>(string method,
            A0 a0)
        {
            var f = new JsonFormatter();
            f.BeginMap();
            f.Key("jsonrpc"); f.Value("2.0");
            f.Key("id"); f.Value(m_nextRequestId++);
            f.Key("method"); f.Value(method);
            f.Key("params"); f.BeginList();
            {
                f.Serialize(a0);
            }
            f.EndList();
            f.EndMap();
            return f.GetStore().Bytes;
        }

        public ArraySegment<Byte> Request<A0, A1>(string method,
            A0 a0, A1 a1)
        {
            var f = new JsonFormatter();
            f.BeginMap();
            f.Key("jsonrpc"); f.Value("2.0");
            f.Key("id"); f.Value(m_nextRequestId++);
            f.Key("method"); f.Value(method);
            f.Key("params"); f.BeginList();
            {
                f.Serialize(a0);
                f.Serialize(a1);
            }
            f.EndList();
            f.EndMap();
            return f.GetStore().Bytes;
        }

        public ArraySegment<Byte> Request<A0, A1, A2>(string method,
            A0 a0, A1 a1, A2 a2)
        {
            var f = new JsonFormatter();
            f.BeginMap();
            f.Key("jsonrpc"); f.Value("2.0");
            f.Key("id"); f.Value(m_nextRequestId++);
            f.Key("method"); f.Value(method);
            f.Key("params"); f.BeginList();
            {
                f.Serialize(a0);
                f.Serialize(a1);
                f.Serialize(a2);
            }
            f.EndList();
            f.EndMap();
            return f.GetStore().Bytes;
        }

        public ArraySegment<Byte> Request<A0, A1, A2, A3>(string method,
            A0 a0, A1 a1, A2 a2, A3 a3)
        {
            var f = new JsonFormatter();
            f.BeginMap();
            f.Key("jsonrpc"); f.Value("2.0");
            f.Key("id"); f.Value(m_nextRequestId++);
            f.Key("method"); f.Value(method);
            f.Key("params"); f.BeginList();
            {
                f.Serialize(a0);
                f.Serialize(a1);
                f.Serialize(a2);
                f.Serialize(a3);
            }
            f.EndList();
            f.EndMap();
            return f.GetStore().Bytes;
        }

        public ArraySegment<Byte> Request<A0, A1, A2, A3, A4>(string method,
            A0 a0, A1 a1, A2 a2, A3 a3, A4 a4)
        {
            var f = new JsonFormatter();
            f.BeginMap();
            f.Key("jsonrpc"); f.Value("2.0");
            f.Key("id"); f.Value(m_nextRequestId++);
            f.Key("method"); f.Value(method);
            f.Key("params"); f.BeginList();
            {
                f.Serialize(a0);
                f.Serialize(a1);
                f.Serialize(a2);
                f.Serialize(a3);
                f.Serialize(a4);
            }
            f.EndList();
            f.EndMap();
            return f.GetStore().Bytes;
        }

        public ArraySegment<Byte> Request<A0, A1, A2, A3, A4, A5>(string method,
            A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5)
        {
            var f = new JsonFormatter();
            f.BeginMap();
            f.Key("jsonrpc"); f.Value("2.0");
            f.Key("id"); f.Value(m_nextRequestId++);
            f.Key("method"); f.Value(method);
            f.Key("params"); f.BeginList();
            {
                f.Serialize(a0);
                f.Serialize(a1);
                f.Serialize(a2);
                f.Serialize(a3);
                f.Serialize(a4);
                f.Serialize(a5);
            }
            f.EndList();
            f.EndMap();
            return f.GetStore().Bytes;
        }
    }
}
