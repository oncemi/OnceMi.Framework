using OnceMi.Framework.Entity.Article;
using OnceMi.Framework.Model.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Article
{
    public interface IArticleTagService : IBaseService<ArticleTags, long>
    {
        /// <summary>
        /// 查询所有文章Tag
        /// </summary>
        /// <returns></returns>
        Task<List<ArticleTagResponse>> QueryAllTags();
    }
}
