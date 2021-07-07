using System;
using System.Linq.Expressions;
using System.Reflection;

namespace CloneExtensionsEx.ExpressionFactories
{
    class ArrayPrimitiveTypeExpressionFactory<T> : ArrayExpressionFactory<T>
    {
        private static MethodInfo _copyTo = typeof(Array).GetRuntimeMethod(
            "CopyTo",
            new Type[2] { typeof(Array), typeof(int) });

        public ArrayPrimitiveTypeExpressionFactory(
            ParameterExpression source,
            Expression target,
            ParameterExpression excludeNames,
            ParameterExpression flags,
            ParameterExpression initializers,
            ParameterExpression createObjectFun,
            ParameterExpression customResolveFun,
            ParameterExpression clonedObjects)
            : base(source, target, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects)
        {
        }

        protected override Expression GetCloneExpression(Func<Type, Expression, MemberInfo, Type, Expression, Expression> getItemCloneExpression)
        {
            var assign = Expression.Assign(
                Target,
                _newArray);

            var copy = Expression.Call(
                Source,
                _copyTo,
                Target,
                Expression.Constant(0));

            return Expression.Block(
                assign,
                GetAddToClonedObjectsExpression(),
                copy);
        }
    }
}