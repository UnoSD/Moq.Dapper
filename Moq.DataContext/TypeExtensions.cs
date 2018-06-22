using System;
using System.Collections.Generic;

namespace Moq.Dapper.Test
{
    internal static class TypeExtensions
    {
        internal static IEnumerable<Type> AllBaseTypes(this Type type)
        {
            while (type != null)
            {
                yield return type;

                type = type.BaseType;
            }
        }
    }
}