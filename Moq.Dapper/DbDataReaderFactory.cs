using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Moq.Dapper
{
    internal static class DbDataReaderFactory
    {
        internal static DbDataReader DbDataReader<TResult>(Func<TResult> result)
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
                var properties =
                    type.GetProperties()
                        .Where(info => info.CanRead &&
                                      (info.PropertyType.IsPrimitive ||
                                       info.PropertyType == typeof(DateTime) ||
                                       info.PropertyType == typeof(DateTimeOffset) ||
                                       info.PropertyType == typeof(decimal) ||
                                       info.PropertyType == typeof(Guid) ||
                                       info.PropertyType == typeof(string) ||
                                       info.PropertyType == typeof(TimeSpan) ||
                                       info.PropertyType == typeof(byte[])))
                        .ToList();

                var columns = properties.Select(property => new DataColumn(property.Name, property.PropertyType)).ToArray();

                dataTable.Columns.AddRange(columns);

                var valuesFactory = properties.Select(info => (Func<object, object>)info.GetValue).ToArray();

                foreach (var element in enumerable)
                    dataTable.Rows.Add(valuesFactory.Select(getValue => getValue(element)).ToArray());
            }

            return new DataTableReader(dataTable);
        }
    }
}