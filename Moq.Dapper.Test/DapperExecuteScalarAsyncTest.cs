using System.Data;
using System.Data.Common;
using Dapper;
using NUnit.Framework;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DapperExecuteScalarAsyncTest
    {
        [Test]
        public void ExecuteScalarAsyncGeneric()
        {
            var connection = new Mock<DbConnection>();

            const string expected = "Hello";

            connection.SetupDapperAsync(c => c.ExecuteScalarAsync<object>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object
                                   .ExecuteScalarAsync<object>("")
                                   .GetAwaiter()
                                   .GetResult();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ExecuteScalarAsyncGenericWithSpecificType()
        {
            var connection = new Mock<DbConnection>();

            const string expected = "Hello";

            connection.SetupDapperAsync(c => c.ExecuteScalarAsync<string>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object
                                   .ExecuteScalarAsync<string>("")
                                   .GetAwaiter()
                                   .GetResult();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ExecuteScalarAsyncGenericWithParameters()
        {
            var connection = new Mock<DbConnection>();
            
            const int expected = 1;

            connection.SetupDapperAsync(c => c.ExecuteScalarAsync<object>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object
                                   .ExecuteScalarAsync<object>("", new { id = 1 })
                                   .GetAwaiter()
                                   .GetResult();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ExecuteScalarAsyncGenericWithDynamicParameters()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 1;

            connection.SetupDapperAsync(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), It.IsAny<CommandType?>()))
                      .ReturnsAsync(expected);

            var actual = connection.Object
                                   .ExecuteScalarAsync("", new DynamicParameters(new { id = 1 }))
                                   .GetAwaiter()
                                   .GetResult();

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}