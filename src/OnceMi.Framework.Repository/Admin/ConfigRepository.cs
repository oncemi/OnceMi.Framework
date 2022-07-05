using Microsoft.Extensions.Logging;
using OnceMi.Framework.IRepository;
using System;

namespace OnceMi.Framework.Repository
{
    public class ConfigRepository : BaseUnitOfWorkRepository<Entity.Admin.Config, long>, IConfigRepository
    {
        private readonly ILogger<ConfigRepository> _logger;
        private readonly IFreeSql _db;

        public ConfigRepository(BaseUnitOfWorkManager uow
            , ILogger<ConfigRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = base.Orm;
        }
    }
}
