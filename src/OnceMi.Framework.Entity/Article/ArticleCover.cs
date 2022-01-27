using FreeSql.DataAnnotations;

namespace OnceMi.Framework.Entity.Article
{
    /// <summary>
    /// 文章封面
    /// </summary>
    [Table(Name = "article_cover")]
    public class ArticleCover : IBaseEntity
    {
        /// <summary>
        /// 文章Id
        /// </summary>
        public long ArticleId { get; set; }

        /// <summary>
        /// 链接
        /// </summary>
        [Column(StringLength = -1, IsNullable = false)]
        public string Url { get; set; }

        //[Navigate(nameof(ArticleId))]
        //public Articles Article { get; set; }
    }
}
