using System;
using System.Collections.Generic;

namespace CloneExtensionsEx
{
    public static class CloneFactory
    {
        private const CloningFlags _defaultFlags
            = CloningFlags.Fields | CloningFlags.Properties | CloningFlags.CollectionItems;

        private static HashSet<Type> _knownImmutableTypes = new HashSet<Type>() {
            typeof(string), typeof(DateTime), typeof(TimeSpan)
        };

        private static IDictionary<Type, Func<object, object>> _customInitializers = new Dictionary<Type, Func<object, object>>();

        public static CloningFlags DefaultFlags
        {
            get { return _defaultFlags; }
        }

        public static IEnumerable<Type> KnownImmutableTypes
        {
            get { return _knownImmutableTypes; }
        }

        public static IDictionary<Type, Func<object, object>> CustomInitializers
        {
            get { return _customInitializers; }
        }

        public static T GetClone<T>(this T source)
        {
            return GetClone(source, _defaultFlags);
        }

        public static T GetClone<T>(this T source, CloningFlags flags)
        {
            return GetClone(source, flags, CustomInitializers);
        }

        public static T GetClone<T>(this T source, IDictionary<Type, Func<object, object>> initializers)
        {
            return GetClone(source, _defaultFlags, initializers);
        }
        public static T GetClone<T>(this T source, IDictionary<Type, Func<object, object>> initializers, Action<ResolveArgs> customResolveFun)
        {
            return GetClone(source,
                new string[] { },
                _defaultFlags,
                initializers,
                null,
                customResolveFun);
        }
        public static T GetClone<T>(this T source, Func<Type, object, object> createObjectFun)
        {
            return GetClone(source, createObjectFun, null);
        }
        public static T GetClone<T>(this T source,  Func<Type, object, object> createObjectFun, Action<ResolveArgs> customResolveFun)
        {
            return GetClone(source,
            new string[] { },
            _defaultFlags,
            CustomInitializers,
            createObjectFun,
            customResolveFun);
        }

        public static T GetClone<T>(this T source, CloningFlags flags, IDictionary<Type, Func<object, object>> initializers)
        {
            //return GetClone(source, flags, initializers);
            return GetClone(source,
                new string[] { },
                flags,
                initializers,
                null,
                null);
        }

        public static T GetClone<T>(this T source, string[] excludeNames, CloningFlags flags, IDictionary<Type, Func<object, object>> initializers, Func<Type, object, object> createObjectFun, Action<ResolveArgs> customResolveFun)
        {
            if (initializers == null)
                throw new ArgumentNullException();

            //创建实例的方法
            //return CloneManager<T>.Clone(source, excludeNames, flags, initializers, createObjectFun, clonedObjects);
            return GetClone(source, excludeNames, flags, initializers, createObjectFun, customResolveFun, new Dictionary<object, object>());
        }

        public static T GetClone<T>(this T source, string[] excludeNames, CloningFlags flags, IDictionary<Type, Func<object, object>> initializers, Func<Type, object, object> createObjectFun, Action<ResolveArgs> customResolveFun, Dictionary<object, object> clonedObjects)
        {
            if (clonedObjects == null)
                clonedObjects = new Dictionary<object, object>();
            //var _type = typeof(T);
            //var constructor = _type.GetConstructor(new Type[0]);
            //if (_type.IsAbstract || _type.IsInterface || (!_type.IsValueType && constructor == null)|| _type==typeof(object))
            //{
            //    if (source == null) return default(T);
            //    return (T)typeof(CloneFactory).GetMethod(nameof(GetCloneEx), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance).MakeGenericMethod(source.GetType()).Invoke(null, new object[] { source, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects });
            //}
            //else
            {
                return GetCloneEx<T>(source, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects);
            }
        }

        private static T GetCloneEx<T>(this T source, string[] excludeNames, CloningFlags flags, IDictionary<Type, Func<object, object>> initializers, Func<Type, object, object> createObjectFun, Action<ResolveArgs> customResolveFun, Dictionary<object, object> clonedObjects)
        {
            return CloneManager<T>.Clone(source, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects);
        }
    }
}
