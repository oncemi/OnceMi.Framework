using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using System;

namespace OnceMi.Framework.Repository
{
    public class JobRepository : BaseUnitOfWorkRepository<Job, long>, IJobRepository
    {
        private readonly ILogger<JobRepository> _logger;
        private readonly IFreeSql _db;

        public JobRepository(BaseUnitOfWorkManager uow
            , ILogger<JobRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = base.Orm;
        }
    }
}
