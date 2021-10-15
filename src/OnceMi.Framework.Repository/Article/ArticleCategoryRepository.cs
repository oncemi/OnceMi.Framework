using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Article;
using OnceMi.Framework.IRepository;
using System;

namespace OnceMi.Framework.Repository
{
    public class ArticleCategoryRepository : BaseUnitOfWorkRepository<ArticleCategories, long>, IArticleCategoryRepository
    {
        private readonly ILogger<ArticleCategoryRepository> _logger;
        private readonly IFreeSql _db;

        public ArticleCategoryRepository(BaseUnitOfWorkManager uow
            , ILogger<ArticleCategoryRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = base.Orm;
        }
    }
}
