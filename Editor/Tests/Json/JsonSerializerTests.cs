using NUnit.Framework;


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
        }
    }
}
