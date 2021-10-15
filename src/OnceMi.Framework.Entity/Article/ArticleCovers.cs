using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Entity.Article
{
    /// <summary>
    /// 文章封面
    /// </summary>
    [Table(Name = nameof(ArticleCovers))]
    public class ArticleCovers : IBaseEntity
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
