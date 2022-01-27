using FreeSql;
using OnceMi.Framework.Entity.Article;

namespace OnceMi.Framework.IRepository
{
    public interface IArticleRepository : IBaseRepository<ArticleInfo, long>, IRepositoryDependency
    {

    }
}
