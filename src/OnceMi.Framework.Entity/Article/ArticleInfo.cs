using FreeSql.DataAnnotations;
using OnceMi.Framework.Entity.Admin;
using System.Collections.Generic;

namespace OnceMi.Framework.Entity.Article
{
    /// <summary>
    /// 文章
    /// </summary>
    [Table(Name = "article_info")]
    public class ArticleInfo : IBaseEntity
    {
        /// <summary>
        /// 标题
        /// </summary>
        [Column(StringLength = 500, IsNullable = false)]
        public string Title { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        [Column(StringLength = 500, IsNullable = true)]
        public string SubTitle { get; set; }

        /// <summary>
        /// 摘要
        /// </summary>
        [Column(StringLength = -1, IsNullable = true)]
        public string Summary { get; set; }

        /// <summary>
        /// 文章内容
        /// </summary>
        [Column(StringLength = -1, IsNullable = true)]
        public string Content { get; set; }

        /// <summary>
        /// 浏览次数
        /// </summary>
        public int ViewNum { get; set; } = 0;

        /// <summary>
        /// 是否允许评论
        /// </summary>
        public bool IsAllowComment { get; set; }

        /// <summary>
        /// 是否为草稿
        /// </summary>
        public bool IsDraw { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool IsTop { get; set; } = false;

        /// <summary>
        /// 评论数量
        /// </summary>
        public int CommentNum { get; set; } = 0;

        /// <summary>
        /// 文章分类
        /// </summary>
        [Navigate(ManyToMany = typeof(ArticleBelongCategory))]
        public virtual List<ArticleCategory> ArticleCategories { get; set; }

        /// <summary>
        /// 评论
        /// </summary>
        [Navigate(nameof(Entity.Article.ArticleComment.ArticleId))]
        public virtual List<ArticleComment> ArticleComments { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        [Navigate(nameof(Entity.Article.ArticleTag.ArticleId))]
        public virtual List<ArticleTag> ArticleTags { get; set; }

        /// <summary>
        /// 封面
        /// </summary>
        [Navigate(nameof(Entity.Article.ArticleCover.ArticleId))]
        public virtual List<ArticleCover> ArticleCovers { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Navigate(nameof(CreatedUserId))]
        public virtual UserInfo CreateUser { get; set; }
    }
}
