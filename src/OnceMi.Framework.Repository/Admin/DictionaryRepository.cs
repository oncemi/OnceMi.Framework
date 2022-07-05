using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using System;

namespace OnceMi.Framework.Repository
{
    public class DictionaryRepository : BaseUnitOfWorkRepository<Dictionary, long>, IDictionaryRepository
    {
        private readonly ILogger<DictionaryRepository> _logger;
        private readonly IFreeSql _db;

        public DictionaryRepository(BaseUnitOfWorkManager uow
            , ILogger<DictionaryRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = base.Orm;
        }
    }
}
