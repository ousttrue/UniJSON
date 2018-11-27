using NUnit.Framework;
using System;
using System.Collections.Generic;


namespace UniJSON
{
    public class RPCTests
    {
        class RpcDispatcher
        {
            delegate void Callback(IValueNode args, IFormatter f);
            Dictionary<string, Callback> m_map = new Dictionary<string, Callback>();

            public void Register<A0>(string method, Action<A0> action)
            {
                throw new NotImplementedException();
            }

            public void Register<A0, A1>(string method, Action<A0, A1> action)
            {
                throw new NotImplementedException();
            }

            public void Register<A0, A1, R>(string method, Func<A0, A1, R> action)
            {
                m_map.Add(method, (args, f) =>
                {
                    var it = args.ArrayItems.GetEnumerator();

                    var a0 = default(A0);
                    it.MoveNext();
                    it.Current.Deserialize(ref a0);

                    var a1 = default(A1);
                    it.MoveNext();
                    it.Current.Deserialize(ref a1);

                    var r = action(a0, a1);
                    f.Serialize(r);
                });
            }

            public void Call(string method, IValueNode args, IFormatter f = null)
            {
                Callback callback;
                if (!m_map.TryGetValue(method, out callback))
                {
                    throw new KeyNotFoundException();
                }
                callback(args, f);
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
            dispatcher.Register("add", (int a, int b) => a + b);

            var rpc = new JsonRpc();
            var request = rpc.Request("add", 1, 2);

            var f = new JsonFormatter();
            var parsed = JsonParser.Parse(new Utf8String(request));
            dispatcher.Call(parsed["method"].GetString(), parsed["params"], f);

            Assert.AreEqual("3", new Utf8String(f.GetStore().Bytes));
        }
    }
}
