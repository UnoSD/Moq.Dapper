using System;
using System.Data.Common;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using DataContextType = System.Data.Linq.DataContext;

namespace Moq.DataContext
{
    public class LinqProviderInterfaceProxy : RealProxy, IRemotingTypeInfo
    {
        private const string SystemDataLinqProviderInterfaceFullName = "System.Data.Linq.Provider.IProvider";

        private readonly Type _type;
        private readonly DbConnection _connection;

        public LinqProviderInterfaceProxy(DbConnection connection) :
            this(typeof(DataContextType).Assembly.GetType(SystemDataLinqProviderInterfaceFullName), connection)
        { }

        private LinqProviderInterfaceProxy(Type type, DbConnection connection) : base(type)
        {
            _type = type;
            _connection = connection;
        }

        public override IMessage Invoke(IMessage msg)
        {
            var call = msg as IMethodCallMessage;

            if (call == null)
                throw new NotSupportedException();

            var method = (MethodInfo)call.MethodBase;

            return method.ReturnType.IsInstanceOfType(_connection) ?
                       new ReturnMessage(_connection, null, 0, call.LogicalCallContext, call) :
                       new ReturnMessage(null, null, 0, call.LogicalCallContext, call);
        }

        public bool CanCastTo(Type fromType, object o) => fromType == _type;

        public string TypeName { get; set; }
    }
}