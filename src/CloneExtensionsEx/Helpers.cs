using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace CloneExtensionsEx
{
    static internal class Helpers
    {
        public static Expression GetCloningFlagsExpression(CloningFlags flags, ParameterExpression parameter)
        {
            var flagExpression = Expression.Convert(Expression.Constant(flags, typeof(CloningFlags)), typeof(byte));

            return Expression.Equal(
                Expression.And(
                    Expression.Convert(parameter, typeof(byte)),
                    flagExpression
                ),
                flagExpression
            );
        }

        public static Expression GetCloneMethodCall(Type type, Expression source, System.Reflection.MemberInfo info, Type propertyType, Expression propertySource, Expression excludeNames, Expression flags, Expression initializers, Expression createObjectFun, Expression customResolveFun, Expression clonedObjects)
        {
            //return Expression.Call(typeof(CloneFactory), nameof(CloneFactory.GetClone), new[] { propertyType }, source, excludeNames, flags, initializers, createObjectFun, clonedObjects);
            return Expression.Call(typeof(Helpers), nameof(Helpers.ExeGetClone), new[] { type, propertyType }, source, Expression.Constant(info, typeof(MemberInfo)), propertySource, excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects);
        }

        public static Expression GetThrowInvalidOperationExceptionExpression(Type type)
        {
            var message = string.Format("You have to provide initialization expression for {0}.", type.FullName);

            return Expression.Throw(
                Expression.New(
                    typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) }),
                    Expression.Constant(message, typeof(string))
                )
            );
        }

        public static T GetFromClonedObjects<T>(Dictionary<object, object> clonedObjects, T source)
        {
            var key = clonedObjects.Keys.FirstOrDefault(k => ReferenceEquals(k, source));
            return key != null ? (T)clonedObjects[key] : default(T);
        }

        public static Expression 处理ExcludeNames(Expression itemCloneExpression, ParameterExpression excludeNames, Member member)
        {

            return Expression.Condition(
                   Expression.Call(typeof(Helpers), nameof(Helpers.IsExcludeName), null, excludeNames, Expression.Constant(member.Info.Name)),
                   Expression.Default(member.Type),
                   itemCloneExpression
                  );
        }
        private static bool IsExcludeName(string[] excludeNames, string name)
        {
            var _excludeNames = excludeNames.Where(f => !string.IsNullOrWhiteSpace(f)).Distinct().ToArray();
            if (_excludeNames.Any(f => f.TrimStart('.') == name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string[] GetExcludeName(string[] excludeNames, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) { return excludeNames; }
            var subExcludeNames = excludeNames.Select(f => System.Text.RegularExpressions.Regex.Replace(f, "\\[\\d+\\]", ""))
                .Where(f => f.StartsWith(name + ".") || (f.StartsWith(".") && f.IndexOf(".", 1) == -1))
                .Select(f => (f.StartsWith(".") && f.IndexOf(".", 1) == -1) ? f : f.Remove(0, name.Length + 1)).ToArray();
            return subExcludeNames;
        }
        public static PropertyT ExeGetClone<T, PropertyT>(T source, MemberInfo info, PropertyT propertySource, string[] excludeNames, CloningFlags flags, IDictionary<Type, Func<object, object>> initializers, Func<Type, object, object> createObjectFun, Action<ResolveArgs> customResolveFun, Dictionary<object, object> clonedObjects)
        {
            if (propertySource == null)
            {
                return default(PropertyT);
            }
            if (clonedObjects.ContainsKey(propertySource))
            {
                return (PropertyT)clonedObjects[propertySource];
            }
            var _excludeNames = GetExcludeName(excludeNames, info?.Name);
            var d = new ResolveArgs(source, typeof(T), info, propertySource, typeof(PropertyT), _excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects);
            customResolveFun?.Invoke(d);
            if (d.IsResolve)
            {
                if (!clonedObjects.ContainsKey(propertySource))
                {
                    clonedObjects.Add(propertySource, d.NewValue);
                }
                return (PropertyT)d.NewValue;
            }
            else
            {
                return CloneFactory.GetClone(propertySource, _excludeNames, flags, initializers, createObjectFun, customResolveFun, clonedObjects);
            }
        }
    }
}
