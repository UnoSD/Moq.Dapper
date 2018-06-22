using System;
using System.Data.Common;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using DataContextType = System.Data.Linq.DataContext;

namespace Moq.DataContext
{
    class LinqProviderInterfaceProxy : RealProxy, IRemotingTypeInfo
    {
        const string SystemDataLinqProviderInterfaceFullName = "System.Data.Linq.Provider.IProvider";

        readonly Type _type;
        readonly DbConnection _connection;

        public LinqProviderInterfaceProxy(DbConnection connection) :
            this(typeof(DataContextType).Assembly
                                        .GetType(SystemDataLinqProviderInterfaceFullName), connection)
        { }

        LinqProviderInterfaceProxy(Type type, DbConnection connection) : base(type)
        {
            _type = type;
            _connection = connection;
        }

        public override IMessage Invoke(IMessage msg)
        {
            if (!(msg is IMethodCallMessage call))
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