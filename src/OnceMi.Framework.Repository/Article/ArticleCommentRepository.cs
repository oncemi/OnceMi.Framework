using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Article;
using OnceMi.Framework.IRepository;
using System;

namespace OnceMi.Framework.Repository
{
    public class ArticleCommentRepository : BaseUnitOfWorkRepository<ArticleComments, long>, IArticleCommentRepository
    {
        private readonly ILogger<ArticleCommentRepository> _logger;
        private readonly IFreeSql _db;

        public ArticleCommentRepository(BaseUnitOfWorkManager uow
            , ILogger<ArticleCommentRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = base.Orm;
        }
    }
}
