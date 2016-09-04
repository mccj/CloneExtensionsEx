using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace CloneExtensionsEx.ExpressionFactories
{
    abstract class DeepShallowExpressionFactoryBase<T> : ExpressionFactoryBase<T>
    {
        public DeepShallowExpressionFactoryBase(ParameterExpression source, Expression target, ParameterExpression excludeNames, ParameterExpression flags, ParameterExpression initializers, ParameterExpression createObjectFun, ParameterExpression customResolveFun, ParameterExpression clonedObjects)
            : base(source, target, excludeNames, flags, initializers, createObjectFun,customResolveFun, clonedObjects)
        {
        }

        public abstract override bool AddNullCheck { get; }

        public abstract override bool VerifyIfAlreadyClonedByReference { get; }

        public override bool IsDeepCloneDifferentThanShallow
        {
            get
            {
                return true;
            }
        }

        public override Expression GetDeepCloneExpression()
        {
            return GetCloneExpression(GetCloneMethodCall);
        }

        public override Expression GetShallowCloneExpression()
        {
            return GetCloneExpression(SimpleReturnItemExpression);
        }

        protected abstract Expression GetCloneExpression(Func<Type, Expression, MemberInfo, Type, Expression, Expression> getItemCloneExpression);

        protected Expression GetAddToClonedObjectsExpression()
        {
            return VerifyIfAlreadyClonedByReference
                   ? (Expression)Expression.Call(ClonedObjects, "Add", new Type[] { }, Source, Target)
                   : Expression.Empty();
        }

        private Expression SimpleReturnItemExpression(Type type, Expression source, System.Reflection.MemberInfo info, Type propertyType, Expression propertySource)
        {
            return propertySource;
        }
    }
}
