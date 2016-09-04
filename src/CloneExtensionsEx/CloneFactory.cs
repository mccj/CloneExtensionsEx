﻿using System;
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

        public static T GetClone<T>(this T source, CloningFlags flags, IDictionary<Type, Func<object, object>> initializers)
        {
            if (initializers == null)
                throw new ArgumentNullException();

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
            //创建实例的方法
            //return CloneManager<T>.Clone(source, excludeNames, flags, initializers, createObjectFun, clonedObjects);
            return GetClone(source, excludeNames, flags, initializers, createObjectFun, customResolveFun, new Dictionary<object, object>());
        }

        internal static T GetClone<T>(this T source, string[] excludeNames, CloningFlags flags, IDictionary<Type, Func<object, object>> initializers, Func<Type, object, object> createObjectFun, Action<ResolveArgs> customResolveFun, Dictionary<object, object> clonedObjects)
        {
            return CloneManager<T>.Clone(source, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects);
        }
    }
}
