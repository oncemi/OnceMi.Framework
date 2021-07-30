using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using System;

namespace OnceMi.Framework.Repository
{
    public class JobHistoriesRepository : BaseUnitOfWorkRepository<JobHistories, long>, IJobHistoriesRepository
    {
        private readonly ILogger<JobHistoriesRepository> _logger;
        private readonly BaseUnitOfWorkManager _uow;
        private readonly IFreeSql _db;

        public JobHistoriesRepository(BaseUnitOfWorkManager uow
            , ILogger<JobHistoriesRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _db = base.Orm;
        }


    }
}
