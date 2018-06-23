using System;
using System.Collections;
using System.Data;
using System.Linq;

namespace Moq.Dapper
{
    static class ObjectExtensions
    {
         internal static DataTable ToDataTable(this object result, Type resultType) =>
             result is IEnumerable results ?
             results.ToDataTable(resultType.GenericTypeArguments.Single()) :
             new[] { result }.ToDataTable(resultType);
    }
}