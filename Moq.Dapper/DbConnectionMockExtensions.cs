using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Moq.Language.Flow;
using Moq.Protected;

namespace Moq.Dapper
{
    public static class DbConnectionMockExtensions
    {
        public static ISetup<DbConnection, TResult> SetupDapper<TResult>(this Mock<DbConnection> mock, Expression<Func<DbConnection, TResult>> expression)
        {
            var call = expression.Body as MethodCallExpression;

            if (call?.Method.DeclaringType != typeof(SqlMapper))
                throw new ArgumentException("Not a Dapper method.");

            switch (call.Method.Name)
            {
                default:
                    throw new NotSupportedException();
            }
        }

        public static ISetup<DbConnection, Task<TResult>> SetupDapperAsync<TResult>(this Mock<DbConnection> mock, Expression<Func<DbConnection, Task<TResult>>> expression)
        {
            var call = expression.Body as MethodCallExpression;

            if (call?.Method.DeclaringType != typeof(SqlMapper))
                throw new ArgumentException("Not a Dapper method.");

            switch (call.Method.Name)
            {
                case nameof(SqlMapper.QueryAsync):
                    return SetupQueryAsync<TResult>(mock);
                case nameof(SqlMapper.ExecuteAsync):
                    return SetupExecuteAsync<TResult>(mock);
                default:
                    throw new NotSupportedException();
            }
        }

        private static ISetup<DbConnection, Task<TResult>> SetupQueryAsync<TResult>(Mock<DbConnection> mock) =>
            DbCommandSetup.SetupCommandAsync<TResult, DbConnection>(mock, (commandMock, result) =>
            {
                commandMock.Protected()
                           .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                           .ReturnsAsync(() => DbDataReaderFactory.DbDataReader(result));
            });

        private static ISetup<DbConnection, Task<TResult>> SetupExecuteAsync<TResult>(Mock<DbConnection> mock) =>
            SetupNonQueryCommandAsync<TResult>(mock, (commandMock, result) =>
            {
                commandMock.Setup(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(result);
            });

        private static ISetup<DbConnection, Task<TResult>> SetupNonQueryCommandAsync<TResult>(Mock<DbConnection> mock, Action<Mock<DbCommand>, Func<int>> mockResult)
        {
            var setupMock = new Mock<ISetup<DbConnection, Task<int>>>();

            var result = default(int);

            setupMock.Setup(setup => setup.Returns(It.IsAny<Func<Task<int>>>()))
                     .Callback<Func<Task<int>>>(r => result = r().Result);

            var commandMock = new Mock<DbCommand>();

            mockResult(commandMock, () => result);

            mock.As<IDbConnection>()
                .Setup(m => m.CreateCommand())
                .Returns(commandMock.Object);

            return (ISetup<DbConnection, Task<TResult>>)setupMock.Object;
        }
    }
}