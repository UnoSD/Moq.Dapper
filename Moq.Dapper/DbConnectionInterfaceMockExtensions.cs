using System;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Moq.Language.Flow;
using Moq.Protected;

namespace Moq.Dapper
{
    public static class DbConnectionInterfaceMockExtensions
    {
        #region Synchronous

        public static ISetup<TConnection, TResult> SetupDapper<TResult, TConnection>(this Mock<TConnection> mock, Expression<Func<TConnection, TResult>> expression)
            where TConnection : class, IDbConnection
        {
            var call = expression.Body as MethodCallExpression;

            if (call?.Method.DeclaringType != typeof(SqlMapper))
                throw new ArgumentException("Not a Dapper method.");

            switch (call.Method.Name)
            {
                case nameof(SqlMapper.Execute):
                    return (ISetup<TConnection, TResult>)SetupExecute(mock);
                case nameof(SqlMapper.ExecuteScalar):
                    return SetupExecuteScalar<TResult, TConnection>(mock);
                case nameof(SqlMapper.Query):
                case nameof(SqlMapper.QueryFirst):
                case nameof(SqlMapper.QueryFirstOrDefault):
                case nameof(SqlMapper.QuerySingle):
                case nameof(SqlMapper.QuerySingleOrDefault):
                    return SetupQuery<TResult, TConnection>(mock);
                default:
                    throw new NotSupportedException();
            }
        }

        private static ISetup<TConnection, TResult> SetupQuery<TResult, TConnection>(Mock<TConnection> mock)
            where TConnection : class, IDbConnection
        {
            return DbCommandSetup.SetupCommand<TResult, TConnection>(
                mock,
                (commandMock, result) =>
                {
                    commandMock
                        .Setup(command => command.ExecuteReader(It.IsAny<CommandBehavior>()))
                        .Returns(() => DbDataReaderProvider.DbDataReader(result));
                });
        }

        private static ISetup<TConnection, TResult> SetupExecuteScalar<TResult, TConnection>(Mock<TConnection> mock)
            where TConnection : class, IDbConnection
        {
            return DbCommandSetup.SetupCommand<TResult, TConnection>(
                mock,
                (commandMock, result) =>
                {
                    commandMock
                        .Setup(command => command.ExecuteScalar())
                        .Returns(() => result());
                });
        }

        private static ISetup<TConnection, int> SetupExecute<TConnection>(Mock<TConnection> mock)
            where TConnection : class, IDbConnection
        {
            return DbCommandSetup.SetupCommand<int, TConnection>(
                mock,
                (commandMock, result) =>
                {
                    commandMock
                        .Setup(command => command.ExecuteNonQuery())
                        .Returns(() => result());
                });
        }
        #endregion

        #region Asynchronous
        public static ISetup<TConnection, Task<TResult>> SetupDapperAsync<TResult, TConnection>(this Mock<TConnection> mock, Expression<Func<TConnection, Task<TResult>>> expression)
            where TConnection : class, IDbConnection
        {
            var call = expression.Body as MethodCallExpression;

            if (call?.Method.DeclaringType != typeof(SqlMapper))
                throw new ArgumentException("Not a Dapper method.");

            switch (call.Method.Name)
            {
                case nameof(SqlMapper.QueryAsync):
                case nameof(SqlMapper.QueryFirstAsync):
                case nameof(SqlMapper.QueryFirstOrDefaultAsync):
                case nameof(SqlMapper.QuerySingleAsync):
                case nameof(SqlMapper.QuerySingleOrDefaultAsync):
                    return SetupQueryAsync<TResult, TConnection>(mock);
                case nameof(SqlMapper.ExecuteScalarAsync):
                    return SetupExecuteScalarAsync<TResult, TConnection>(mock);
                case nameof(SqlMapper.ExecuteAsync) when typeof(TResult) == typeof(int):
                    return (ISetup<TConnection, Task<TResult>>)SetupExecuteAsync(mock);
                default:
                    throw new NotSupportedException();
            }
        }

        private static ISetup<TConnection, Task<int>> SetupExecuteAsync<TConnection>(Mock<TConnection> mock)
            where TConnection : class, IDbConnection
        {
            return DbCommandSetup.SetupNonQueryCommandAsync(
                mock,
                (commandMock, result) =>
                {
                    commandMock
                        .Setup(command => command.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(result);
                });
        }

        private static ISetup<TConnection, Task<TResult>> SetupExecuteScalarAsync<TResult, TConnection>(Mock<TConnection> mock)
            where TConnection : class, IDbConnection
        {
            return DbCommandSetup.SetupCommandAsync<TResult, TConnection>(
                mock,
                (commandMock, result) =>
                {
                    commandMock
                        .Setup(command => command.ExecuteScalarAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(result);
                });
        }

        private static ISetup<TConnection, Task<TResult>> SetupQueryAsync<TResult, TConnection>(Mock<TConnection> mock)
            where TConnection : class, IDbConnection
        {
            return DbCommandSetup.SetupCommandAsync<TResult, TConnection>(
                mock,
                (commandMock, result) =>
                {
                    commandMock
                    .Protected()
                    .Setup<Task<DbDataReader>>(
                        "ExecuteDbDataReaderAsync",
                        ItExpr.IsAny<CommandBehavior>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(() => DbDataReaderProvider.DbDataReader(result));
                });
        }

        #endregion
    }
}
