using NUnit.Framework;
using System.IO;
using UnityEngine;


namespace UniJSON
{
    public class SchemaTests
    {
        /// <summary>
        /// http://json-schema.org/examples.html
        /// </summary>
        public class Person
        {
            [JsonSchema(Required = true)]
            public string firstName;

            [JsonSchema(Required = true)]
            public string lastName;

            [JsonSchema(Description = "Age in years", Minimum = 0)]
            public int age;
        }

        [Test]
        public void CreateFromClass()
        {
            var s = JsonSchema.FromType<Person>();
            Assert.AreEqual("Person", s.Title);

            var v = s.Validator as JsonObjectValidator;
            Assert.AreEqual(JsonValueType.Object, v.JsonValueType);
            Assert.AreEqual(JsonValueType.String, v.Properties["firstName"].Validator.JsonValueType);
            Assert.AreEqual(JsonValueType.String, v.Properties["lastName"].Validator.JsonValueType);
            Assert.AreEqual(JsonValueType.Integer, v.Properties["age"].Validator.JsonValueType);
            Assert.AreEqual("Age in years", v.Properties["age"].Description);
            //Assert.AreEqual(0, s.Properties["age"].Minimum);
            Assert.AreEqual(new[] { "firstName", "lastName" }, v.Required);
        }

        public enum ProjectionType
        {
            Perspective,
            Orthographic
        }

        class EnumStringTest
        {
            [JsonSchema(EnumSerializationType =EnumSerializationType.AsLowerString)]
            public ProjectionType type;
        }

        class EnumIntTest
        {
            [JsonSchema(EnumSerializationType = EnumSerializationType.AsInt)]
            public ProjectionType type;
        }

        [Test]
        public void TestEnumAsString()
        {
            var json = @"
{
    ""type"": ""object"",
    ""properties"": {

        ""type"": {

            ""anyOf"": [
            {
                ""enum"": [ ""perspective"" ]
            },
            {
                ""enum"": [ ""orthographic"" ]
            },
            {
                ""type"": ""string""
            }
            ]

        }
        
    }
}
";

            var fromJson = new JsonSchema();
            fromJson.Parse(null, JsonParser.Parse(json), "enum test");

            var fromType = JsonSchema.FromType<EnumStringTest>();

            Assert.AreEqual(fromJson, fromType);
        }

        [Test]
        public void TestEnumAsInt()
        {
            var json = @"
{
    ""type"": ""object"",
    ""properties"": {

        ""type"": {

            ""anyOf"": [
            {
                ""enum"": [ 0 ]
            },
            {
                ""enum"": [ 1 ]
            },
            {
                ""type"": ""integer""
            }
            ]

        }
        
    }
}
";

            var fromJson = new JsonSchema();
            fromJson.Parse(null, JsonParser.Parse(json), "enum test");

            var fromType = JsonSchema.FromType<EnumIntTest>();

            Assert.AreEqual(fromJson, fromType);
        }

    }
}
