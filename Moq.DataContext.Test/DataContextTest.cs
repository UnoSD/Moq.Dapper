using System.Data;
using System.Data.Common;
using Dapper;
using NUnit.Framework;
using DataContextType = System.Data.Linq.DataContext;
using Moq.DataContext;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DataContextTest
    {
        [Test]
        public void ExecuteScalar()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 77;

            connection.As<IDbConnection>()
                      .SetupDapper(c => c.ExecuteScalar<int>(It.IsAny<string>(), null, null, null, null))
                      .Returns(expected);

            var mock = new DataContextMock<TestDataContext>(connection);

            var actual = mock.Object.Connection.ExecuteScalar<int>("");

            Assert.That(actual, Is.EqualTo(expected));
        }

        public class TestDataContext : DataContextType
        {
            public TestDataContext() : base("") { }
        }
    }
}