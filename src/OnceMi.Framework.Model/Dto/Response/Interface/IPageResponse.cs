using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class IPageResponse<T> where T : IResponse
    {
        /// <summary>
        /// 当前页
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每页大小
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// 总行数
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// 当前页数据
        /// </summary>
        public IEnumerable<T> PageData { get; set; }
    }
}
