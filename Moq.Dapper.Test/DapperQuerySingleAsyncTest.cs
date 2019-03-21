using System.Data;
using Dapper;
using NUnit.Framework;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DapperQuerySingleAsyncTest
    {
        [Test]
        public void QuerySingleOrDefaultIntegerAsync()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;

            connection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object.QuerySingleOrDefaultAsync<int>("").GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void QuerySingleOrDefaultStringAsync()
        {
            var connection = new Mock<IDbConnection>();

            const string expected = "test";

            connection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<string>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object.QuerySingleOrDefaultAsync<string>("").GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void QuerySingleIntegerAsync()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;

            connection.SetupDapperAsync(c => c.QuerySingleAsync<int>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object.QuerySingleAsync<int>("").GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void QuerySingleStringAsync()
        {
            var connection = new Mock<IDbConnection>();

            const string expected = "test";

            connection.SetupDapperAsync(c => c.QuerySingleAsync<string>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object.QuerySingleAsync<string>("").GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}