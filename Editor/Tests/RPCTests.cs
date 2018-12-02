using NUnit.Framework;


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
    }
}
