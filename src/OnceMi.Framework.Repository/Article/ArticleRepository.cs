using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Article;
using OnceMi.Framework.IRepository;
using System;

namespace OnceMi.Framework.Repository
{
    public class ArticleRepository : BaseUnitOfWorkRepository<Articles, long>, IArticleRepository
    {
        private readonly ILogger<ArticleRepository> _logger;
        private readonly IFreeSql _db;

        public ArticleRepository(BaseUnitOfWorkManager uow
            , ILogger<ArticleRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = base.Orm;
        }
    }
}
