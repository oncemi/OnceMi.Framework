using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using System;

namespace OnceMi.Framework.Repository
{
    public class OrganizeRepository : BaseUnitOfWorkRepository<Organizes, long>, IOrganizeRepository
    {
        private readonly ILogger<OrganizeRepository> _logger;
        private readonly IFreeSql _db;

        public OrganizeRepository(BaseUnitOfWorkManager uow
            , ILogger<OrganizeRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = base.Orm;
        }
    }
}
