using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Util.Extensions
{

    public static class MapperExtensions
    {
        /// <summary>
        /// 忽略名称相同但是类型不同的属性
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDestination> IgnoreDifferentTypeProperty<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression)
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);
            var sourceProperties = sourceType.GetProperties();
            var destProperties = destinationType.GetProperties();
            foreach (var destItem in destProperties)
            {
                foreach (var sourceItem in sourceProperties)
                {
                    if (destItem.Name == sourceItem.Name && destItem.PropertyType != sourceItem.PropertyType)
                    {
                        expression.ForMember(destItem.Name, opt => opt.Ignore());
                        break;
                    }
                }
            }
            return expression;
        }

        /// <summary>
        /// 忽略名称相同但是类型不同的属性
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="sourceType"></param>
        /// <param name="destType"></param>
        /// <returns></returns>
        public static IMappingExpression IgnoreDifferentTypeProperty(this IMappingExpression expression, Type sourceType, Type destType)
        {
            var sourceProperties = sourceType.GetProperties();
            var destProperties = destType.GetProperties();
            foreach (var destItem in destProperties)
            {
                foreach (var sourceItem in sourceProperties)
                {
                    if (destItem.Name == sourceItem.Name && destItem.PropertyType != sourceItem.PropertyType)
                    {
                        expression.ForMember(destItem.Name, opt => opt.Ignore());
                        break;
                    }
                }
            }
            return expression;
        }

        /// <summary>
        /// 忽略未映射的属性
        /// </summary>
        /// <param name="profile"></param>
        public static void IgnoreUnmapped(this IProfileExpression profile)
        {
            profile.ForAllMaps(IgnoreUnmappedProperties);
        }

        /// <summary>
        /// 忽略未映射的属性
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="filter"></param>
        public static void IgnoreUnmapped(this IProfileExpression profile, Func<TypeMap, bool> filter)
        {
            profile.ForAllMaps((map, expr) =>
            {
                if (filter(map))
                {
                    IgnoreUnmappedProperties(map, expr);
                }
            });
        }

        /// <summary>
        /// 忽略未映射的属性
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        public static void IgnoreUnmapped(this IProfileExpression profile, Type src, Type dest)
        {
            profile.IgnoreUnmapped((TypeMap map) => map.SourceType == src && map.DestinationType == dest);
        }

        /// <summary>
        /// 忽略未映射的属性
        /// </summary>
        /// <typeparam name="TSrc"></typeparam>
        /// <typeparam name="TDest"></typeparam>
        /// <param name="profile"></param>
        public static void IgnoreUnmapped<TSrc, TDest>(this IProfileExpression profile)
        {
            profile.IgnoreUnmapped(typeof(TSrc), typeof(TDest));
        }

        private static void IgnoreUnmappedProperties(TypeMap map, IMappingExpression expr)
        {
            var unmappedPropertyNames = map.GetUnmappedPropertyNames();
            foreach (var propName in unmappedPropertyNames)
            {
                //if (map.SourceType.GetProperty(propName) != null)
                //{
                //    expr.ForSourceMember(propName, opt => opt.DoNotValidate());
                //}
                if (map.DestinationType.GetProperty(propName) != null)
                {
                    expr.ForMember(propName, opt => opt.Ignore());
                }
            }
        }
    }
}
