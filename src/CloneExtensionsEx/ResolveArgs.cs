using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace CloneExtensionsEx
{
    public class ResolveArgs
    {
        private Dictionary<object, object> clonedObjects;

        public ResolveArgs(object source, Type sourceType, MemberInfo info, object propertySource, Type propertySourceType, string[] excludeNames, CloningFlags flags, IDictionary<Type, Func<object, object>> initializers, Func<Type, object, object> createObjectFun, Action<ResolveArgs> customResolveFun, Dictionary<object, object> clonedObjects)
        {
            this.Source = source;
            this.SourceType = sourceType;
            this.Member = info;
            this.PropertySource = propertySource;
            this.PropertySourceType = propertySourceType;
            this.ExcludeNames = excludeNames;
            this.CloningFlags = flags;
            this.Initializers = initializers;
            this.createObjectFun = createObjectFun;
            this.customResolveFun = customResolveFun;
            this.clonedObjects = clonedObjects;
        }

        public ResolveArgs()
        {
        }

        public object NewValue { get; set; } = null;
        public bool IsResolve { get; set; } = false;
        public object Source { get; }
        public Type SourceType { get; }
        public MemberInfo Member { get; }
        public object PropertySource { get; }
        public Type PropertySourceType { get; }
        public string[] ExcludeNames { get; }
        public CloningFlags CloningFlags { get; }
        public IDictionary<Type, Func<object, object>> Initializers { get; }
        public Func<Type, object, object> createObjectFun { get; }
        public Action<ResolveArgs> customResolveFun { get; }
        public object GetClone(object source)
        {
            var getCloneMethod = typeof(ResolveArgs).GetRuntimeMethod(nameof(GetCloneEx), new[] { typeof(string) }/*BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance*/);
            return getCloneMethod.MakeGenericMethod(PropertySourceType).Invoke(this, new object[] { source });
        }
        public object GetClone(object source, Type type)
        {
            var getCloneMethod = typeof(ResolveArgs).GetRuntimeMethod(nameof(GetCloneEx), new[] { typeof(string) }/*BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance*/);
            return getCloneMethod.MakeGenericMethod(type).Invoke(this, new object[] { source });
        }
        private T GetCloneEx<T>(T source)
        {
            return GetClone(source, this.ExcludeNames, this.CloningFlags, this.Initializers, this.createObjectFun, this.customResolveFun);
        }
        public T GetClone<T>(T source, string[] excludeNames, CloningFlags flags, IDictionary<Type, Func<object, object>> initializers, Func<Type, object, object> createObjectFun, Action<ResolveArgs> customResolveFun)
        {
            return CloneFactory.GetClone(source, excludeNames, flags, initializers, createObjectFun, customResolveFun, this.clonedObjects);
        }
    }
}