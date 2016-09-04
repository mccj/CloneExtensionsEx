using System;
using System.Collections.Generic;

namespace CloneExtensionsEx
{
    internal static class CloneManager<T>
    {
        private static Func<T, string[], CloningFlags, IDictionary<Type, Func<object, object>>, Func<Type, object, object>, Action<ResolveArgs>, Dictionary<object, object>, T> _clone;

        private static readonly IDictionary<Type, Func<object, object>> _emptyCustomInitializersDictionary = new Dictionary<Type, Func<object, object>>();

        static CloneManager()
        {
            var factory = new ExpressionFactory<T>();
            _clone = factory.GetCloneFunc();
        }

        internal static T Clone(T source, string[] excludeNames, CloningFlags flags, IDictionary<Type, Func<object, object>> initializers, Func<Type,object, object> createObjectFun, Action<ResolveArgs> customResolveFun, Dictionary<object, object> clonedObjects)
        {
            return _clone(source,excludeNames, flags, initializers, createObjectFun,customResolveFun, clonedObjects);
        }
    }
}
