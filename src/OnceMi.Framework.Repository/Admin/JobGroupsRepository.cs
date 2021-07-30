using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using System;

namespace OnceMi.Framework.Repository
{
    public class JobGroupsRepository : BaseUnitOfWorkRepository<JobGroups, long>, IJobGroupsRepository
    {
        private readonly ILogger<JobGroupsRepository> _logger;
        private readonly BaseUnitOfWorkManager _uow;
        private readonly IFreeSql _db;

        public JobGroupsRepository(BaseUnitOfWorkManager uow
            , ILogger<JobGroupsRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _db = base.Orm;
        }


    }
}
