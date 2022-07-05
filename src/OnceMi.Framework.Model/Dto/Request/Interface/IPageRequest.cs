using OnceMi.Framework.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace OnceMi.Framework.Model.Dto
{
    /// <summary>
    /// 分页查询
    /// </summary>
    public class IPageRequest : ISearchRequest
    {
        /// <summary>
        /// 当前页
        /// </summary>
        [Required(ErrorMessage = "查询页数不能为空")]
        [Range(0, 1000000, ErrorMessage = "页数查询范围只能在1-1000000之间")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每页大小
        /// </summary>
        [Required(ErrorMessage = "每页查询条数不能为空")]
        [Range(0, int.MaxValue, ErrorMessage = "查询条数必须大于0")]
        public int Size { get; set; } = 20;

        /// <summary>
        /// 排序字段和排序方式，可包含多个orderby参数 |
        /// U参数示例 |
        /// 单个排序字段：orderby=name,desc |
        /// 多个排序字段：orderby=name,desc,createtime,asc
        /// </summary>
        public string[] OrderBy { get; set; }

        /// <summary>
        /// 整理后的排序规则
        /// </summary>
        [JsonIgnore]
        [IgnoreDataMember]
        public List<OrderRule> OrderByParams
        {
            get
            {
                if (this.OrderBy == null || this.OrderBy.Length == 0)
                {
                    return new List<OrderRule>();
                }
                List<OrderRule> result = new List<OrderRule>();
                foreach (var orderByItem in OrderBy)
                {
                    if (string.IsNullOrEmpty(orderByItem) || orderByItem.Equals("null", StringComparison.OrdinalIgnoreCase))
                        continue;
                    List<string> items = orderByItem.Split(",").Where(p => !string.IsNullOrEmpty(p)).ToList();
                    //最后一个不是排序方式结尾，添加排序方式字段
                    if (!IsOrderField(items[^1]))
                    {
                        items.Add("asc");
                    }
                    List<int> orderMethodIndex = new List<int>();
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (IsOrderField(items[i]))
                        {
                            orderMethodIndex.Add(i);
                        }
                    }
                    int start = 0;
                    for (int i = 0; i < orderMethodIndex.Count; i++)
                    {
                        if (IsOrderField(items[start]) && start <= orderMethodIndex[i])
                        {
                            start++;
                            continue;
                        }
                        for (int j = start; j < orderMethodIndex[i]; j++)
                        {
                            if (!result.Any(p => p.Filed.Equals(items[j], StringComparison.OrdinalIgnoreCase)))
                            {
                                result.Add(new OrderRule(items[j], items[orderMethodIndex[i]]));
                            }
                        }
                        start = orderMethodIndex[i] + 1;
                        if (start >= items.Count) break;
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 是否为排序字段（asc，desc）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool IsOrderField(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            if (value.Equals("asc", StringComparison.OrdinalIgnoreCase) || value.Equals("desc", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
    }
}
