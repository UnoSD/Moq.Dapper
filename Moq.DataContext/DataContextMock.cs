using System.Data.Common;
using System.Linq;
using System.Reflection;
using Moq.Dapper.Test;
using DataContextType = System.Data.Linq.DataContext;

namespace Moq.DataContext
{
    public class DataContextMock<T> : Mock<T> where T : DataContextType, new()
    {
        private readonly Mock<DbConnection> _connectionMock;

        public DataContextMock(Mock<DbConnection> connectionMock) => _connectionMock = connectionMock;

        public override T Object => CreateDataContextMock();

        public T CreateDataContextMock()
        {
            var proxy = new LinqProviderInterfaceProxy(_connectionMock.Object);

            var dataContext = new T();

            var providerField = typeof(T).AllBaseTypes()
                                         .First(type => type == typeof(DataContextType))
                                         .GetField("provider", BindingFlags.Instance | BindingFlags.NonPublic);

            providerField?.SetValue(dataContext, proxy.GetTransparentProxy());

            return dataContext;
        }
    }
}
