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
        static Dictionary<string, Func<string, object>> _methods;
        const string NotMethodCall = "Expression is not a method call. Only Dapper methods allowed.";
        const string NotDapperMethod = "Not a Dapper method used in the expression.";

        public static ISetup<IDbConnection, T> SetupDapperV2<T>
        (
            this Mock<IDbConnection> connection,
            Expression<Func<IDbConnection, T>> expression
        )
        {
            var invocationInfo = expression.Parse();

            var cmd = connection.SetupCommand();

            var m = cmd.GetInvokedMethod(invocationInfo);

            connection.OnInvokeReturn(() => cmd.SetupLateBindValue());

            return null;
        }

        public static Mock<DbCommand> SetupCommand(this Mock<IDbConnection> cm)
        {
            var dbCommandMock = new Mock<DbCommand>();

            cm.Setup(c => c.CreateCommand())
                .Returns(dbCommandMock.Object);

            return dbCommandMock;
        }

        //public static ISetup<IDbConnection, T> SetupDapperV2<T>
        //(
        //    this Mock<IDbConnection> connection,
        //    Expression<Func<IDbConnection, T>> expression
        //)
        //{
        //    var p = expression.Parse();

        //    T valueStore = default;

        //    var dbCommandMock = new Mock<IDbCommand>();

        //    //dbCommandMock.SetupProperty(c => c.CommandText);
        //    //dbCommandMock.Object.CommandText == expression.Body...

        //    dbCommandMock.Setup(c => c.ExecuteNonQuery())
        //                 .Returns(() => valueStore is int x ? x : 0);

        //    connection.Setup(c => c.CreateCommand())
        //              .Returns(dbCommandMock.Object);

        //    var setupMock = new Mock<ISetup<IDbConnection, T>>();

        //    setupMock.Setup(s => s.Returns(It.IsAny<T>()))
        //             .Callback<T>(value => valueStore = value);

        //    return setupMock.Object;
        //}

        public static object Parse<T>(
            this Expression<Func<IDbConnection, T>> expression)
        {
            var call = 
                expression.Body as MethodCallExpression ??
                throw new ArgumentException(NotMethodCall);

            return call.Method.DeclaringType != typeof(SqlMapper) ? 
                throw new ArgumentException(NotDapperMethod) :
                Parse(call);
        }

        public static object Parse(MethodCallExpression call)
        {
            var sqlParameter = 
                call.Method
                    .GetParameters()
                    .FirstOrDefault(p => p.Name == "sql");

            var sqlArgument =
                sqlParameter == null ?
                    null :
                    (call.Arguments[sqlParameter.Position] as ConstantExpression)?.Value as string;

            return Parse(call.Method.Name, sqlArgument);
        }

        public static object Parse(string methodName, string sqlArgument) =>
            _methods.TryGetValue(methodName, out var method) ?
                method(sqlArgument) :
                throw new NotImplementedException(methodName);
    }
}