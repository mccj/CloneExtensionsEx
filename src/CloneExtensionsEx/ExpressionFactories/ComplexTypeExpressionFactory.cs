using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CloneExtensionsEx.ExpressionFactories
{
    class ComplexTypeExpressionFactory<T> : DeepShallowExpressionFactoryBase<T>
    {
        Type _type;
        Expression _typeExpression;
        public ComplexTypeExpressionFactory(ParameterExpression source, Expression target, ParameterExpression excludeNames, ParameterExpression flags, ParameterExpression initializers, ParameterExpression createObjectFun, ParameterExpression customResolveFun, ParameterExpression clonedObjects)
            : base(source, target, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects)
        {
            _type = typeof(T);
            _typeExpression = Expression.Constant(_type, typeof(Type));
        }

        public override bool AddNullCheck
        {
            get
            {
                return !_type.IsValueType;
            }
        }

        public override bool VerifyIfAlreadyClonedByReference
        {
            get
            {
                return !_type.IsValueType;
            }
        }

        protected override Expression GetCloneExpression(Func<Type, Expression, MemberInfo, Type, Expression, Expression> getItemCloneExpression)
        {
            var initialization = GetInitializationExpression();
            var fields =
                Expression.IfThen(
                    Helpers.GetCloningFlagsExpression(CloningFlags.Fields, Flags),
                    GetFieldsCloneExpression(getItemCloneExpression)
                );
            var properties =
                Expression.IfThen(
                    Helpers.GetCloningFlagsExpression(CloningFlags.Properties, Flags),
                    GetPropertiesCloneExpression(getItemCloneExpression)
                );

            var collectionItems = GetCollectionItemsExpression(getItemCloneExpression);

            return Expression.Block(initialization, GetAddToClonedObjectsExpression(), fields, properties, collectionItems);
        }
        /// <summary>
        /// 获取初始化表达式
        /// </summary>
        /// <returns></returns>
        private Expression GetInitializationExpression()
        {
            // initializers.ContainsKey method call
            var containsKeyCall = Expression.Call(Initializers, "ContainsKey", null, _typeExpression);

            // initializer delegate invoke
            var dictIndex = Expression.Property(Initializers, "Item", _typeExpression);
            var funcInvokeCall = Expression.Call(dictIndex, "Invoke", null, Expression.Convert(Source, typeof(object)));
            var initializerCall = Expression.Convert(funcInvokeCall, _type);

            // parameterless constructor
            var constructor = _type.GetConstructor(new Type[0]);
            var initType =
                (_type.IsAbstract || _type.IsInterface || (!_type.IsValueType && constructor == null)) ?
                Helpers.GetThrowInvalidOperationExceptionExpression(_type) :
                Expression.Assign(
                    Target,
                    _type.IsValueType ? (Expression)Source : Expression.New(_type)
                );

            return Expression.IfThenElse(
                containsKeyCall,
                Expression.Assign(Target, initializerCall),
                ////////////////////////////////////////
                Expression.IfThenElse(
                    Expression.NotEqual(CreateObjectFun, Expression.Constant(null)),
                        Expression.Assign(
                            Target,
                            Expression.Convert(Expression.Call(CreateObjectFun, "Invoke", null, _typeExpression, Expression.Convert(Source, typeof(object))), _type)
                        ),
                    initType
                )
            );
        }

        private Expression GetFieldsCloneExpression(Func<Type, Expression, MemberInfo, Type, Expression, Expression> getItemCloneExpression)
        {
            var fields = from f in _type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                         where !f.GetCustomAttributes(typeof(NonClonedAttribute), true).Any()
                         where !f.IsInitOnly
                         select new Member(f, f.FieldType);

            return GetMembersCloneExpression(fields.ToArray(), getItemCloneExpression);
        }
        private PropertyInfo[] GetProperties(Type _type)
        {
            if (_type.IsInterface)
            {
                return _type.GetInterfaces().Concat(new[] { _type }).Distinct().SelectMany(f => f.GetProperties()).Distinct().ToArray();
            }
            else
            {
                return _type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            }
        }
        private Expression GetPropertiesCloneExpression(Func<Type, Expression, MemberInfo, Type, Expression, Expression> getItemCloneExpression)
        {
            // get all public properties with public setter and getter, which are not indexed properties
            var properties = from p in GetProperties(_type)
                             let setMethod = p.GetSetMethod(false)
                             let getMethod = p.GetGetMethod(false)
                             where !p.GetCustomAttributes(typeof(NonClonedAttribute), true).Any()
                             where setMethod != null && getMethod != null && !p.GetIndexParameters().Any()
                             select new Member(p, p.PropertyType);

            return GetMembersCloneExpression(properties.ToArray(), getItemCloneExpression);
        }

        private Expression GetMembersCloneExpression(Member[] members, Func<Type, Expression, MemberInfo, Type, Expression, Expression> getItemCloneExpression)
        {
            if (!members.Any())
                return Expression.Empty();

            return Expression.Block(
                members.Select(m =>
                    Expression.Assign(
                        Expression.MakeMemberAccess(Target, m.Info),
                        Helpers.处理ExcludeNames(getItemCloneExpression(_type, Source, m.Info, m.Type, Expression.MakeMemberAccess(Source, m.Info)), ExcludeNames, m)
                        )));
        }

        private Expression GetCollectionItemsExpression(Func<Type, Expression, MemberInfo, Type, Expression, Expression> getItemCloneExpression)
        {
            var collectionType = _type.GetInterfaces()
                                      .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>));
            if (collectionType == null)
                return Expression.Empty();

            return Expression.IfThen(
                Helpers.GetCloningFlagsExpression(CloningFlags.CollectionItems, Flags),
                GetForeachAddExpression(collectionType));
        }

        private Expression GetForeachAddExpression(Type collectionType)
        {
            var collection = Expression.Variable(collectionType);
            var itemType = collectionType.GetGenericArguments().First();
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(itemType);
            var enumerator = Expression.Variable(enumeratorType);
            var getEnumeratorCall = Expression.Call(Expression.Convert(Source, enumerableType), "GetEnumerator", null);
            var assignToEnumerator = Expression.Assign(enumerator, Expression.Convert(getEnumeratorCall, enumeratorType));
            var assignToCollection = Expression.Assign(collection, Expression.Convert(Target, collectionType));
            var moveNextCall = Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext"));
            var currentProperty = Expression.Property(enumerator, "Current");
            var breakLabel = Expression.Label();

            return Expression.Block(
                new[] { enumerator, collection },
                assignToEnumerator,
                assignToCollection,
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.NotEqual(moveNextCall, Expression.Constant(false, typeof(bool))),
                        Expression.Call(collection, "Add", null,
                            GetCloneMethodCall(_type, Source, null, itemType, currentProperty)),
                        Expression.Break(breakLabel)
                    ),
                    breakLabel
                )
            );
        }

    }
}
