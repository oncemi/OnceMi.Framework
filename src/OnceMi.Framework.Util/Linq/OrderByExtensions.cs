using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Util.Linq
{
    public static class OrderByExtensions
    {
        private static PropertyInfo GetPropertyInfo(Type objType, string name)
        {
            var properties = objType.GetProperties();
            var matchedProperty = properties.FirstOrDefault(p => p.Name == name);
            if (matchedProperty == null)
                throw new ArgumentException("name");

            return matchedProperty;
        }

        private static LambdaExpression GetOrderExpression(Type objType, PropertyInfo pi)
        {
            var paramExpr = Expression.Parameter(objType);
            var propAccess = Expression.PropertyOrField(paramExpr, pi.Name);
            var expr = Expression.Lambda(propAccess, paramExpr);
            return expr;
        }

        private static IOrderedEnumerable<T> DoOrder<T>(IEnumerable<T> query, string name, string Type)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == Type && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IOrderedEnumerable<T>)genericMethod.Invoke(null, new object[] { query, expr.Compile() });
        }

        private static IOrderedQueryable<T> DoOrder<T>(IQueryable<T> query, string name, string Type)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == Type && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IOrderedQueryable<T>)genericMethod.Invoke(null, new object[] { query, expr.Compile() });
        }

        #region Order

        public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> query, string name)
        {
            return DoOrder(query, name, nameof(OrderBy));
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, string name)
        {
            return DoOrder(query, name, nameof(OrderBy));
        }

        public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> query, string name)
        {
            return DoOrder(query, name, nameof(OrderByDescending));
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string name)
        {
            return DoOrder(query, name, nameof(OrderByDescending));
        }

        #endregion


        #region ThenBy

        public static IOrderedEnumerable<T> ThenBy<T>(this IEnumerable<T> query, string name)
        {
            return DoOrder(query, name, nameof(ThenBy));
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IQueryable<T> query, string name)
        {
            return DoOrder(query, name, nameof(ThenBy));
        }

        public static IOrderedEnumerable<T> ThenByDescending<T>(this IEnumerable<T> query, string name)
        {
            return DoOrder(query, name, nameof(ThenByDescending));
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IQueryable<T> query, string name)
        {
            return DoOrder(query, name, nameof(ThenByDescending));
        }

        #endregion

    }
}
