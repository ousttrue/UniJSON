using NUnit.Framework;

namespace UniJSON
{
    public class ValidatorTests
    {
        [Test]
        public void IntValidator()
        {
            var c = new JsonSchemaValidationContext("test");

            {
                var v = new JsonIntValidator();
                v.Maximum = 0;
                Assert.NotNull(v.Validate(c, 1));
                Assert.Null(v.Validate(c, 0));
                Assert.Null(v.Validate(c, -1));
            }
            {
                var v = new JsonIntValidator();
                v.Maximum = 0;
                v.ExclusiveMaximum = true;
                Assert.NotNull(v.Validate(c, 1));
                Assert.NotNull(v.Validate(c, 0));
                Assert.Null(v.Validate(c, -1));
            }
            {
                var v = new JsonIntValidator();
                v.Minimum = 0;
                Assert.Null(v.Validate(c, 1));
                Assert.Null(v.Validate(c, 0));
                Assert.NotNull(v.Validate(c, -1));
            }
            {
                var v = new JsonIntValidator();
                v.Minimum = 0;
                v.ExclusiveMinimum = true;
                Assert.Null(v.Validate(c, 1));
                Assert.NotNull(v.Validate(c, 0));
                Assert.NotNull(v.Validate(c, -1));
            }
            {
                var v = new JsonIntValidator();
                v.MultipleOf = 4;
                Assert.Null(v.Validate(c, 4));
                Assert.NotNull(v.Validate(c, 5));
            }
        }

        [Test]
        public void NumberValidator()
        {
            var c = new JsonSchemaValidationContext("test");

            {
                var v = new JsonNumberValidator();
                v.Maximum = 0.1;
                Assert.NotNull(v.Validate(c, 1));
                Assert.Null(v.Validate(c, 0.1));
                Assert.Null(v.Validate(c, -1));
            }
            {
                var v = new JsonNumberValidator();
                v.Maximum = 0.1;
                v.ExclusiveMaximum = true;
                Assert.NotNull(v.Validate(c, 1));
                Assert.NotNull(v.Validate(c, 0.1));
                Assert.Null(v.Validate(c, -1));
            }
            {
                var v = new JsonNumberValidator();
                v.Minimum = 0.1;
                Assert.Null(v.Validate(c, 1));
                Assert.Null(v.Validate(c, 0.1));
                Assert.NotNull(v.Validate(c, -1));
            }
            {
                var v = new JsonNumberValidator();
                v.Minimum = 0.1;
                v.ExclusiveMinimum = true;
                Assert.Null(v.Validate(c, 1));
                Assert.NotNull(v.Validate(c, 0.1));
                Assert.NotNull(v.Validate(c, -1));
            }
        }

        [Test]
        public void BoolValidator()
        {
            Assert.Fail();
        }

        [Test]
        public void StringValidator()
        {
            Assert.Fail();
        }

        [Test]
        public void StringEnumValidator()
        {
            Assert.Fail();
        }

        [Test]
        public void IntEnumValidator()
        {
            Assert.Fail();
        }

        [Test]
        public void ArrayValidator()
        {
            Assert.Fail();
        }

        [Test]
        public void ObjectValidator()
        {
            Assert.Fail();
        }
    }
}
