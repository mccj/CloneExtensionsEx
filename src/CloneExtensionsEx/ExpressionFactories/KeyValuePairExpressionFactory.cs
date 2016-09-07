﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CloneExtensionsEx.ExpressionFactories
{
    class KeyValuePairExpressionFactory<T> : DeepShallowExpressionFactoryBase<T>
    {
        private ConstructorInfo _constructor;
        private Type _keyType;
        private Type _valueType;

        public KeyValuePairExpressionFactory(ParameterExpression source, Expression target, ParameterExpression excludeNames, ParameterExpression flags, ParameterExpression initializers, ParameterExpression createObjectFun, ParameterExpression customResolveFun, ParameterExpression clonedObjects)
            : base(source, target, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects)
        {
            var type = typeof(T);
            _constructor = type.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 2);
            _keyType = type.GetGenericArguments()[0];
            _valueType = type.GetGenericArguments()[1];
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
            return
                Expression.Assign(
                    Target,
                    Expression.New(
                        _constructor,
                        getItemCloneExpression(typeof(T), Source, null, _keyType, Expression.Property(Source, "Key")),
                        getItemCloneExpression(typeof(T), Source, null, _valueType, Expression.Property(Source, "Value"))));
        }
    }
}
