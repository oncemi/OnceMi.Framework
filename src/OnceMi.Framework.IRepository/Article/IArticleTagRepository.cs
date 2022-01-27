using FreeSql;
using OnceMi.Framework.Entity.Article;

namespace OnceMi.Framework.IRepository
{
    public interface IArticleTagRepository : IBaseRepository<ArticleTag, long>, IRepositoryDependency
    {

    }
}
