using System;
using System.Linq.Expressions;

namespace CloneExtensionsEx.ExpressionFactories
{
    class PrimitiveTypeExpressionFactory<T> : ExpressionFactoryBase<T>
    {
        public PrimitiveTypeExpressionFactory(ParameterExpression source, Expression target, ParameterExpression excludeNames, ParameterExpression flags, ParameterExpression initializers, ParameterExpression createObjectFun, ParameterExpression customResolveFun, ParameterExpression clonedObjects)
            : base(source, target, excludeNames, flags, initializers,createObjectFun, customResolveFun, clonedObjects)
        {
        }

        public override bool IsDeepCloneDifferentThanShallow
        {
            get
            {
                return false;
            }
        }

        public override bool AddNullCheck
        {
            get
            {
                return false;
            }
        }

        public override bool VerifyIfAlreadyClonedByReference
        {
            get
            {
                return false;
            }
        }

        public override Expression GetDeepCloneExpression()
        {
            return Expression.Assign(Target, Source);
        }

        public override Expression GetShallowCloneExpression()
        {
            throw new InvalidOperationException();
        }
    }
}
