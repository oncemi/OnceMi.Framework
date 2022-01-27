using FreeSql.DataAnnotations;

namespace OnceMi.Framework.Entity.Article
{
    /// <summary>
    /// 文章分组（中间表）
    /// </summary>
    [Table(Name = "article_belong_category")]
    public class ArticleBelongCategory : IBaseEntity
    {
        public long ArticleId { get; set; }

        public long CategoryId { get; set; }

        [Navigate(nameof(CategoryId))]
        public ArticleCategory ArticleCategory { get; set; }

        [Navigate(nameof(ArticleId))]
        public ArticleInfo Article { get; set; }
    }
}
