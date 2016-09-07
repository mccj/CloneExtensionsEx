using System;
using System.Linq.Expressions;

namespace CloneExtensionsEx.ExpressionFactories
{
    class NullableExpressionFactory<T> : ExpressionFactoryBase<T>
    {
        private Type _structType = typeof(T).GetGenericArguments()[0];

        public NullableExpressionFactory(ParameterExpression source, Expression target, ParameterExpression excludeNames, ParameterExpression flags, ParameterExpression initializers, ParameterExpression createObjectFun, ParameterExpression customResolveFun, ParameterExpression clonedObjects)
            : base(source, target, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects)
        {
        }

        public override bool IsDeepCloneDifferentThanShallow
        {
            get
            {
                return true;
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
            var structType = typeof(T).GetGenericArguments()[0];

            var cloneCall = GetCloneMethodCall(typeof(T),Source,null,structType, Expression.Property(Source, "Value"));
            var newNullable = Expression.New(typeof(T).GetConstructor(new[] { _structType }), cloneCall);

            return
                Expression.IfThenElse(
                    Expression.Equal(
                        Expression.Property(Source, "HasValue"),
                        Expression.Constant(false)),
                    Expression.Assign(Target, Expression.Constant(null, typeof(T))),
                    Expression.Assign(Target, newNullable));
        }

        public override Expression GetShallowCloneExpression()
        {
            return Expression.Assign(Target, Source);
        }
    }
}
