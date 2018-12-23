using NUnit.Framework;


namespace UniJSON.Toml
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

            {
                var result = TomlParser.Parse(@"
[table]
value = 1
");
                Assert.True(result.IsMap());
                Assert.AreEqual(1, result["table"]["value"].GetInt32());
            }
        }
    }
}
