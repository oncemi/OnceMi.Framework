using OnceMi.Framework.Entity.Article;
using OnceMi.Framework.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class ArticleResponse : IResponse
    {
        public long Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// 文章内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 文章摘要
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// 作者名称
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// 浏览次数
        /// </summary>
        public int ViewNum { get; set; } = 1;

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
        public int CommentNum { get; set; }

        /// <summary>
        /// 文章分类
        /// </summary>
        public List<ArticleCategoryResponse> Categories { get; set; }

        /// <summary>
        /// 评论
        /// </summary>
        public List<ArticleComments> Comments { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public List<ArticleTags> Tags { get; set; }

        /// <summary>
        /// 封面
        /// </summary>
        public List<ArticleCovers> Covers { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// 创建者Id
        /// </summary>
        public long CreatedUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedTime { get; set; }

        /// <summary>
        /// 修改者Id
        /// </summary>
        public long? UpdatedUserId { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdatedTime { get; set; }
    }
}
