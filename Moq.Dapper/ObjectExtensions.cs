using System;
using System.Collections;
using System.Data;
using System.Linq;

namespace Moq.Dapper
{
    static class ObjectExtensions
    {
        internal static DataTable ToDataTable(this object result, Type resultType) =>
            result switch
            {
                null                => Array.CreateInstance(resultType, 0).ToDataTable(resultType),
                string resultString => new[] { resultString }.ToDataTable(resultType),
                IEnumerable results => results.ToDataTable(resultType.GenericTypeArguments.Single()),
                _                   => new[] { result }.ToDataTable(resultType)
            };
    }
}