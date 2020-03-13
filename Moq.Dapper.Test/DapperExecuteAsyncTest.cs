using System.Data;
using System.Data.Common;
using Dapper;
using NUnit.Framework;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DapperExecuteAsyncTest
    {
        [Test]
        public void ExecuteAsync_UseDbConnection()
        {
            var connection = new Mock<DbConnection>();

            connection.SetupDapperAsync(c => c.ExecuteAsync("", null, null, null, null))
                      .ReturnsAsync(1);

            var result = connection.Object
                                   .ExecuteAsync("")
                                   .GetAwaiter()
                                   .GetResult();

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void ExecuteAsync_UseIDbConnection()
        {
            var connection = new Mock<IDbConnection>();

            connection.SetupDapperAsync(c => c.ExecuteAsync("", null, null, null, null))
                      .ReturnsAsync(1);

            var result = connection.Object
                                   .ExecuteAsync("")
                                   .GetAwaiter()
                                   .GetResult();

            Assert.That(result, Is.EqualTo(1));
        }
    }
}