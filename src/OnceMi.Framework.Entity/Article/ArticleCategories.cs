using FreeSql.DataAnnotations;
using System.Collections.Generic;

namespace OnceMi.Framework.Entity.Article
{
    /// <summary>
    /// 文章分类
    /// </summary>
    [Table(Name = nameof(ArticleCategories))]
    public class ArticleCategories : IBaseEntity
    {
        [Column(IsNullable = true)]
        public long? ParentId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Column(StringLength = 50, IsNullable = false)]
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [Column(StringLength = -1, IsNullable = true)]
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
        /// 子条目
        /// </summary>
        [Column(IsIgnore = true)]
        public List<ArticleCategories> Children { get; set; }

        [Navigate(ManyToMany = typeof(ArticleCategoriesMiddle))]
        public virtual List<Articles> Users { get; set; }
    }
}
