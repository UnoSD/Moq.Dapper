using System;
using System.Collections;
using System.Data;
using System.Linq;

namespace Moq.Dapper
{
    static class ObjectExtensions
    {
        internal static DataTable ToDataTable(this object result, Type resultType)
        {
            switch (result)
            {
                case null:
                    return Array.CreateInstance(resultType, 0).ToDataTable(resultType);
                case string resultString: // this needs to come before IEnumerable bccause strings are IEnumerable<char>
                    return new[] { resultString }.ToDataTable(resultType);
                case IEnumerable results:
                    return results.ToDataTable(resultType.GenericTypeArguments.Single());
                default:
                    return new[] { result }.ToDataTable(resultType);
            }
        }
    }
}