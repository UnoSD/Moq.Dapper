using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Numerics;
using Dapper;
using NUnit.Framework;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DapperQueryAsyncTest
    {
        [Test]
        public void QueryAsyncGeneric()
        {
            var connection = new Mock<IDbConnection>();

            var expected = new[] { 7, 77, 777 };

            connection.SetupDapperAsync(c => c.QueryAsync<int>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object.QueryAsync<int>("").GetAwaiter().GetResult().ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        public void QueryAsyncGenericUsingDbConnectionInterface()
        {
            var connection = new Mock<IDbConnection>();

            var expected = new[] { 7, 77, 777 };

            connection.SetupDapperAsync(c => c.QueryAsync<int>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object.QueryAsync<int>("").GetAwaiter().GetResult().ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
        }
        
        [Test]
        public void QueryAsyncGenericComplexType()
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
                    ByteArrayPropery = new byte[] { 7 }
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
                    ByteArrayPropery = new byte[] { 7, 7 }
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
                    NullableIntegerProperty = null,
                    ByteArrayPropery = new byte[] { 7, 7, 7 }
                }
            };

            connection.SetupDapperAsync(c => c.QueryAsync<ComplexType>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object
                                   .QueryAsync<ComplexType>("")
                                   .GetAwaiter()
                                   .GetResult()
                                   .ToList();

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
            public BigInteger BigIntegerProperty { get; set; }
            public long LongProperty { get; set; }
            public int IntegerProperty { get; set; }
            public string StringProperty { get; set; }
            public Guid GuidProperty { get; set; }
            public DateTime DateTimeProperty { get; set; }
            public DateTime? NullableDateTimeProperty { get; set; }
            public int? NullableIntegerProperty { get; set; }
            public byte[] ByteArrayPropery { get; set; }
        }
    }
}