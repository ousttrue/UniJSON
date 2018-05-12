using NUnit.Framework;
using UniJSON;


public class ParserTests
{
    [Test]
    public void Tests()
    {
        var result = JSONParser.Parse("1");
        Assert.AreEqual(1, result[0].GetInt32());
    }
}
