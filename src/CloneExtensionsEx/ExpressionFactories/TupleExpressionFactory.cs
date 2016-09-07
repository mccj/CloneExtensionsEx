using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace CloneExtensionsEx.ExpressionFactories
{
    class TupleExpressionFactory<T> : DeepShallowExpressionFactoryBase<T>
    {
        Type[] _genericTypes;

        public TupleExpressionFactory(ParameterExpression source, Expression target, ParameterExpression excludeNames, ParameterExpression flags, ParameterExpression initializers, ParameterExpression createObjectFun, ParameterExpression customResolveFun, ParameterExpression clonedObjects)
            : base(source, target, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects)
        {
            _genericTypes = typeof(T).GetGenericArguments();
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

        protected override Expression GetCloneExpression(Func<Type, Expression, MemberInfo, Type, Expression, Expression> getItemCloneExpression)
        {
            var itemsCloneExpressions = new Expression[_genericTypes.Length];

            // Can't loop to 8, because instead of Item8 the last one is called Rest
            var loopCount = Math.Min(_genericTypes.Length, 7);
            for (int i = 0; i < loopCount; i++)
            {
                itemsCloneExpressions[i] = getItemCloneExpression(typeof(T), Source, null,
                                            _genericTypes[i],
                                            Expression.Property(
                                                Source,
                                                "Item" + (i + 1).ToString(CultureInfo.InvariantCulture)));
            }

            // add Rest expression if it's necessary
            if (_genericTypes.Length == 8)
                itemsCloneExpressions[7] = getItemCloneExpression(typeof(T), Source, null,
                                            _genericTypes[7],
                                            Expression.Property(
                                                Source,
                                                "Rest"));

            var constructor = typeof(T).GetConstructors()[0];

            return
                Expression.Assign(
                    Target,
                    Expression.New(
                        constructor,
                        itemsCloneExpressions
                    ));
        }
    }
}
