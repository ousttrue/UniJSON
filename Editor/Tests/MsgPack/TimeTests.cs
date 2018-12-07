using NUnit.Framework;
using System;


namespace UniJSON.MsgPack
{
    public class TimeTests
    {
        [Test]
        public void TimeTest()
        {
            var time = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

            var f = new MsgPackFormatter();
            f.Value(time);

            var bytes = f.GetStoreBytes().ArrayOrCopy();
            unchecked
            {
                Assert.AreEqual(new byte[]
                {
                0xd6, (byte)-1, 0, 0, 0, 0
                }, bytes);
            }

            Assert.AreEqual(1544235135, new DateTimeOffset(2018, 12, 8, 2, 12, 15, TimeSpan.Zero).ToUnixTimeSeconds());

            f.GetStore().Clear();
            Assert.Catch(() =>
            {
                f.Value(new DateTimeOffset());
            });
        }
    }
}
