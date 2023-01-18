using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Moq.Language.Flow;
using Moq.Protected;

namespace Moq.Dapper
{
    public static class DbCommandSetup
    {
        internal static ISetup<TConnection, Task<TResult>> SetupCommandAsync<TResult, TConnection>(Mock<TConnection> mock, Action<Mock<DbCommand>, Func<TResult>> mockResult)
            where TConnection : class, IDbConnection
        {
            var setupMock = new Mock<ISetup<TConnection, Task<TResult>>>();

            var result = default(TResult);

            setupMock.Setup(setup => setup.Returns(It.IsAny<Func<Task<TResult>>>()))
                     .Callback<Func<Task<TResult>>>(r => result = r().Result);

            var commandMock = new Mock<DbCommand>();

            commandMock.SetupAllProperties();

            var parametersMock = new Mock<DbParameterCollection>();
            parametersMock.Setup(x => x.GetEnumerator()).Returns(new Mock<IEnumerator>().Object);
            commandMock.Protected()
                       .SetupGet<DbParameterCollection>("DbParameterCollection")
                       .Returns(parametersMock.Object);

            commandMock.Protected()
                       .Setup<DbParameter>("CreateDbParameter")
                       .Returns(new Mock<DbParameter>().Object);

            mockResult(commandMock, () => result);

            var iDbConnectionMock = mock.As<IDbConnection>();

            iDbConnectionMock.Setup(m => m.CreateCommand())
                             .Returns(commandMock.Object);

            iDbConnectionMock.SetupGet(m => m.State)
                             .Returns(ConnectionState.Open);

            if (typeof(TConnection) == typeof(DbConnection))
                mock.Protected()
                    .Setup<DbCommand>("CreateDbCommand")
                    .Returns(commandMock.Object);

            return setupMock.Object;
        }
    }
}