using System;
using System.Collections;
using System.Data;
using System.Linq;

namespace Moq.Dapper
{
    public static class EnumerableExtensions
    {
        internal static DataTable ToDataTable(this IEnumerable results, Type tableType)
        {
            var dataTable = new DataTable();

            if (tableType.IsPrimitive || tableType == typeof(string))
            {
                dataTable.Columns.Add();

                foreach (var element in results)
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
                    tableType.GetProperties().
                              Where
                                  (
                                   info => info.CanRead &&
                                           IsMatchingType(info.PropertyType) ||
                                           IsNullable(info.PropertyType) &&
                                           IsMatchingType(Nullable.GetUnderlyingType(info.PropertyType))).
                              ToList();

                var columns = properties.Select(property => new DataColumn(property.Name, GetDataColumnType(property.PropertyType))).ToArray();

                dataTable.Columns.AddRange(columns);

                var valuesFactory = properties.Select(info => (Func<object, object>)info.GetValue).ToArray();

                foreach (var element in results)
                    dataTable.Rows.Add(valuesFactory.Select(getValue => getValue(element)).ToArray());
            }

            return dataTable;
        }
    }
}