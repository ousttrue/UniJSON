using NUnit.Framework;
using System;


namespace UniJSON
{
    public class RPCTests
    {
        class JsonRpc
        {
            int m_nextRequestId = 1;

            public ArraySegment<Byte> Request<A0>(string method, A0 a0)
            {
                var f = new JsonFormatter();
                f.BeginMap();
                f.Key("jsonrpc"); f.Value("2.0");
                f.Key("id"); f.Value(m_nextRequestId++);
                f.Key("method"); f.Value(method);
                f.Key("params"); f.BeginList();
                f.Serialize(a0);
                f.EndList();
                f.EndMap();
                return f.GetStore().Bytes;
            }
        }

        [Test]
        public void RPCTest()
        {
            var rpc = new JsonRpc();

            var request = rpc.Request("num", 1);

            Assert.AreEqual(JsonParser.Parse("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"num\",\"params\":[1]}"), JsonParser.Parse(new Utf8String(request)));
        }
    }
}
