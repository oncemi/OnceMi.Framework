using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using System;

namespace OnceMi.Framework.Repository
{
    public class JobHistoriyRepository : BaseUnitOfWorkRepository<JobHistory, long>, IJobHistoryRepository
    {
        private readonly ILogger<JobHistoriyRepository> _logger;
        private readonly IFreeSql _db;

        public JobHistoriyRepository(BaseUnitOfWorkManager uow
            , ILogger<JobHistoriyRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = base.Orm;
        }
    }
}
