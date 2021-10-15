using FreeSql;
using OnceMi.Framework.Entity.Article;

namespace OnceMi.Framework.IRepository
{
    public interface IArticleCommentRepository : IBaseRepository<ArticleComments, long>, IRepositoryDependency
    {

    }
}
