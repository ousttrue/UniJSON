using NUnit.Framework;
using UniJson;


public class ParserTests
{
    [Test]
    public void Tests()
    {
        var result = JsonParser.Parse("1");
        Assert.AreEqual(1, result[0].GetInt32());
    }
}
