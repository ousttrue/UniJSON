using NUnit.Framework;
using System;


namespace UniJSON
{
    public class TimeTests
    {
        [Test]
        public void Time32Test()
        {
            var time = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

            var f = new MsgPackFormatter();
            f.Time32(time);

            var bytes = f.GetStoreBytes().ArrayOrCopy();
            unchecked
            {
                Assert.AreEqual(new byte[]
                {
                0xd6, (byte)-1, 0, 0, 0, 0
                }, bytes);
            }
        }
    }
}
