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

        [Test]
        public void Execute()
        {
            var connection = new Mock<IDbConnection>();

            connection.SetupDapper(c => c.Execute(It.IsAny<string>(), null, null, null, null))
                      .Returns(1);

            var result = connection.Object
                                   .Execute("");

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void ExecuteAsync()
        {
            var connection = new Mock<IDbConnection>();

            connection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(1);

            var result = connection.Object
                                   .Execute("");

            Assert.That(result, Is.EqualTo(1));
        }
    }
}