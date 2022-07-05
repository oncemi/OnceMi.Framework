using FreeSql;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Util.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace OnceMi.Framework.IRepository
{
    public static class FreeSqlOrderExtension
    {
        /// <summary>
        /// 根据传入的排序参数，进行排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="select"></param>
        /// <param name="orderBys"></param>
        /// <returns></returns>
        public static ISelect<T> OrderBy<T>(this ISelect<T> select, IEnumerable<OrderRule> orderBys) where T : class
        {
            if (orderBys == null || orderBys.Count() == 0)
                return select;

            var fields = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (fields == null || fields.Length == 0)
                return select;

            foreach (var orderItem in orderBys)
            {
                foreach (var fieldItem in fields)
                {
                    if (orderItem.Filed.Equals(fieldItem.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        select.OrderByPropertyName(fieldItem.Name, orderItem.OrderBy == "asc");
                        break;
                    }
                }
            }
            return select;
        }

        /// <summary>
        /// 根据传入的排序参数，进行排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="select"></param>
        /// <param name="orderBys"></param>
        /// <returns></returns>
        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> select, IEnumerable<OrderRule> orderBys) where T : class
        {
            if (orderBys == null || orderBys.Count() == 0)
                return select;

            var fields = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (fields == null || fields.Length == 0)
                return select;
            IOrderedEnumerable<T> order = null;
            foreach (var orderItem in orderBys)
            {
                foreach (var fieldItem in fields)
                {
                    if (orderItem.Filed.Equals(fieldItem.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        if (order == null)
                        {
                            if (orderItem.OrderBy == "asc")
                            {
                                order = select.OrderBy(fieldItem.Name);
                            }
                            else
                            {
                                order = select.OrderByDescending(fieldItem.Name);
                            }
                        }
                        else
                        {
                            if (orderItem.OrderBy == "asc")
                            {
                                order = order.ThenBy(fieldItem.Name);
                            }
                            else
                            {
                                order = order.ThenByDescending(fieldItem.Name);
                            }
                        }
                        break;
                    }
                }
            }
            return order == null ? select : order;
        }
    }
}