using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Article;
using OnceMi.Framework.IRepository;
using System;

namespace OnceMi.Framework.Repository
{
    public class ArticleTagRepository : BaseUnitOfWorkRepository<ArticleTag, long>, IArticleTagRepository
    {
        private readonly ILogger<ArticleTagRepository> _logger;
        private readonly IFreeSql _db;

        public ArticleTagRepository(BaseUnitOfWorkManager uow
            , ILogger<ArticleTagRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = base.Orm;
        }
    }
}
