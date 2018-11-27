using NUnit.Framework;
using System;

namespace UniJSON
{
    public class RPCTests
    {
        class RpcDispatcher
        {
            public void Register<A0>(Action<A0> action)
            {

            }

            public void Call(IValueNode parsed, IFormatter f = null)
            {

            }
        }

        static void Print(string msg)
        {

        }

        static int Add(int a, int b)
        {
            return a + b;
        }

        static string Join(string a, string b, string c)
        {
            return a + "," + b + "," + c;
        }

        [Test]
        public void RequestTest()
        {
            var rpc = new JsonRpc();
            {
                var request = rpc.Request("num1", 1);
                Assert.AreEqual(
                    JsonParser.Parse("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"num1\",\"params\":[1]}"),
                    JsonParser.Parse(new Utf8String(request)));

                
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
            var request = rpc.Request("add", 1, 2);

            var f = new JsonFormatter();
            dispatcher.Call(JsonParser.Parse(new Utf8String(request)), f);

            Assert.AreEqual("3", new Utf8String(f.GetStore().Bytes));
        }
    }
}
