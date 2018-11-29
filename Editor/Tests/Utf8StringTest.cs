using NUnit.Framework;
using System.Linq;


namespace UniJSON
{
    public class Utf8StringTests
    {
        [Test]
        public void Utf8StringTest()
        {
            var abc = Utf8String.From("abc");
            var ab = Utf8String.From("ab");
            var bc = Utf8String.From("bc");

            Assert.True(abc.StartsWith(ab));
            Assert.False(ab.StartsWith(abc));

            Assert.True(abc.EndsWith(bc));
            Assert.False(bc.EndsWith(abc));

            Assert.AreEqual(Utf8String.From("abbc"), ab.Concat(bc));

            Assert.AreEqual(2, abc.IndexOf((byte)'c'));
        }

        [Test]
        public void ShortUtf8Test()
        {
            var a0 = Utf8String4.Create("a");
            Assert.AreEqual("a", a0);
            var a1 = Utf8String4.Create(new byte[] { (byte)'a', 0x00 });
            Assert.AreEqual(a0, a1);
            var a2 = Utf8String4.Create("漢");
            Assert.AreEqual(3, a2.ByteLength);
        }

        [Test]
        public void QuoteTest()
        {
            {
                var value = Utf8String.From("ho日本語ge");
                var quoted = Utf8String.From("\"ho日本語ge\"");
                Assert.AreEqual(quoted, JsonString.Quote(value));
                Assert.AreEqual(value, JsonString.Unquote(quoted));
            }

            {
                var value = Utf8String.From("fuga\n  ho日本語ge");
                var quoted = Utf8String.From("\"fuga\\n  ho日本語ge\"");
                Assert.AreEqual(quoted, JsonString.Quote(value));
                Assert.AreEqual(value, JsonString.Unquote(quoted));
            }
        }

        [Test]
        public void SplitTest()
        {
            {
                var value = Utf8String.From("a/b/c");
                var splited = value.Split((byte)'/').ToArray();
                Assert.AreEqual(3, splited.Length);
                Assert.AreEqual(splited[0], Utf8String.From("a"));
                Assert.AreEqual(splited[1], Utf8String.From("b"));
                Assert.AreEqual(splited[2], Utf8String.From("c"));
            }
            {
                var value = Utf8String.From("/a/b/c/");
                var splited = value.Split((byte)'/').ToArray();
                Assert.AreEqual(4, splited.Length);
                Assert.AreEqual(splited[0], Utf8String.From(""));
                Assert.AreEqual(splited[1], Utf8String.From("a"));
                Assert.AreEqual(splited[2], Utf8String.From("b"));
                Assert.AreEqual(splited[3], Utf8String.From("c"));
            }
        }

        [Test]
        public void AtoiTest()
        {
            Assert.AreEqual(1234, Utf8String.From("1234").ToInt32());
        }
    }
}
