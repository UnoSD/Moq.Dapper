using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using Dapper;
using Moq.Language.Flow;

namespace Moq.Dapper.Test
{
    public static class MoqDapper
    {
        const string NotMethodCall = "Expression is not a method call. Only Dapper methods allowed.";
        const string NotDapperMethod = "Not a Dapper method used in the expression.";

        static readonly Dictionary<string, Func<string, object>> Methods =
            new Dictionary<string, Func<string, object>>
            {
                ["Execute"] = null
            };

        public static ISetup<IDbConnection, T> SetupDapperV2<T>
        (
            this Mock<IDbConnection> connectionMock,
            Expression<Func<IDbConnection, T>> expression
        )
        {
            var invocationInfo = expression.ValidateParse();

            var commandMock = connectionMock.CreateCommandMock();

            var setup = UpdateCommandSetupOnConnectionSetup<T>(commandMock);

            return setup;
        }

        static ISetup<IDbConnection, T> UpdateCommandSetupOnConnectionSetup<T>(Mock<DbCommand> commandMock)
        {
            var setupMock = new Mock<ISetup<IDbConnection, T>>();

            setupMock.Setup(s => s.Returns(It.IsAny<T>()))
                     .Callback<T>(commandMock.SetupCommand);

            return setupMock.Object;
        }

        static void SetupCommand<T>(this Mock<DbCommand> commandMock, T value)
        {
            switch (value)
            {
                case int x when "Method is" != nameof(SqlMapper.Execute):
                    commandMock.Setup(c => c.ExecuteNonQuery())
                               .Returns(x);
                    break;
                //default:
                //    var dt = value.ToDataTable();
                //    commandMock.Setup(c => c.ExecuteReader())
                //               .Returns(new DataTableReader(dt));
                //    break;
            }
        }

        public static Mock<DbCommand> CreateCommandMock(this Mock<IDbConnection> cm)
        {
            var dbCommandMock = new Mock<DbCommand>();

            dbCommandMock.SetupProperty(c => c.CommandText);

            cm.Setup(c => c.CreateCommand())
              .Returns(dbCommandMock.Object);

            return dbCommandMock;
        }

        static InvocationInfo ValidateParse(this LambdaExpression expression)
        {
            var call =
                expression.Body as MethodCallExpression ??
                throw new ArgumentException(NotMethodCall);

            return call.Method.DeclaringType != typeof(SqlMapper) ?
                   throw new ArgumentException(NotDapperMethod) :
                   call.ValidateParse();
        }

        static InvocationInfo ValidateParse(this MethodCallExpression call)
        {
            var sqlParameter =
                call.Method
                    .GetParameters()
                    .FirstOrDefault(p => p.Name == "sql");

            var sqlArgument =
                sqlParameter == null ?
                    null :
                    (call.Arguments[sqlParameter.Position] as ConstantExpression)?.Value as string;

            return ValidateParse(call.Method.Name, sqlArgument);
        }

        static InvocationInfo ValidateParse(string methodName, string sqlArgument) =>
            Methods.ContainsKey(methodName) ?
            new InvocationInfo
            {
                Method = methodName,
                Sql = sqlArgument
            } : 
            throw new NotImplementedException(methodName);

        class InvocationInfo
        {
            internal string Method { get; set; }
            internal string Sql { get; set; }
        }
    }
}