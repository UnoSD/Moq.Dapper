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
        public static ISetup<DbConnection, TResult> SetupDapper<TResult>(this Mock<DbConnection> mock,
            Expression<Func<DbConnection, TResult>> expression)
        {
            var call = expression.Body as MethodCallExpression;

            if (call?.Method.DeclaringType != typeof(SqlMapper))
                throw new ArgumentException("Not a Dapper mehtod.");

            switch (call.Method.Name)
            {
                default:
                    throw new NotSupportedException();
            }
        }

        public static ISetup<DbConnection, Task<TResult>> SetupDapperAsync<TResult>(this Mock<DbConnection> mock,
            Expression<Func<DbConnection, Task<TResult>>> expression)
        {
            var call = expression.Body as MethodCallExpression;

            if (call?.Method.DeclaringType != typeof(SqlMapper))
                throw new ArgumentException("Not a Dapper mehtod.");

            switch (call.Method.Name)
            {
                case nameof(SqlMapper.QueryAsync):
                    return SetupQueryAsync<TResult>(mock);
                default:
                    throw new NotSupportedException();
            }
        }

        private static ISetup<DbConnection, Task<TResult>> SetupQueryAsync<TResult>(Mock<DbConnection> mock)
        {
            return SetupCommandAsync<TResult>(mock, (commandMock, result) =>
            {
                commandMock.Protected()
                    .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(() => DbDataReader(result));
            });
        }

        private static DbDataReader DbDataReader<TResult>(Func<TResult> result)
        {
            // TResult must be IEnumerable if we're invoking SqlMapper.Query.
            var enumerable = (IEnumerable) result();

            var dataTable = new DataTable();

            // Assuming SqlMapper.Query returns always generic IEnumerable<TResult>.
            var type = typeof(TResult).GenericTypeArguments.First();

            if (type.IsPrimitive || type == typeof(string))
            {
                dataTable.Columns.Add();

                foreach (var element in enumerable)
                    dataTable.Rows.Add(element);
            }
            else
            {
                var properties =
                    type.GetProperties().Where
                    (
                        info => info.CanRead &&
                                (info.PropertyType.IsPrimitive ||
                                 info.PropertyType == typeof(DateTime) ||
                                 info.PropertyType == typeof(DateTimeOffset) ||
                                 info.PropertyType == typeof(decimal) ||
                                 info.PropertyType == typeof(Guid) ||
                                 info.PropertyType == typeof(string) ||
                                 info.PropertyType == typeof(TimeSpan))).ToList();

                var columns = properties.Select(property => new DataColumn(property.Name, property.PropertyType))
                    .ToArray();

                dataTable.Columns.AddRange(columns);

                var valuesFactory = properties.Select(info => (Func<object, object>) info.GetValue).ToArray();

                foreach (var element in enumerable)
                    dataTable.Rows.Add(valuesFactory.Select(getValue => getValue(element)).ToArray());
            }

            return new DataTableReader(dataTable);
        }

        private static ISetup<DbConnection, Task<TResult>> SetupCommandAsync<TResult>(Mock<DbConnection> mock,
            Action<Mock<DbCommand>, Func<TResult>> mockResult)
        {
            var setupMock = new Mock<ISetup<DbConnection, Task<TResult>>>();

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

            mock.Protected()
                .Setup<DbCommand>("CreateDbCommand")
                .Returns(commandMock.Object);

            return setupMock.Object;
        }
    }
}