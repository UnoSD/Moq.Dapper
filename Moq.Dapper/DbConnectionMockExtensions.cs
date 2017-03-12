using System;
using System.Data.Common;
using System.Linq.Expressions;
using Dapper;
using Moq.Language.Flow;

namespace Moq.Dapper
{
    public static class DbConnectionMockExtensions
    {
        public static ISetup<DbConnection, TResult> SetupDapper<TResult>(this Mock<DbConnection> mock, Expression<Func<DbConnection, TResult>> expression)
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
    }
}