using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Entity.Article
{
    /// <summary>
    /// 文章标签
    /// </summary>
    [Table(Name = "article_tags")]
    public class ArticleTag : IBaseEntity
    {
        /// <summary>
        /// 文章Id
        /// </summary>
        public long ArticleId { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        [Column(StringLength = 100, IsNullable = false)]
        public string Tag { get; set; }

        //[Navigate(nameof(ArticleId))]
        //public Articles Article { get; set; }
    }
}
