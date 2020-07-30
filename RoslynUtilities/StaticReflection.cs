using System;
using System.Linq.Expressions;
using System.Reflection;

namespace monacoEditorCSharp.Utilities
{
    public static class StaticReflection
    {
        public static ConstructorInfo GetConstructorInfo<T>(Expression<Func<T>> expression)
        {
            NewExpression body = expression.Body as NewExpression;
            if (body == null)
            {
                throw new InvalidOperationException("Invalid expression form passed");
            }
            return body.Constructor;
        }

        public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expression)
        {
            return GetMethodInfo((LambdaExpression)expression);
        }

        public static MethodInfo GetMethodInfo(Expression<Action> expression)
        {
            return GetMethodInfo((LambdaExpression)expression);
        }

        private static MethodInfo GetMethodInfo(LambdaExpression lambda)
        {
            GuardProperExpressionForm(lambda.Body);
            MethodCallExpression body = (MethodCallExpression)lambda.Body;
            return body.Method;
        }

        public static MethodInfo GetPropertyGetMethodInfo<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            MethodInfo getMethod = GetPropertyInfo<T, TProperty>(expression).GetGetMethod();
            if (getMethod == null)
            {
                throw new InvalidOperationException("Invalid expression form passed");
            }
            return getMethod;
        }

        private static PropertyInfo GetPropertyInfo<T, TProperty>(LambdaExpression lambda)
        {
            MemberExpression body = lambda.Body as MemberExpression;
            if (body == null)
            {
                throw new InvalidOperationException("Invalid expression form passed");
            }
            PropertyInfo member = body.Member as PropertyInfo;
            if (member == null)
            {
                throw new InvalidOperationException("Invalid expression form passed");
            }
            return member;
        }

        public static MethodInfo GetPropertySetMethodInfo<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            MethodInfo setMethod = GetPropertyInfo<T, TProperty>(expression).GetSetMethod();
            if (setMethod == null)
            {
                throw new InvalidOperationException("Invalid expression form passed");
            }
            return setMethod;
        }

        private static void GuardProperExpressionForm(Expression expression)
        {
            if (expression.NodeType != ExpressionType.Call)
            {
                throw new InvalidOperationException("Invalid expression form passed");
            }
        }
    }
}
