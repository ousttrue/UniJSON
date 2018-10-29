using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace UniJSON.MsgPack
{
    [TestFixture]
    public class MapTest
    {
        [Test]
        public void fix_map()
        {
            var bytes = new MsgPackFormatter()
            .BeginMap(2)
            .Key("0").Value(1)
            .Key("2").Value(3)
            .EndMap()
            .GetStore().Bytes;
            ;

            Assert.AreEqual(new Byte[]{
                0x82, 0x00, 0x01, 0x02, 0x03
            }, bytes.ToEnumerable());

            var value = MsgPackParser.Parse(bytes);

            Assert.AreEqual(2, value.Count);
            Assert.AreEqual(1, value["0"].GetValue());
            Assert.AreEqual(3, value["2"].GetValue());
        }

        [Test]
        public void map16()
        {
            var w = new MsgPackFormatter();
            int size = 18;
            w.BeginMap(size);
            for (int i = 0; i < size; ++i)
            {
                w.Value(i);
                w.Value(i + 5);
            }
            var bytes = w.GetStore().Bytes.ToEnumerable().ToArray();

            Assert.AreEqual(
                new Byte[]{0xde, 0x0, 0x12, 0x0, 0x5, 0x1, 0x6, 0x2, 0x7, 0x3, 0x8, 0x4, 0x9, 0x5, 0xa, 0x6, 0xb, 0x7, 0xc, 0x8, 0xd, 0x9, 0xe, 0xa, 0xf, 0xb, 0x10, 0xc,
0x11, 0xd, 0x12, 0xe, 0x13, 0xf, 0x14, 0x10, 0x15, 0x11, 0x16},
            bytes);

            var value = MsgPackParser.Parse(bytes);

            Assert.AreEqual(15, value[10].GetValue());
        }
    }
}
