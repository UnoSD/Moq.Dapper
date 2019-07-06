using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Moq.Language.Flow;
using Moq.Protected;

namespace Moq.Dapper
{
    public static class DbCommandSetup
    {
        internal static ISetup<TConnection, TResult> SetupCommand<TResult, TConnection>(Mock<TConnection> mock, Action<Mock<IDbCommand>, Func<TResult>> mockResult)
            where TConnection : class, IDbConnection
        {
            var setupMock = new Mock<ISetup<TConnection, TResult>>();
            var returnsMock = new Mock<IReturnsResult<TConnection>>();

            Func<TResult> getResult = null;
            Action callback = null;

            setupMock.Setup(setup => setup.Returns(It.IsAny<Func<TResult>>()))
                     .Returns(returnsMock.Object)
                     .Callback<Func<TResult>>(r => getResult = r);

            setupMock.Setup(setup => setup.Returns(It.IsAny<TResult>()))
                     .Returns(returnsMock.Object)
                     .Callback<TResult>(r => getResult = () => r);

            returnsMock.Setup(rm => rm.Callback(It.IsAny<Action>()))
                       .Callback<Action>(a => callback = a);

            var commandMock = new Mock<IDbCommand>();

            commandMock.SetupGet(a => a.Parameters)
                       .Returns(new Mock<IDataParameterCollection>().Object);

            commandMock.Setup(a => a.CreateParameter())
                       .Returns(new Mock<IDbDataParameter>().Object);

            mockResult(commandMock, () =>
            {
                var result = getResult();
                callback?.Invoke();
                return result;
            });

            mock.Setup(connection => connection.CreateCommand())
            .Returns(commandMock.Object);

            return setupMock.Object;
        }

        internal static ISetup<TConnection, Task<TResult>> SetupCommandAsync<TResult, TConnection>(Mock<TConnection> mock, Action<Mock<DbCommand>, Func<TResult>> mockResult)
            where TConnection : class, IDbConnection
        {
            var setupMock = new Mock<ISetup<TConnection, Task<TResult>>>();

            var result = default(TResult);

            setupMock.Setup(setup => setup.Returns(It.IsAny<Func<Task<TResult>>>()))
                     .Callback<Func<Task<TResult>>>(r => result = r().Result);

            var commandMock = new Mock<DbCommand>();

            commandMock.Protected()
                       .SetupGet<DbParameterCollection>("DbParameterCollection")
                       .Returns(new Mock<DbParameterCollection>().Object);

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
                    .Setup<IDbCommand>("CreateDbCommand")
                    .Returns(commandMock.Object);

            return setupMock.Object;
        }

        internal static ISetup<TConnection, Task<int>> SetupNonQueryCommandAsync<TConnection>(Mock<TConnection> mock, Action<Mock<DbCommand>, Func<int>> mockResult)
            where TConnection : class, IDbConnection
        {
            var setupMock = new Mock<ISetup<TConnection, Task<int>>>();

            var result = default(int);

            setupMock.Setup(setup => setup.Returns(It.IsAny<Func<Task<int>>>()))
                     .Callback<Func<Task<int>>>(r => result = r().Result);

            var commandMock = new Mock<DbCommand>();

            commandMock.Protected()
                       .SetupGet<DbParameterCollection>("DbParameterCollection")
                       .Returns(new Mock<DbParameterCollection>().Object);

            commandMock.Protected()
                       .Setup<DbParameter>("CreateDbParameter")
                       .Returns(new Mock<DbParameter>().Object);

            mockResult(commandMock, () => result);

            mock
                .As<IDbConnection>()
                .Setup(m => m.CreateCommand())
                .Returns(commandMock.Object);

            return setupMock.Object;
        }
    }
}
