using NUnit.Framework;


namespace UniJSON
{
    public class Utf8StringTests
    {
        [Test]
        public void Utf8StringTest()
        {
            var abc = Utf8String.FromString("abc");
            var ab = Utf8String.FromString("ab");
            var bc = Utf8String.FromString("bc");

            Assert.True(abc.StartsWith(ab));
            Assert.False(ab.StartsWith(abc));

            Assert.True(abc.EndsWith(bc));
            Assert.False(bc.EndsWith(abc));

            Assert.AreEqual(Utf8String.FromString("abbc"), ab.Concat(bc));

            Assert.AreEqual(2, abc.IndexOf((byte)'c'));
        }
    }
}
