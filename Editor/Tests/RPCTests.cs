using NUnit.Framework;


namespace UniJSON
{
    public class RPCTests
    {
        [Test]
        public void RPCTest()
        {
            var rpc = new JsonRpc();

            {
                var request = rpc.Request("num", 1);
                Assert.AreEqual(
                    JsonParser.Parse("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"num\",\"params\":[1]}"),
                    JsonParser.Parse(new Utf8String(request)));
            }
            {
                var request = rpc.Request("num", 2, true);
                Assert.AreEqual(
                    JsonParser.Parse("{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"num\",\"params\":[2,true]}"),
                    JsonParser.Parse(new Utf8String(request)));
            }
            {
                var request = rpc.Request("num",
                    3, true, "abc", false, (string)null, new[] { 1, 2 });
                Assert.AreEqual(
                    JsonParser.Parse("{\"jsonrpc\":\"2.0\",\"id\":3,\"method\":\"num\",\"params\":[3,true,\"abc\",false,null,[1,2]]}"),
                    JsonParser.Parse(new Utf8String(request)));
            }
        }
    }
}
