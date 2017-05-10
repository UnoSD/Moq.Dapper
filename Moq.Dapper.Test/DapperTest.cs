using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DapperTest
    {
        [Test]
        public void QueryGeneric()
        {
            var connection = new Mock<IDbConnection>();

            var expected = new[] { 7, 77, 777 };

            connection.SetupDapper(c => c.Query<int>(It.IsAny<string>(), null, null, true, null, null))
                      .Returns(expected);

            var actual = connection.Object.Query<int>("", null, null, true, null, null).ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        public void QueryGenericComplexType()
        {
            var connection = new Mock<IDbConnection>();

            var expected = new[]
            {
                new ComplexType { StringProperty = "String1", IntegerProperty = 7 },
                new ComplexType { StringProperty = "String2", IntegerProperty = 77 },
                new ComplexType { StringProperty = "String3", IntegerProperty = 777 }
            };

            connection.SetupDapper(c => c.Query<ComplexType>(It.IsAny<string>(), null, null, true, null, null))
                      .Returns(expected);

            var actual = connection.Object.Query<ComplexType>("", null, null, true, null, null).ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));

            foreach (var complexObject in expected)
            {
                var match = actual.Where(co => co.StringProperty == complexObject.StringProperty && co.IntegerProperty == complexObject.IntegerProperty);

                Assert.That(match.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void ExecuteScalar()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 77;

            connection.SetupDapper(c => c.ExecuteScalar<int>(It.IsAny<string>(), null, null, null, null))
                      .Returns(expected);

            var actual = connection.Object.ExecuteScalar<int>("", null, null, null);

            Assert.That(actual, Is.EqualTo(expected));
        }
        
        [Test]
        public void ExecuteScalarWithParameters()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 77;

            connection.SetupDapper(c => c.ExecuteScalar<int>(It.IsAny<string>(), null, null, null, null))
                      .Returns(expected);

            var actual = connection.Object.ExecuteScalar<int>("select @id", new { id = 1 }, null, null);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void NonDapperMethodException()
        {
            var connection = new Mock<IDbConnection>();

            ActualValueDelegate<object> valueDelegate = () =>
                connection.SetupDapper(c => c.BeginTransaction());

            Assert.That(valueDelegate, Throws.ArgumentException);
        }

        public class ComplexType
        {
            public int IntegerProperty { get; set; }
            public string StringProperty { get; set; }
        }
    }
}
