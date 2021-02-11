using System;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Dapper;
using NUnit.Framework;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DapperQueryTest
    {
        [Test]
        public void QueryGeneric()
        {
            var connection = new Mock<IDbConnection>();

            var expected = new[] { 7, 77, 777 };

            connection.SetupDapper(c => c.Query<int>(It.IsAny<string>(), null, null, true, null, null))
                      .Returns(expected);

            var actual = connection.Object.Query<int>("").ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        public void QuerySingle()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;

            connection.SetupDapper(c => c.QuerySingle<int>(It.IsAny<string>(), null, null, null, null))
                      .Returns(expected);

            var actual = connection.Object.QuerySingle<int>("");

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void QuerySingleOrDefault()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;

            connection.SetupDapper(c => c.QuerySingleOrDefault<int>(It.IsAny<string>(), null, null, null, null))
                      .Returns(expected);

            var actual = connection.Object.QuerySingleOrDefault<int>("");

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void QueryFirst()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;

            connection.SetupDapper(c => c.QueryFirst<int>(It.IsAny<string>(), null, null, null, null))
                      .Returns(expected);

            var actual = connection.Object.QueryFirst<int>("");

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void QueryFirstOrDefault()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;

            connection.SetupDapper(c => c.QueryFirstOrDefault<int>(It.IsAny<string>(), null, null, null, null))
                      .Returns(expected);

            var actual = connection.Object.QueryFirstOrDefault<int>("");

            Assert.That(actual, Is.EqualTo(expected));
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
                    LongProperty = 70,
                    BigIntegerProperty = 700,
                    GuidProperty = Guid.Parse("CF01F32D-A55B-4C4A-9B33-AAC1C20A85BB"),
                    DateTimeProperty = new DateTime(2000, 1, 1),
                    NullableDateTimeProperty = new DateTime(2000, 1, 1),
                    NullableIntegerProperty = 9,
                    ByteArrayPropery = new byte[] { 1, 2, 4, 8 }
                },
                new ComplexType
                {
                    StringProperty = "String2",
                    IntegerProperty = 77,
                    LongProperty = 770,
                    BigIntegerProperty = 7700,
                    GuidProperty = Guid.Parse("FBECE122-6E2E-4791-B781-C30843DFE343"),
                    DateTimeProperty = new DateTime(2000, 1, 2),
                    NullableDateTimeProperty = new DateTime(2000, 1, 2),
                    NullableIntegerProperty = 99,
                    ByteArrayPropery = new byte[] { 1, 3, 5, 7 }
                },
                new ComplexType
                {
                    StringProperty = "String3",
                    IntegerProperty = 777,
                    LongProperty = 7770,
                    BigIntegerProperty = 77700,
                    GuidProperty = Guid.Parse("712B6DA1-71D8-4D60-8FEF-3F4800A6B04F"),
                    DateTimeProperty = new DateTime(2000, 1, 3),
                    NullableDateTimeProperty = null,
                    NullableIntegerProperty = null
                }
            };

            connection.SetupDapper(c => c.Query<ComplexType>(It.IsAny<string>(), null, null, true, null, null))
                      .Returns(expected);

            var actual = connection.Object.Query<ComplexType>("").ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));

            foreach (var complexObject in expected)
            {
                var match = actual.Where(co => co.StringProperty == complexObject.StringProperty &&
                                               co.IntegerProperty == complexObject.IntegerProperty &&
                                               co.LongProperty == complexObject.LongProperty &&
                                               co.BigIntegerProperty == complexObject.BigIntegerProperty &&
                                               co.GuidProperty == complexObject.GuidProperty &&
                                               co.DateTimeProperty == complexObject.DateTimeProperty &&
                                               co.NullableIntegerProperty == complexObject.NullableIntegerProperty &&
                                               co.NullableDateTimeProperty == complexObject.NullableDateTimeProperty &&
                                               co.ByteArrayPropery == complexObject.ByteArrayPropery);

                Assert.That(match.Count, Is.EqualTo(1));
            }
        }

        public class ComplexType
        {
            public int IntegerProperty { get; set; }
            public long LongProperty { get; set; }
            public BigInteger BigIntegerProperty { get; set; }
            public string StringProperty { get; set; }
            public Guid GuidProperty { get; set; }
            public DateTime DateTimeProperty { get; set; }
            public DateTime? NullableDateTimeProperty { get; set; }
            public int? NullableIntegerProperty { get; set; }
            public byte[] ByteArrayPropery { get; set; }
        }
    }
}
