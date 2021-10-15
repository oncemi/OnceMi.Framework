using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Entity.Article
{
    /// <summary>
    /// 文章评论
    /// </summary>
    [Table(Name = nameof(ArticleComments))]
    public class ArticleComments : IBaseEntity
    {
        /// <summary>
        /// 父Id
        /// </summary>
        [Column(IsNullable = true)]
        public long? ParentId { get; set; }

        /// <summary>
        /// 文章Id
        /// </summary>
        public long ArticleId { get; set; }

        /// <summary>
        /// 评论内容
        /// </summary>
        [Column(StringLength = -1, IsNullable = true)]
        public string Content { get; set; }

        /// <summary>
        /// 点赞数量
        /// </summary>
        public long Like { get; set; } = 0;

        /// <summary>
        /// 是否审核
        /// </summary>
        public bool IsReview { get; set; } = false;

        /// <summary>
        /// 审核人Id
        /// </summary>
        [Column(IsNullable = true)]
        public long? ReviewerId { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        [Column(IsNullable = true)]
        public DateTime? ReviewerTime { get; set; }

        //[Navigate(nameof(ArticleId))]
        //public Articles Article { get; set; }

        /// <summary>
        /// 子条目
        /// </summary>
        [Column(IsIgnore = true)]
        public List<ArticleComments> Children { get; set; }
    }
}
