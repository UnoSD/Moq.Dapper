using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Dapper;
using Moq.Language.Flow;

namespace Moq.Dapper
{
    public static class DbConnectionInterfaceMockExtensions
    {
        public static ISetup<IDbConnection, TResult> SetupDapper<TResult>(this Mock<IDbConnection> mock, Expression<Func<IDbConnection, TResult>> expression)
        {
            var call = expression.Body as MethodCallExpression;

            if (call?.Method.DeclaringType != typeof(SqlMapper))
                throw new ArgumentException("Not a Dapper mehtod.");

            switch (call.Method.Name)
            {
                case nameof(SqlMapper.ExecuteScalar):
                    return SetupExecuteScalar<TResult>(mock);
                case nameof(SqlMapper.Query):
                    return SetupQuery<TResult>(mock);
                default:
                    throw new NotSupportedException();
            }
        }

        private static ISetup<IDbConnection, TResult> SetupQuery<TResult>(Mock<IDbConnection> mock) =>
            SetupCommand<TResult>(mock, (commandMock, result) =>
            {
                commandMock.Setup(command => command.ExecuteReader(It.IsAny<CommandBehavior>()))
                           .Returns(() =>
                           {
                               // TResult must be IEnumerable if we're invoking SqlMapper.Query.
                               var enumerable = (IEnumerable)result();

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
                                   var valuesFactory = type.GetProperties()
                                                           .Where(info => info.CanRead && (info.PropertyType.IsPrimitive || info.PropertyType == typeof(string)))
                                                           .ToDictionary(info => info.Name, info => (Func<object, object>) info.GetValue);

                                   dataTable.Columns.AddRange(valuesFactory.Keys.Select(name => new DataColumn(name)).ToArray());

                                   foreach (var element in enumerable)
                                       dataTable.Rows.Add(valuesFactory.Values.Select(getValue => getValue(element)).ToArray());
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
    }
}
