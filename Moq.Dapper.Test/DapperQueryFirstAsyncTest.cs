using System.Data;
using Dapper;
using NUnit.Framework;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DapperQueryFirstAsyncTest
    {
        [Test]
        public void QueryFirstOrDefaultIntegerAsync()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;

            connection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object
                                   .QueryFirstOrDefaultAsync<int>("")
                                   .GetAwaiter()
                                   .GetResult();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void QueryFirstOrDefaultStringAsync()
        {
            var connection = new Mock<IDbConnection>();

            const string expected = "test";

            connection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<string>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object
                                   .QueryFirstOrDefaultAsync<string>("")
                                   .GetAwaiter()
                                   .GetResult();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void QueryFirstIntegerAsync()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;

            connection.SetupDapperAsync(c => c.QueryFirstAsync<int>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object
                                   .QueryFirstAsync<int>("")
                                   .GetAwaiter()
                                   .GetResult();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void QueryFirstStringAsync()
        {
            var connection = new Mock<IDbConnection>();

            const string expected = "test";

            connection.SetupDapperAsync(c => c.QueryFirstAsync<string>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object
                                   .QueryFirstAsync<string>("")
                                   .GetAwaiter()
                                   .GetResult();

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}