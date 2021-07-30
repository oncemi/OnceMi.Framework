using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class ISearchRequest : IRequest
    {
        /// <summary>
        /// 排序
        /// </summary>
        [JsonIgnore]
        public List<OrderByModel> OrderByModels
        {
            get
            {
                if (this.OrderBy == null || this.OrderBy.Length == 0)
                {
                    return new List<OrderByModel>();
                }
                List<OrderByModel> result = new List<OrderByModel>();
                foreach (var orderByItem in OrderBy)
                {
                    if (string.IsNullOrEmpty(orderByItem) 
                        || orderByItem.Equals("null", StringComparison.OrdinalIgnoreCase))
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
                                result.Add(new OrderByModel(items[j], items[orderMethodIndex[i]]));
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
        /// 排序字段和排序方式，URL参数示例：orderby=name,desc
        /// 可包含多个orderby参数
        /// </summary>
        public string[] OrderBy { get; set; }

        /// <summary>
        /// 查询关键字
        /// </summary>
        public string Search { get; set; }

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
