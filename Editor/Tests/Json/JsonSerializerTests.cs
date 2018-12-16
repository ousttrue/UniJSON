﻿using NUnit.Framework;
using System;
using System.Collections.Generic;


namespace UniJSON
{
    public class JsonSerializerTests
    {
        struct Point
        {
            public float X;
            public float Y;

            public float[] Vector;

            public override string ToString()
            {
                return string.Format("{{X={0}, Y={1}, {2}}}", X, Y, Vector);
            }
        }

        enum HogeFuga
        {
            Hoge,
            Fuga,
        }

        struct EnumTest
        { 
            public HogeFuga EnumDefault;

            [JsonSchema(EnumSerializationType =EnumSerializationType.AsInt)]
            public HogeFuga EnumAsInt;

            [JsonSchema(EnumSerializationType = EnumSerializationType.AsString)]
            public HogeFuga EnumAsString;

            [JsonSchema(EnumSerializationType = EnumSerializationType.AsLowerString)]
            public HogeFuga EnumAsLowerString;
        }

        #region Serializer
        static void SerializeValue<T>(T value, string json)
        {
            var b = new BytesStore();
            var f = new JsonFormatter(b);

            f.Serialize(value);
            Assert.AreEqual(json, new Utf8String(b.Bytes).ToString());
        }

        [Test]
        public void JsonSerializerTest()
        {
            SerializeValue(1, "1");
            SerializeValue(1.1f, "1.1");
            SerializeValue(1.2, "1.2");
            SerializeValue(true, "true");
            SerializeValue(false, "false");
            SerializeValue("ascii", "\"ascii\"");

            SerializeValue(new[] { 1 }, "[1]");
            SerializeValue(new[] { 1.1f }, "[1.1]");
            SerializeValue(new[] { 1.2 }, "[1.2]");
            SerializeValue(new[] { true, false }, "[true,false]");
            SerializeValue(new[] { "ascii" }, "[\"ascii\"]");
            SerializeValue(new List<int> { 1 }, "[1]");
            //SerializeValue(new object[] { null, 1, "a" }, "[null,1,\"a\"]");

            SerializeValue(new Dictionary<string, object> { }, "{}");
            SerializeValue(new Dictionary<string, object> { { "a", 1 } }, "{\"a\":1}");
            SerializeValue(new Dictionary<string, object> { { "a",
                    new Dictionary<string, object>{
                    } } }, "{\"a\":{}}");

            SerializeValue(new Point { X = 1 }, "{\"X\":1,\"Y\":0}");

            SerializeValue(HogeFuga.Fuga, "1");

            SerializeValue(new EnumTest(), "{\"EnumDefault\":0,\"EnumAsInt\":0,\"EnumAsString\":\"Hoge\",\"EnumAsLowerString\":\"hoge\"}");
        }

        [Test]
        public void KeyValue()
        {
            var p = new Point
            {
                X = 1,
                Vector = new float[] { 1, 2, 3 }
            };

            var f = new JsonFormatter();
            f.BeginMap();
            f.KeyValue(() => p.Vector);
            f.EndMap();

            var json = JsonParser.Parse(new Utf8String(f.GetStoreBytes()));

            Assert.AreEqual(1, json.ValueCount);
            Assert.AreEqual(1, json["Vector"][0].GetInt32());
        }
        #endregion

        #region Deserialize
        static void DeserializeValue<T>(T value, string json)
        {
            var parsed = JsonParser.Parse(json);

            var t = default(T);
            parsed.Deserialize(ref t);

            Assert.AreEqual(value, t);
        }

        [Test]
        public void JsonDeserializerTest()
        {
            DeserializeValue(1, "1");
            DeserializeValue(1.1f, "1.1");
            DeserializeValue(1.2, "1.2");
            DeserializeValue(true, "true");
            DeserializeValue(false, "false");
            DeserializeValue("ascii", "\"ascii\"");

            DeserializeValue(new[] { 1 }, "[1]");
            DeserializeValue(new[] { 1.1f }, "[1.1]");
            DeserializeValue(new[] { 1.2 }, "[1.2]");
            DeserializeValue(new[] { true, false }, "[true,false]");
            DeserializeValue(new[] { "ascii" }, "[\"ascii\"]");
            DeserializeValue(new List<int> { 1 }, "[1]");
            //DeserializeValue(new object[] { null, 1, "a" }, "[null,1,\"a\"]");

            DeserializeValue(new Dictionary<string, object> { }, "{}");
            DeserializeValue(new Dictionary<string, object> { { "a", 1 } }, "{\"a\":1}");
            DeserializeValue(new Dictionary<string, object> { { "a",
                    new Dictionary<string, object>{
                    } } }, "{\"a\":{}}");

            DeserializeValue(new Point { X = 1 }, "{\"X\":1,\"Y\":0}");

            DeserializeValue(HogeFuga.Fuga, "1");

            DeserializeValue(new EnumTest(), "{\"EnumDefault\":0,\"EnumAsInt\":0,\"EnumAsString\":\"Hoge\",\"EnumAsLowerString\":\"hoge\"}");
        }
        #endregion
    }
}
