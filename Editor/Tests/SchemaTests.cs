using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text;
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
            [JsonSchemaPropertyAttribute(Required = true)]
            public string firstName;

            [JsonSchemaPropertyAttribute(Required = true)]
            public string lastName;

            [JsonSchemaPropertyAttribute(Description = "Age in years", Minimum = 0)]
            public int age;
        }

        [Test]
        public void CreateFromClass()
        {
            var s = JsonSchema.Create<Person>();
            Assert.AreEqual("Person", s.Title);
            Assert.AreEqual("object", s.Type);
            Assert.AreEqual("string", s.Properties["firstName"].Type);
            Assert.AreEqual("string", s.Properties["lastName"].Type);
            Assert.AreEqual("integer", s.Properties["age"].Type);
            Assert.AreEqual("Age in years", s.Properties["age"].Description);
            Assert.AreEqual(0, s.Properties["age"].Minimum);
            Assert.AreEqual(new[] { "firstName", "lastName" }, s.Required);
        }

        [Test]
        public void CreateFromJSON()
        {
            var path = Path.GetFullPath(Application.dataPath + "/../glTF/specification/2.0/schema/accessor.schema.json");
            var s = JsonSchema.ParseFromPath(path);
            Assert.AreEqual("Accessor", s.Title);
            Assert.AreEqual("object", s.Type);
            Assert.AreEqual("integer", s.Properties["bufferView"].Type);
            Assert.AreEqual("integer", s.Properties["byteOffset"].Type);
            //Assert.True(s.Required.);
        }
    }
}
