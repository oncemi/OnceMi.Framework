using FreeSql;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.Util.Extensions;
using System;

namespace OnceMi.Framework.Repository
{
    public class PermissionRepository : BaseUnitOfWorkRepository<RolePermissions, long>, IPermissionRepository
    {
        private readonly ILogger<PermissionRepository> _logger;
        private readonly BaseUnitOfWorkManager _uow;
        private readonly IFreeSql _db;

        public PermissionRepository(BaseUnitOfWorkManager uow
            , ILogger<PermissionRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _db = base.Orm;
        }


    }
}
