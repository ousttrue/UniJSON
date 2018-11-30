using NUnit.Framework;
using System;
using System.Collections.Generic;


namespace UniJSON
{
    public class RPCTests
    {
        [Test]
        public void RequestTest()
        {
            var rpc = new JsonRpc();
            {
                var l = JsonParser.Parse("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"num1\",\"params\":[1]}");
                var request = rpc.Request("num1", 1);
                var u = new Utf8String(request);
                var r = JsonParser.Parse(u);
                Assert.AreEqual(
                    l,
                    r);
            }
            {
                var request = rpc.Request("num2", 2, true);
                Assert.AreEqual(
                    JsonParser.Parse("{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"num2\",\"params\":[2,true]}"),
                    JsonParser.Parse(new Utf8String(request)));
            }
            {
                var request = rpc.Request("num3",
                    3, true, "abc", false, (string)null, new[] { 1, 2 });
                Assert.AreEqual(
                    JsonParser.Parse("{\"jsonrpc\":\"2.0\",\"id\":3,\"method\":\"num3\",\"params\":[3,true,\"abc\",false,null,[1,2]]}"),
                    JsonParser.Parse(new Utf8String(request)));
            }
        }

        [Test]
        public void DispatcherTest()
        {
            var dispatcher = new RpcDispatcher();
            var rpc = new JsonRpc();

            {
                dispatcher.Register("add", (int a, int b) => a + b);
                var request = rpc.Request("add", 1, 2);

                var parsed = JsonParser.Parse(new Utf8String(request));
                var f = new JsonFormatter();
                dispatcher.Call(parsed["method"].GetString(), parsed["params"], f);

                Assert.AreEqual(Utf8String.From("3"), new Utf8String(f.GetStore().Bytes));
            }

            {
                string msg = null;
                dispatcher.Register("print", (string _msg) => { msg = _msg; });
                var request = rpc.Request("print", "hoge");

                var parsed = JsonParser.Parse(new Utf8String(request));
                var f = new JsonFormatter();
                dispatcher.Call(parsed["method"].GetString(), parsed["params"], f);

                Assert.AreEqual("hoge", msg);
            }
        }
    }
}
