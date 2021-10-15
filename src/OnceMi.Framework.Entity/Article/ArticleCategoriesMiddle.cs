using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Entity.Article
{
    /// <summary>
    /// 文章分组（中间表）
    /// </summary>
    [Table(Name = nameof(ArticleCategoriesMiddle))]
    public class ArticleCategoriesMiddle : IBaseEntity
    {
        public long ArticleId { get; set; }

        public long CategoryId { get; set; }

        [Navigate(nameof(CategoryId))]
        public ArticleCategories ArticleCategory { get; set; }

        [Navigate(nameof(ArticleId))]
        public Articles Article { get; set; }
    }
}
