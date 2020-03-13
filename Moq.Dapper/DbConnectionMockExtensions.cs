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
    public static class DbConnectionMockExtensions
    {
        public static ISetup<TDbConnection, TResult> SetupDapper<TResult, TDbConnection>(this Mock<TDbConnection> mock, Expression<Func<TDbConnection, TResult>> expression) 
            where TDbConnection : class, IDbConnection
        {
            var call = expression.Body as MethodCallExpression;

            if (call?.Method.DeclaringType != typeof(SqlMapper))
                throw new ArgumentException("Not a Dapper method.");

            switch (call.Method.Name)
            {
                case nameof(SqlMapper.ExecuteScalar):
                    return SetupExecuteScalar<TResult, TDbConnection>(mock);

                case nameof(SqlMapper.Query):
                case nameof(SqlMapper.QueryFirstOrDefault):
                    return SetupQuery<TResult, TDbConnection>(mock);

                default:
                    throw new NotSupportedException();
            }
        }

        public static ISetup<TDbConnection, int> SetupDapper<TDbConnection>(this Mock<TDbConnection> mock, Expression<Func<TDbConnection, int>> expression)
            where TDbConnection : class, IDbConnection
        {
            var call = expression.Body as MethodCallExpression;

            if (call?.Method.DeclaringType != typeof(SqlMapper))
                throw new ArgumentException("Not a Dapper method.");

            switch (call.Method.Name)
            {
                case nameof(SqlMapper.Execute):
                    return SetupExecute(mock);
                default:
                    return SetupDapper<int, TDbConnection>(mock, expression);
            }
        }

        public static ISetup<TDbConnection, Task<TResult>> SetupDapperAsync<TResult, TDbConnection>(this Mock<TDbConnection> mock, Expression<Func<TDbConnection, Task<TResult>>> expression)
            where TDbConnection : class, IDbConnection
        {
            var call = expression.Body as MethodCallExpression;

            if (call?.Method.DeclaringType != typeof(SqlMapper))
                throw new ArgumentException("Not a Dapper method.");

            switch (call.Method.Name)
            {
                case nameof(SqlMapper.QueryAsync):
                    return SetupQueryAsync<TResult, TDbConnection>(mock);

                case nameof(SqlMapper.ExecuteScalarAsync):
                    return SetupExecuteScalarAsync<TResult, TDbConnection>(mock);

                default:
                    throw new NotSupportedException();
            }
        }

        public static ISetup<TDbConnection, Task<int>> SetupDapperAsync<TDbConnection>(this Mock<TDbConnection> mock, Expression<Func<TDbConnection, Task<int>>> expression)
            where TDbConnection : class, IDbConnection
        {
            var call = expression.Body as MethodCallExpression;

            if (call?.Method.DeclaringType != typeof(SqlMapper))
                throw new ArgumentException("Not a Dapper method.");

            switch (call.Method.Name)
            {
                case nameof(SqlMapper.ExecuteAsync):
                    return SetupExecuteAsync(mock);
                default:
                    return SetupDapperAsync<int, TDbConnection>(mock, expression);
            }
        }

        static ISetup<TDbConnection, Task<int>> SetupExecuteAsync<TDbConnection>(Mock<TDbConnection> mock) where TDbConnection : class, IDbConnection  =>
            SetupNonQueryCommandAsync(mock, (commandMock, result) =>
            {
                commandMock.Setup(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(result);
            });

        static ISetup<TDbConnection, Task<int>> SetupNonQueryCommandAsync<TDbConnection>(Mock<TDbConnection> mock, Action<Mock<DbCommand>, Func<int>> mockResult)
            where TDbConnection : class, IDbConnection
        {
            var setupMock = new Mock<ISetup<TDbConnection, Task<int>>>();

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

        static ISetup<TDbConnection, Task<TResult>> SetupQueryAsync<TResult, TDbConnection>(Mock<TDbConnection> mock) where TDbConnection : class, IDbConnection =>
            SetupCommandAsync<TResult, TDbConnection>(mock, (commandMock, result) =>
            {
                commandMock.Protected()
                           .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                           .ReturnsAsync(() => DbDataReaderFactory.DbDataReader(result));
            });

        static ISetup<TDbConnection, Task<TResult>> SetupExecuteScalarAsync<TResult, TDbConnection>(Mock<TDbConnection> mock) where TDbConnection : class, IDbConnection =>
            SetupExecuteScalarCommandAsync<TResult, TDbConnection>(mock, (commandMock, result) =>
            {
                commandMock.Setup(x => x.ExecuteScalarAsync(It.IsAny<CancellationToken>())).ReturnsAsync(result);
            });

        static ISetup<TDbConnection, Task<TResult>> SetupExecuteScalarCommandAsync<TResult, TDbConnection>(Mock<TDbConnection> mock, Action<Mock<DbCommand>, Func<object>> mockResult)
             where TDbConnection : class, IDbConnection
        {
            var setupMock = new Mock<ISetup<TDbConnection, Task<TResult>>>();

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

        static ISetup<TDbConnection, TResult> SetupQuery<TResult, TDbConnection>(Mock<TDbConnection> mock) where TDbConnection : class, IDbConnection =>
            SetupCommand<TResult, TDbConnection>(mock, (commandMock, getResult) =>
                commandMock.Setup(command => command.ExecuteReader(It.IsAny<CommandBehavior>()))
                           .Returns(() => getResult().ToDataTable(typeof(TResult))
                                                     .ToDataTableReader()));

        static ISetup<TDbConnection, TResult> SetupExecuteScalar<TResult, TDbConnection>(Mock<TDbConnection> mock) where TDbConnection : class, IDbConnection =>
            SetupCommand<TResult, TDbConnection>(mock, (commandMock, result) =>
                commandMock.Setup(command => command.ExecuteScalar())
                           .Returns(() => result()));

        static ISetup<TDbConnection, int> SetupExecute<TDbConnection>(Mock<TDbConnection> mock) where TDbConnection : class, IDbConnection =>
            SetupCommand<int, TDbConnection>(mock, (commandMock, result) =>
                commandMock.Setup(command => command.ExecuteNonQuery())
                           .Returns(result));
    }
}
