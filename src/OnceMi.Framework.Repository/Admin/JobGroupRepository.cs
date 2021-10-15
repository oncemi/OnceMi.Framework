using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using System;

namespace OnceMi.Framework.Repository
{
    public class JobGroupRepository : BaseUnitOfWorkRepository<JobGroups, long>, IJobGroupRepository
    {
        private readonly ILogger<JobGroupRepository> _logger;
        private readonly IFreeSql _db;

        public JobGroupRepository(BaseUnitOfWorkManager uow
            , ILogger<JobGroupRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = base.Orm;
        }
    }
}
