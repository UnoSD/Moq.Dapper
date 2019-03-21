using System.Data;
using Dapper;
using NUnit.Framework;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DapperQueryFirstTest
    {
        [Test]
        public void QueryFirstOrDefaultInteger()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;

            connection.SetupDapper(c => c.QueryFirstOrDefault<int>(It.IsAny<string>(), null, null, null, null))
                      .Returns(expected);

            var actual = connection.Object.QueryFirstOrDefault<int>("");

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void QueryFirstOrDefaultString()
        {
            var connection = new Mock<IDbConnection>();

            const string expected = "test";

            connection.SetupDapper(c => c.QueryFirstOrDefault<string>(It.IsAny<string>(), null, null, null, null))
                      .Returns(expected);

            var actual = connection.Object.QueryFirstOrDefault<string>("");

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
