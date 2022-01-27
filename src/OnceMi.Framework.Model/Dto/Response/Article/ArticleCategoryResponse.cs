using OnceMi.Framework.Entity.Article;
using OnceMi.Framework.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    /// <summary>
    /// 文章分类DTO
    /// </summary>
    [MapperFrom(typeof(ArticleCategory))]
    [MapperTo(typeof(ICascaderResponse))]
    public class ArticleCategoryResponse : ITreeResponse<ArticleCategoryResponse>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 能否被锁定（锁定分组无法被删除）
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// 创建者Id
        /// </summary>
        public long? CreatedUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedTime { get; set; }

        /// <summary>
        /// 修改者Id
        /// </summary>
        public long? UpdatedUserId { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdatedTime { get; set; }

        /// <summary>
        /// Label
        /// </summary>
        public override string Label { get => this.Name; set { } }
    }
}
