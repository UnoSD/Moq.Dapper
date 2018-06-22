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
    public static class DbConnectionInterfaceMockExtensions
    {
        public static ISetup<IDbConnection, TResult> SetupDapper<TResult>(this Mock<IDbConnection> mock, Expression<Func<IDbConnection, TResult>> expression)
        {
            var call = expression.Body as MethodCallExpression;

            if (call?.Method.DeclaringType != typeof(SqlMapper))
                throw new ArgumentException("Not a Dapper method.");

            switch (call.Method.Name)
            {
                case nameof(SqlMapper.QueryFirstOrDefault):
                    return SetupQuery<TResult>(mock);
                case nameof(SqlMapper.ExecuteScalar):
                    return SetupExecuteScalar<TResult>(mock);
                case nameof(SqlMapper.Query):
                    return SetupQuery<TResult>(mock);
                case nameof(SqlMapper.Execute):
                    return SetupExecute<TResult>(mock);
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
                default:
                    throw new NotSupportedException();
            }
        }

        private static ISetup<IDbConnection, Task<TResult>> SetupQueryAsync<TResult>(Mock<IDbConnection> mock) =>
            DbCommandSetup.SetupCommandAsync<TResult, IDbConnection>(mock, (commandMock, result) =>
            {
                commandMock.Protected()
                           .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                           .ReturnsAsync(() => DbDataReaderFactory.DbDataReader(result));
            });

        private static ISetup<IDbConnection, TResult> SetupQuery<TResult>(Mock<IDbConnection> mock) =>
            SetupCommand<TResult>(mock, (commandMock, getResult) =>
            {
                commandMock.Setup(command => command.ExecuteReader(It.IsAny<CommandBehavior>()))
                           .Returns(() =>
                           {
                               var result = getResult();
                               var results = result as IEnumerable;
                               var enumerable = results ?? new[] { result };

                               var dataTable = new DataTable();

                               // Assuming SqlMapper.Query returns always generic IEnumerable<TResult>.
                               var type = results == null ? 
                                          typeof(TResult) :
                                          typeof(TResult).GenericTypeArguments.First();

                               if (type.IsPrimitive || type == typeof(string))
                               {
                                   dataTable.Columns.Add();

                                   foreach (var element in enumerable)
                                       dataTable.Rows.Add(element);
                               }
                               else
                               {
                                   bool IsNullable(Type t) =>
                                       t.IsGenericType &&
                                       t.GetGenericTypeDefinition() == typeof(Nullable<>);

                                   Type GetDataColumnType(Type source) =>
                                       IsNullable(source) ?
                                       Nullable.GetUnderlyingType(source) :
                                       source;

                                   bool IsMatchingType(Type t) =>
                                       t.IsPrimitive ||
                                       t == typeof(DateTime) ||
                                       t == typeof(DateTimeOffset) ||
                                       t == typeof(decimal) ||
                                       t == typeof(Guid) ||
                                       t == typeof(string) ||
                                       t == typeof(TimeSpan) ||
                                       t == typeof(byte[]);
                                   
                                   var properties = 
                                       type.GetProperties()
                                           .Where(info => info.CanRead &&
                                                          IsMatchingType(info.PropertyType) ||
                                                          IsNullable(info.PropertyType) &&
                                                          IsMatchingType(Nullable.GetUnderlyingType(info.PropertyType)))
                                           .ToList();
                                   
                                   var columns = properties.Select(property => new DataColumn(property.Name, GetDataColumnType(property.PropertyType)))
                                                           .ToArray();

                                   dataTable.Columns.AddRange(columns);

                                   var valuesFactory = properties.Select(info => (Func<object, object>)info.GetValue)
                                                                 .ToArray();
                                   
                                   foreach (var element in enumerable)
                                       dataTable.Rows.Add(valuesFactory.Select(getValue => getValue(element)).ToArray());
                               }
                               
                               return new DataTableReader(dataTable);
                           });
            });

        private static ISetup<IDbConnection, TResult> SetupCommand<TResult>(Mock<IDbConnection> mock, Action<Mock<IDbCommand>, Func<TResult>> mockResult)
        {
            var setupMock = new Mock<ISetup<IDbConnection, TResult>>();

            var result = default(TResult);

            setupMock.Setup(setup => setup.Returns(It.IsAny<TResult>()))
                     .Callback<TResult>(r => result = r);

            var commandMock = new Mock<IDbCommand>();
            
            commandMock.SetupGet(a => a.Parameters)
                       .Returns(new Mock<IDataParameterCollection>().Object);

            commandMock.Setup(a => a.CreateParameter())
                       .Returns(new Mock<IDbDataParameter>().Object);

            mockResult(commandMock, () => result);

            mock.Setup(connection => connection.CreateCommand())
                .Returns(commandMock.Object);

            return setupMock.Object;
        }

        private static ISetup<IDbConnection, TResult> SetupExecuteScalar<TResult>(Mock<IDbConnection> mock) =>
            SetupCommand<TResult>(mock, (commandMock, result) =>
                commandMock.Setup(command => command.ExecuteScalar())
                                                    .Returns(() => result()));

        private static ISetup<IDbConnection, TResult> SetupExecute<TResult>(Mock<IDbConnection> mock) =>
            SetupCommand<TResult>(mock, (commandMock, result) =>
                commandMock.Setup(command => command.ExecuteNonQuery())
                                                    .Returns(() => Convert.ToInt32(result())));
    }
}
