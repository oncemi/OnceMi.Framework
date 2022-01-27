using FreeSql;
using OnceMi.Framework.Entity.Article;

namespace OnceMi.Framework.IRepository
{
    public interface IArticleCategoryRepository : IBaseRepository<ArticleCategory, long>, IRepositoryDependency
    {

    }
}
