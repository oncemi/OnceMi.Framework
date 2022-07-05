using System.Collections.Generic;

namespace OnceMi.Framework.Model.Dto
{
    public class ICascaderResponse : IResponse
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 父Id
        /// </summary>
        public long? ParentId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 子项
        /// </summary>
        public List<ICascaderResponse> Children { get; set; }
    }
}
