using OnceMi.Framework.Entity.Article;
using OnceMi.Framework.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    [MapperFrom(typeof(ArticleTag))]
    public class ArticleTagResponse
    {
        public long Id { get; set; }

        /// <summary>
        /// 文章Id
        /// </summary>
        public long ArticleId { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; set; }
    }
}
