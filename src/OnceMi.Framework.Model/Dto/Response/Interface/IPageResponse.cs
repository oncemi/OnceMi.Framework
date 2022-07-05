using System.Collections.Generic;

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
