using FreeSql;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.Util.Extensions;
using System;

namespace OnceMi.Framework.Repository
{
    public class ConfigRepository : BaseUnitOfWorkRepository<Configs, long>, IConfigRepository
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
