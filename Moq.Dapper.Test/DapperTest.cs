using System;
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
                new ComplexType
                {
                    StringProperty = "String1",
                    IntegerProperty = 7,
                    GuidProperty = Guid.Parse("CF01F32D-A55B-4C4A-9B33-AAC1C20A85BB"),
                    DateTimeProperty = new DateTime(2000, 1, 1)
                },
                new ComplexType
                {
                    StringProperty = "String2",
                    IntegerProperty = 77,
                    GuidProperty = Guid.Parse("FBECE122-6E2E-4791-B781-C30843DFE343"),
                    DateTimeProperty = new DateTime(2000, 1, 2)
                },
                new ComplexType
                {
                    StringProperty = "String3",
                    IntegerProperty = 777,
                    GuidProperty = Guid.Parse("712B6DA1-71D8-4D60-8FEF-3F4800A6B04F"),
                    DateTimeProperty = new DateTime(2000, 1, 3)
                }
            };

            connection.SetupDapper(c => c.Query<ComplexType>(It.IsAny<string>(), null, null, true, null, null))
                      .Returns(expected);

            var actual = connection.Object.Query<ComplexType>("", null, null, true, null, null).ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));

            foreach (var complexObject in expected)
            {
                var match = actual.Where(co => co.StringProperty == complexObject.StringProperty &&
                                               co.IntegerProperty == complexObject.IntegerProperty &&
                                               co.GuidProperty == complexObject.GuidProperty &&
                                               co.DateTimeProperty == complexObject.DateTimeProperty);

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

            var actual = connection.Object.ExecuteScalar<int>("", new { id = 1 }, null, null);

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
            public Guid GuidProperty { get; set; }
            public DateTime DateTimeProperty { get; set; }
        }
    }
}
