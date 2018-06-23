using System.Data;

namespace Moq.Dapper
{
    static class DataTableExtensions
    {
        internal static DataTableReader ToDataTableReader(this DataTable dataTable) =>
            new DataTableReader(dataTable);
    }
}