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
            [JsonSchemaAttribute(Required = true)]
            public string firstName;

            [JsonSchemaAttribute(Required = true)]
            public string lastName;

            [JsonSchemaAttribute(Description = "Age in years", Minimum = 0)]
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
            Assert.IsTrue(v.Properties["firstName"].Validator.Required);
            Assert.IsTrue(v.Properties["lastName"].Validator.Required);
        }
    }
}
