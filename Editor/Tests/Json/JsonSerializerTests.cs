using NUnit.Framework;
using System.Collections.Generic;


namespace UniJSON
{
    public class JsonSerializerTests
    {
        static void SerializeValue<T>(T value, string json)
        {
            var b = new BytesStore();
            var f = new JsonFormatter(b);

            f.Serialize(value);
            Assert.AreEqual(json, new Utf8String(b.Bytes).ToString());
        }

        [Test]
        public void JsonSerializerTest()
        {
            SerializeValue(1, "1");
            SerializeValue(1.1f, "1.1");
            SerializeValue(1.2, "1.2");
            SerializeValue(true, "true");
            SerializeValue(false, "false");
            SerializeValue("ascii", "\"ascii\"");

            SerializeValue(new[] { 1 }, "[1]");
            SerializeValue(new[] { 1.1f }, "[1.1]");
            SerializeValue(new[] { 1.2 }, "[1.2]");
            SerializeValue(new[] { true, false }, "[true,false]");
            SerializeValue(new[] { "ascii" }, "[\"ascii\"]");
            SerializeValue(new List<int> { 1 }, "[1]");
            //SerializeValue(new object[] { null, 1, "a" }, "[null,1,\"a\"]");

            SerializeValue(new Dictionary<string, object> { }, "{}");
            SerializeValue(new Dictionary<string, object> { {"a", 1} }, "{\"a\":1}");
            SerializeValue(new Dictionary<string, object> { { "a",
                    new Dictionary<string, object>{
                    } } }, "{\"a\":{}}");
        }
    }
}
