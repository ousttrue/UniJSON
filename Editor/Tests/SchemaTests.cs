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
            [JsonSchemaProperty(Required = true)]
            public string firstName;

            [JsonSchemaProperty(Required = true)]
            public string lastName;

            [JsonSchemaProperty(Description = "Age in years", Minimum = 0)]
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
        public void Gltf_Accessor()
        {
            var path = Path.GetFullPath(Application.dataPath + "/../glTF/specification/2.0/schema/accessor.schema.json");
            var fromSchema = JsonSchema.ParseFromPath(path);
            Assert.AreEqual("Accessor", fromSchema.Title);
            Assert.AreEqual("object", fromSchema.Type);
            Assert.AreEqual("integer", fromSchema.Properties["bufferView"].Type);
            Assert.AreEqual("integer", fromSchema.Properties["byteOffset"].Type);

            var fromClass = JsonSchema.Create<UniGLTF.glTFAccessor>();
            Assert.True(fromSchema.MatchProperties(fromClass));
        }
    }
}
