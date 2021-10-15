using OnceMi.Framework.Entity.Article;
using OnceMi.Framework.Model.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Article
{
    public interface IArticleCategoryService : IBaseService<ArticleCategories, long>
    {
        Task<IPageResponse<ArticleCategoryResponse>> Query(IPageRequest request, bool onlyQueryEnabled = false);

        Task<ArticleCategoryResponse> Query(long id);

        Task<ArticleCategoryResponse> Insert(CreateArticleCategoryRequest request);

        Task Update(UpdateArticleCategoryRequest request);

        Task Delete(List<long> ids);
    }
}
