using System;
using System.Linq.Expressions;

namespace CloneExtensionsEx.ExpressionFactories
{
    abstract class ExpressionFactoryBase<T> : IExpressionFactory<T>
    {
        private readonly ParameterExpression _source;
        private readonly Expression _target;
        private readonly ParameterExpression _flags;
        private readonly ParameterExpression _initializers;
        private readonly ParameterExpression _createObjectFun;
        private readonly ParameterExpression _customResolveFun;
        private readonly ParameterExpression _clonedObjects;
        private readonly ParameterExpression _excludeNames;

        protected ExpressionFactoryBase(ParameterExpression source, Expression target, ParameterExpression excludeNames, ParameterExpression flags, ParameterExpression initializers, ParameterExpression createObjectFun, ParameterExpression customResolveFun, ParameterExpression clonedObjects)
        {
            _source = source;
            _target = target;
            _flags = flags;
            _initializers = initializers;
            _createObjectFun = createObjectFun;
            _customResolveFun = customResolveFun;
            _clonedObjects = clonedObjects;
            _excludeNames = excludeNames;
        }

        protected ParameterExpression Source { get { return _source; } }
        protected ParameterExpression Flags { get { return _flags; } }
        protected ParameterExpression Initializers { get { return _initializers; } }
        protected ParameterExpression CreateObjectFun { get { return _createObjectFun; } }
        protected ParameterExpression CustomResolveFun { get { return _customResolveFun; } }
        protected ParameterExpression ClonedObjects { get { return _clonedObjects; } }
        protected ParameterExpression ExcludeNames { get { return _excludeNames; } }
        protected Expression Target { get { return _target; } }

        public abstract bool IsDeepCloneDifferentThanShallow { get; }

        public abstract bool AddNullCheck { get; }

        public abstract bool VerifyIfAlreadyClonedByReference { get; }

        public abstract Expression GetDeepCloneExpression();

        public abstract Expression GetShallowCloneExpression();

        //protected Expression GetCloneMethodCall(Type type, Expression item)
        //{
        //    return Helpers.GetCloneMethodCall(type, item, Flags, Initializers, ClonedObjects);
        //}
        protected Expression GetCloneMethodCall(Type type, Expression source, System.Reflection.MemberInfo info, Type propertyType, Expression propertySource)
        {
            return Helpers.GetCloneMethodCall(type, source, info, propertyType, propertySource, ExcludeNames, Flags, Initializers, CreateObjectFun, CustomResolveFun, ClonedObjects);
        }
    }
}
