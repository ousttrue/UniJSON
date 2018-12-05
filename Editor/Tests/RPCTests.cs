using NUnit.Framework;
using UniJSON.MsgPack;

namespace UniJSON
{
    public class RPCTests
    {
        [Test]
        public void JsonRpcRequestTest()
        {
            var f = new JsonFormatter();

            {
                f.Clear();
                var l = JsonParser.Parse("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"num1\",\"params\":[1]}");
                f.Request("num1", 1);
                var u = new Utf8String(f.GetStore().Bytes);
                var r = JsonParser.Parse(u);
                Assert.AreEqual(
                    l,
                    r);
            }
            {
                f.Clear();
                f.Request("num2", 2, true);
                Assert.AreEqual(
                    JsonParser.Parse("{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"num2\",\"params\":[2,true]}"),
                    JsonParser.Parse(new Utf8String(f.GetStore().Bytes)));
            }
            {
                f.Clear();
                f.Request("num3",
                    3, true, "abc", false, (string)null, new[] { 1, 2 });
                Assert.AreEqual(
                    JsonParser.Parse("{\"jsonrpc\":\"2.0\",\"id\":3,\"method\":\"num3\",\"params\":[3,true,\"abc\",false,null,[1,2]]}"),
                    JsonParser.Parse(new Utf8String(f.GetStore().Bytes)));
            }
        }

        [Test]
        public void JsonRpcDispatcherTest()
        {
            var dispatcher = new RpcDispatcher();
            var f = new JsonFormatter();

            {
                f.Clear();
                dispatcher.Register("add", (int a, int b) => a + b);
                f.Request("add", 1, 2);

                var parsed = JsonParser.Parse(new Utf8String(f.GetStore().Bytes));

                f.Clear();
                dispatcher.Call(f, parsed["id"].GetInt32(), parsed["method"].GetString(), parsed["params"]);
                var response = JsonParser.Parse(new Utf8String(f.GetStore().Bytes));
                Assert.AreEqual(3, response["result"].GetInt32());
            }

            {
                string msg = null;
                dispatcher.Register("print", (string _msg) => { msg = _msg; });
                f.Clear();
                f.Request("print", "hoge");

                var parsed = JsonParser.Parse(new Utf8String(f.GetStore().Bytes));
                f.Clear();
                dispatcher.Call(f, parsed["id"].GetInt32(), parsed["method"].GetString(), parsed["params"]);

                Assert.AreEqual("hoge", msg);
            }
        }

        [Test]
        public void MsgPackRpcDispatcherTest()
        {
            var dispatcher = new RpcDispatcher();
            var f = new MsgPackFormatter();

            {
                f.Clear();
                dispatcher.Register("add", (int a, int b) => a + b);
                f.Request("add", 1, 2);

                var request = MsgPackParser.Parse(f.GetStore().Bytes);
                Assert.AreEqual(4, request.ValueCount);
                Assert.AreEqual(MsgPackFormatter.REQUEST_TYPE, request[0].GetInt32());

                f.Clear();
                dispatcher.Call(f, request[1].GetInt32(), request[2].GetString(), request[3]);
                var response = MsgPackParser.Parse(f.GetStore().Bytes);
                Assert.AreEqual(4, response.ValueCount);
                Assert.AreEqual(MsgPackFormatter.RESPONSE_TYPE, response[0].GetInt32());
                Assert.True(response[2].IsNull());
                Assert.AreEqual(3, response[3].GetInt32());
            }
        }
    }
}
