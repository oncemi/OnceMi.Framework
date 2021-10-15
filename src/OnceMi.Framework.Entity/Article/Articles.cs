using FreeSql.DataAnnotations;
using OnceMi.Framework.Entity.Admin;
using System.Collections.Generic;

namespace OnceMi.Framework.Entity.Article
{
    [Table(Name = nameof(Articles))]
    public class Articles : IBaseEntity
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
        [Navigate(ManyToMany = typeof(ArticleCategoriesMiddle))]
        public virtual List<ArticleCategories> ArticleCategories { get; set; }

        /// <summary>
        /// 评论
        /// </summary>
        [Navigate(nameof(Article.ArticleComments.ArticleId))]
        public virtual List<ArticleComments> ArticleComments { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        [Navigate(nameof(Article.ArticleTags.ArticleId))]
        public virtual List<ArticleTags> ArticleTags { get; set; }

        /// <summary>
        /// 封面
        /// </summary>
        [Navigate(nameof(Article.ArticleCovers.ArticleId))]
        public virtual List<ArticleCovers> ArticleCovers { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Navigate(nameof(CreatedUserId))]
        public virtual Users CreateUser { get; set; }
    }
}
