using System;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Moq.Language.Flow;
using Moq.Protected;
using static Moq.Dapper.DbCommandSetup;

namespace Moq.Dapper
{
    public static class DbConnectionInterfaceMockExtensions
    {
        public static ISetup<IDbConnection, TResult> SetupDapper<TResult>(this Mock<IDbConnection> mock, Expression<Func<IDbConnection, TResult>> expression)
        {
            var call = expression.Body as MethodCallExpression;

            if (call?.Method.DeclaringType != typeof(SqlMapper))
                throw new ArgumentException("Not a Dapper method.");

            switch (call.Method.Name)
            {
                case nameof(SqlMapper.Execute):
                    return (ISetup<IDbConnection, TResult>)SetupExecute(mock);

                case nameof(SqlMapper.ExecuteScalar):
                    return SetupExecuteScalar<TResult>(mock);

                case nameof(SqlMapper.Query):
                case nameof(SqlMapper.QueryFirstOrDefault):
                    return SetupQuery<TResult>(mock);

                default:
                    throw new NotSupportedException();
            }
        }

        public static ISetup<IDbConnection, Task<TResult>> SetupDapperAsync<TResult>(this Mock<IDbConnection> mock, Expression<Func<IDbConnection, Task<TResult>>> expression)
        {
            var call = expression.Body as MethodCallExpression;

            if (call?.Method.DeclaringType != typeof(SqlMapper))
                throw new ArgumentException("Not a Dapper method.");

            switch (call.Method.Name)
            {
                case nameof(SqlMapper.QueryAsync):
                    return SetupQueryAsync<TResult>(mock);

                case nameof(SqlMapper.ExecuteScalarAsync):
                    return SetupExecuteScalarAsync<TResult>(mock);

                default:
                    throw new NotSupportedException();
            }
        }

        public static ISetup<IDbConnection, Task<int>> SetupDapperAsync(this Mock<IDbConnection> mock, Expression<Func<IDbConnection, Task<int>>> expression)
        {
            var call = expression.Body as MethodCallExpression;

            if (call?.Method.DeclaringType != typeof(SqlMapper))
                throw new ArgumentException("Not a Dapper method.");

            switch (call.Method.Name)
            {
                case nameof(SqlMapper.ExecuteAsync):
                    return SetupExecuteAsync(mock);
                default:
                    return SetupDapperAsync<int>(mock, expression);
            }
        }

        static ISetup<IDbConnection, Task<int>> SetupExecuteAsync(Mock<IDbConnection> mock) =>
            SetupNonQueryCommandAsync(mock, (commandMock, result) =>
            {
                commandMock.Setup(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(result);
            });

        static ISetup<IDbConnection, Task<int>> SetupNonQueryCommandAsync(Mock<IDbConnection> mock, Action<Mock<DbCommand>, Func<int>> mockResult)
        {
            var setupMock = new Mock<ISetup<IDbConnection, Task<int>>>();

            var result = default(int);

            setupMock.Setup(setup => setup.Returns(It.IsAny<Func<Task<int>>>()))
                     .Callback<Func<Task<int>>>(r => result = r().Result);

            var commandMock = new Mock<DbCommand>();

            commandMock.SetupAllProperties();

            commandMock.Protected()
                       .SetupGet<DbParameterCollection>("DbParameterCollection")
                       .Returns(new Mock<DbParameterCollection>().Object);

            commandMock.Protected()
                       .Setup<DbParameter>("CreateDbParameter")
                       .Returns(new Mock<DbParameter>().Object);

            mockResult(commandMock, () => result);

            mock.As<IDbConnection>()
                .Setup(m => m.CreateCommand())
                .Returns(commandMock.Object);

            mock.As<IDbConnection>()
                .SetupGet(m => m.State)
                .Returns(ConnectionState.Open);

            return setupMock.Object;
        }

        static ISetup<IDbConnection, Task<TResult>> SetupQueryAsync<TResult>(Mock<IDbConnection> mock) =>
            SetupCommandAsync<TResult, IDbConnection>(mock, (commandMock, result) =>
            {
                commandMock.Protected()
                           .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                           .ReturnsAsync(() => DbDataReaderFactory.DbDataReader(result));
            });

        static ISetup<IDbConnection, Task<TResult>> SetupExecuteScalarAsync<TResult>(Mock<IDbConnection> mock) =>
            SetupExecuteScalarCommandAsync<TResult>(mock, (commandMock, result) =>
            {
                commandMock.Setup(x => x.ExecuteScalarAsync(It.IsAny<CancellationToken>())).ReturnsAsync(result);
            });

        static ISetup<IDbConnection, Task<TResult>> SetupExecuteScalarCommandAsync<TResult>(Mock<IDbConnection> mock, Action<Mock<DbCommand>, Func<object>> mockResult)
        {
            var setupMock = new Mock<ISetup<IDbConnection, Task<TResult>>>();

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

            mock.As<IDbConnection>()
                .Setup(m => m.CreateCommand())
                .Returns(commandMock.Object);

            mock.As<IDbConnection>()
                .SetupGet(m => m.State)
                .Returns(ConnectionState.Open);

            return setupMock.Object;
        }

        static ISetup<IDbConnection, TResult> SetupQuery<TResult>(Mock<IDbConnection> mock) =>
            SetupCommand<TResult>(mock, (commandMock, getResult) =>
                commandMock.Setup(command => command.ExecuteReader(It.IsAny<CommandBehavior>()))
                           .Returns(() => getResult().ToDataTable(typeof(TResult))
                                                     .ToDataTableReader()));

        static ISetup<IDbConnection, TResult> SetupExecuteScalar<TResult>(Mock<IDbConnection> mock) =>
            SetupCommand<TResult>(mock, (commandMock, result) =>
                commandMock.Setup(command => command.ExecuteScalar())
                           .Returns(() => result()));

        static ISetup<IDbConnection, int> SetupExecute(Mock<IDbConnection> mock) =>
            SetupCommand<int>(mock, (commandMock, result) =>
                commandMock.Setup(command => command.ExecuteNonQuery())
                           .Returns(result));
    }
}
