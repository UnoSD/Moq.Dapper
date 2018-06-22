using System.Data;
using Dapper;
using NUnit.Framework;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DapperExecuteTest
    {
        [Test]
        public void ExecuteScalar()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 77;

            connection.SetupDapper(c => c.ExecuteScalar<int>(It.IsAny<string>(), null, null, null, null))
                      .Returns(expected);

            var actual = connection.Object.ExecuteScalar<int>("");

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ExecuteScalarWithParameters()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 77;

            connection.SetupDapper(c => c.ExecuteScalar<int>(It.IsAny<string>(), null, null, null, null))
                      .Returns(expected);

            var actual = connection.Object.ExecuteScalar<int>("", new { id = 1 });

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}