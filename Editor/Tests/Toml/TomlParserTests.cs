using NUnit.Framework;


namespace UniJSON
{
    class TomlParserTests
    {
        [Test]
        public void Tests()
        {
            {
                var result = TomlParser.Parse(@"
value = 1
");
                Assert.True(result.IsMap());
                Assert.AreEqual(1, result["value"].GetInt32());
            }
        }
    }
}
