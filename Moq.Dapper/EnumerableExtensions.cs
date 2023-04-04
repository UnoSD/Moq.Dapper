using Castle.DynamicProxy.Internal;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Numerics;

namespace Moq.Dapper
{
    public static class EnumerableExtensions
    {
        internal static DataTable ToDataTable(this IEnumerable results, Type tableType)
        {
            var dataTable = new DataTable();
            
            bool IsNullable(Type t) =>
                    t.IsGenericType &&
                    t.GetGenericTypeDefinition() == typeof(Nullable<>);

            Type GetDataColumnType(Type source) =>
                IsNullable(source) ?
                    Nullable.GetUnderlyingType(source) :
                    source;

            bool IsADapperQuerySupportedType(Type t) =>
                    t.IsPrimitive ||
                    t.IsEnum ||
                    t == typeof(DateTime) ||
                    t == typeof(DateTimeOffset) ||
                    t == typeof(decimal) ||
                    t == typeof(Guid) ||
                    t == typeof(string) ||
                    t == typeof(TimeSpan) ||
                    t == typeof(byte[]);

            //Dapper does not list BigInteger in it's type map.
            //So, Query<BigInteger> returns 0 for every BigInteger in Response.
            bool IsMatchingType(Type t) =>
                    IsADapperQuerySupportedType(t) ||
                    t == typeof(BigInteger);

            var underlyingType = GetDataColumnType(tableType);

            if (IsADapperQuerySupportedType(underlyingType))
            {
                dataTable.Columns.Add(new DataColumn("Column1", underlyingType));

                foreach (var element in results)
                {
                    if(element == null)
                    {
                        dataTable.Rows.Add(DBNull.Value);
                    }
                    else
                    {
                        dataTable.Rows.Add(element);
                    }
                }
                    
            }
            else
            {
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