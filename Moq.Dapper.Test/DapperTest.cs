using System.Data;
using NUnit.Framework;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DapperTest
    {
        [Test]
        public void NonDapperMethodException()
        {
            var connection = new Mock<IDbConnection>();

            object BeginTransaction() => connection.SetupDapper(c => c.BeginTransaction());

            Assert.That(BeginTransaction, Throws.ArgumentException);
        }
    }
}