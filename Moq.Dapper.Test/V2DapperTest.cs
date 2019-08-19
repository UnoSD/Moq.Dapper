using System.Data;
using NUnit.Framework;
using Dapper;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class V2DapperTest
    {
        [Test]
        public void Test()
        {
            var connection = new Mock<IDbConnection>();

            connection.SetupDapperV2(c => c.Execute("query", null, null, null, null))
                      .Returns(5);

            var result = 
                connection.Object
                    .Execute("query", null, null, null, null);

            Assert.That(result, Is.EqualTo(5));
        }
    }
}
