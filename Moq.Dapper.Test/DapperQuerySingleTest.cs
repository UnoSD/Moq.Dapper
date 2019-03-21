using System.Data;
using Dapper;
using NUnit.Framework;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DapperQuerySingleTest
    {
        [Test]
        public void QueryFirstOrDefaultInteger()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;

            connection.SetupDapper(c => c.QuerySingleOrDefault<int>(It.IsAny<string>(), null, null, null, null))
                      .Returns(expected);

            var actual = connection.Object.QuerySingleOrDefault<int>("");

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void QueryFirstOrDefaultString()
        {
            var connection = new Mock<IDbConnection>();

            const string expected = "test";

            connection.SetupDapper(c => c.QuerySingleOrDefault<string>(It.IsAny<string>(), null, null, null, null))
                      .Returns(expected);

            var actual = connection.Object.QuerySingleOrDefault<string>("");

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
