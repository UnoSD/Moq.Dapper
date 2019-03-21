using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DapperExecuteAsyncTest
    {
        [Test]
        public void ExecuteAsync()
        {
            var connection = new Mock<DbConnection>();

            connection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(1);

            var actual = connection.Object
                .ExecuteAsync("")
                .GetAwaiter()
                .GetResult();

            Assert.That(actual, Is.EqualTo(1));
        }

        [Test]
        public void ExecuteAsyncUsingDbConnectionInterface()
        {
            var connection = new Mock<DbConnection>().As<IDbConnection>();

            connection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(1);

            var actual = connection.Object
                .ExecuteAsync("")
                .GetAwaiter()
                .GetResult();

            Assert.That(actual, Is.EqualTo(1));
        }

        [Test]
        public void ExecuteScalarAsync()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 77;

            connection.As<IDbConnection>().SetupDapperAsync(c => c.ExecuteScalarAsync(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object
                .ExecuteScalarAsync("")
                .GetAwaiter()
                .GetResult() as Func<object>;

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual(), Is.EqualTo(expected));
        }

        [Test]
        public void ExecuteScalarAsyncWithParameters()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 77;

            connection.SetupDapperAsync(c => c.ExecuteScalarAsync(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object
                .ExecuteScalarAsync("", new { id = 1 })
                .GetAwaiter()
                .GetResult() as Func<object>;

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual(), Is.EqualTo(expected));
        }
    }
}