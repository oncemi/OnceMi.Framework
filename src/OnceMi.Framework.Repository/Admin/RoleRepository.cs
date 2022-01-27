using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using System;

namespace OnceMi.Framework.Repository
{
    public class RoleRepository : BaseUnitOfWorkRepository<Role, long>, IRoleRepository
    {
        private readonly ILogger<RoleRepository> _logger;
        private readonly IFreeSql _db;

        public RoleRepository(BaseUnitOfWorkManager uow
            , ILogger<RoleRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = base.Orm;
        }
    }
}
