using NUnit.Framework;
using UnityEngine;
using System.Linq;
using System.Text;

namespace UniJSON
{
    public class JsonFormatterTests
    {
        [Test]
        public void IndentTest()
        {
            var formatter = new JsonFormatter(2);
            formatter.BeginMap();
            formatter.Key("a"); formatter.Value(1);
            formatter.EndMap();

            var json = formatter.ToString();
            Debug.Log(json);
        }

        [Test]
        public void NullTest()
        {
            var nullbytes = Encoding.UTF8.GetBytes("null");
            var json = new JsonFormatter();
            json.Null();
            Assert.True(json.GetStore().Bytes.ToEnumerable().SequenceEqual(nullbytes));
        }
    }
}
