using NUnit.Framework;
using UniJSON;


public class SchemaTests
{
    /// <summary>
    /// http://json-schema.org/examples.html
    /// </summary>
    public class Person
    {
        [JSONSchema(true)]
        public string firstName;

        [JSONSchema(true)]
        public string lastName;

        [JSONSchema(false, "Age in years", 0)]
        public int age;
    }

    [Test]
    public void Tests()
    {
        var s = JSONSchema.Create<Person>();
        Assert.AreEqual("Person", s.Title);
        Assert.AreEqual("object", s.Type);
        Assert.AreEqual("string", s.Properties["firstName"].Type);
        Assert.AreEqual("string", s.Properties["lastName"].Type);
        Assert.AreEqual("integer", s.Properties["age"].Type);
        Assert.AreEqual("Age in years", s.Properties["age"].Description);
        Assert.AreEqual(0, s.Properties["age"].Minimum);
        Assert.AreEqual(new[] { "firstName", "lastName" }, s.Required);
    }
}
