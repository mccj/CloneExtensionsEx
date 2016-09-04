using System;
using System.Collections.Generic;

namespace CloneExtensionsEx
{
    public class ResolveArgs
    {
        private IDictionary<Type, Func<object, object>> initializers;
        public ResolveArgs(object source, Type sourceType, string[] excludeNames, CloningFlags flags, IDictionary<Type, Func<object, object>> initializers)
        {
            this.Source = source;
            this.SourceType = sourceType;
            this.CloningFlags = flags;
            this.initializers = initializers;
        }

        public object NewValue { get; set; } = null;
        public bool IsResolve { get; set; } = false;
        public object Source { get; }
        public string[] ExcludeNames { get; }
        private CloningFlags CloningFlags { get; }
        public Type SourceType { get; }
        public System.Reflection.MemberInfo Member { get; }

    }
}
