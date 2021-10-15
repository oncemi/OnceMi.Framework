using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnceMi.Framework.Model.Dto
{
    public class CreateOrUpdateArticleRequest : IRequest
    {
        /// <summary>
        /// Id，更新不能为空
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [Required(ErrorMessage = "菜单名称不能为空")]
        [MaxLength(50, ErrorMessage = "文章标题不能超过50个字")]
        public string Title { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string SubTitle { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 摘要
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// 是否草稿
        /// </summary>
        public bool IsDraft { get; set; }

        /// <summary>
        /// 是否启用评论
        /// </summary>
        public bool IsAllowComment { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool IsTop { get; set; }

        /// <summary>
        /// 文章分类
        /// </summary>
        public List<long> Categories { get; set; }

        /// <summary>
        /// 文章标签
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// 封面
        /// </summary>
        public List<string> Covers { get; set; }
    }
}
