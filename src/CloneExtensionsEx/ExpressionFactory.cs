﻿using CloneExtensionsEx.ExpressionFactories;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CloneExtensionsEx
{
    class ExpressionFactory<T>
    {
        private static Type _type = typeof(T);
        private static Expression _typeExpression = Expression.Constant(_type, typeof(Type));
        private static Expression nullConstant = null;

        private static ParameterExpression source = Expression.Parameter(_type, "source");
        private static ParameterExpression flags = Expression.Parameter(typeof(CloningFlags), "flags");
        private static ParameterExpression initializers = Expression.Parameter(typeof(IDictionary<Type, Func<object, object>>), "initializers");
        private static ParameterExpression clonedObjects = Expression.Parameter(typeof(Dictionary<object, object>), "clonedObjects");
        private static ParameterExpression createObjectFun = Expression.Parameter(typeof(Func<Type, object, object>), "createObjectFun");
        private static ParameterExpression customResolveFun = Expression.Parameter(typeof(Action<ResolveArgs>), "customResolveFun");
        private static ParameterExpression excludeNames = Expression.Parameter(typeof(string[]), "excludeNames");

        private static ParameterExpression target = Expression.Variable(_type, "target");

        static ExpressionFactory()
        {
            Initialize();
        }

        public static Expression<Func<T, string[], CloningFlags, IDictionary<Type, Func<object, object>>, Func<Type, object, object>, Action<ResolveArgs>, Dictionary<object, object>, T>> CloneExpression { get; private set; }

        internal Func<T, string[], CloningFlags, IDictionary<Type, Func<object, object>>, Func<Type, object, object>, Action<ResolveArgs>, Dictionary<object, object>, T> GetCloneFunc()
        {
            return CloneExpression.Compile();
        }

        public static void Initialize()
        {
            var returnLabel = Expression.Label(_type);

            var expressionFactory = GetExpressionFactory(source, target, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects, returnLabel);

            if (expressionFactory.AddNullCheck || expressionFactory.VerifyIfAlreadyClonedByReference)
                nullConstant = Expression.Constant(null, _type);

            Expression cloneLogic;
            if (expressionFactory.IsDeepCloneDifferentThanShallow)
            {
                cloneLogic =
                    Expression.IfThenElse(
                        Expression.Not(Helpers.GetCloningFlagsExpression(CloningFlags.Shallow, flags)),
                        GetFromClonedObjectsOrCallDeepClone(expressionFactory),
                        expressionFactory.GetShallowCloneExpression()
                    );
            }
            else
            {
                cloneLogic = expressionFactory.GetDeepCloneExpression();
            }

            Expression cloneExpression = cloneLogic;
            if (expressionFactory.AddNullCheck)
            {
                cloneExpression =
                    Expression.IfThenElse(
                        Expression.Equal(source, nullConstant),
                        Expression.Assign(target, nullConstant),
                        cloneLogic
                        );
            }

            var block = Expression.Block(new[] { target },
                new Expression[] { cloneExpression, Expression.Label(returnLabel, target) });

            CloneExpression =
                Expression.Lambda<Func<T, string[], CloningFlags, IDictionary<Type, Func<object, object>>, Func<Type, object, object>, Action<ResolveArgs>, Dictionary<object, object>, T>>(
                    block,
                    new[] { source, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects });
        }

        private static Expression GetFromClonedObjectsOrCallDeepClone(IExpressionFactory<T> expressionFactory)
        {
            if (expressionFactory.VerifyIfAlreadyClonedByReference)
            {
                var getFromCollectionCall = Expression.Call(typeof(Helpers), "GetFromClonedObjects", new[] { _type }, new Expression[] { clonedObjects, source });
                var fromClonedObjects = Expression.Variable(_type, "fromClonedObjects");
                var assignFromCall = Expression.Assign(fromClonedObjects, getFromCollectionCall);

                var ifElse =
                    Expression.IfThenElse(
                        Expression.NotEqual(fromClonedObjects, nullConstant),
                            Expression.Assign(target, fromClonedObjects),
                            expressionFactory.GetDeepCloneExpression());

                return Expression.Block(new[] { fromClonedObjects },
                    assignFromCall, ifElse);
            }
            else
            {
                return expressionFactory.GetDeepCloneExpression();
            }
        }

        private static IExpressionFactory<T> GetExpressionFactory(ParameterExpression source, Expression target, ParameterExpression excludeNames, ParameterExpression flags, ParameterExpression initializers, ParameterExpression createObjectFun, ParameterExpression customResolveFun, ParameterExpression clonedObjects, LabelTarget returnLabel)
        {
            if (_type.IsPrimitiveOrKnownImmutable() || typeof(Delegate).IsAssignableFrom(_type))
            {
                return new PrimitiveTypeExpressionFactory<T>(source, target, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects);
            }
            else if (_type.IsGenericType && _type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return new NullableExpressionFactory<T>(source, target, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects);
            }
            else if (_type.IsArray)
            {
                return new ArrayExpressionFactory<T>(source, target, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects);
            }
            else if (_type.IsGenericType &&
                (_type.GetGenericTypeDefinition() == typeof(Tuple<>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,,,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,,,,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,,,,,>)
                || _type.GetGenericTypeDefinition() == typeof(Tuple<,,,,,,,>)))
            {
                return new TupleExpressionFactory<T>(source, target, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects);
            }
            else if (_type.IsGenericType && _type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                return new KeyValuePairExpressionFactory<T>(source, target, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects);
            }

            return new ComplexTypeExpressionFactory<T>(source, target, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects);
        }
    }
}
