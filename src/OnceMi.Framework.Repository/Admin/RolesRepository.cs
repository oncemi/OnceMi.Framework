using FreeSql;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.Util.Extensions;
using OnceMi.IdentityServer4.User.Entities;
using System;

namespace OnceMi.Framework.Repository
{
    public class RolesRepository : BaseUnitOfWorkRepository<Roles, long>, IRolesRepository
    {
        private readonly ILogger<RolesRepository> _logger;
        private readonly BaseUnitOfWorkManager _uow;
        private readonly IFreeSql _db;

        public RolesRepository(BaseUnitOfWorkManager uow
            , ILogger<RolesRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _db = base.Orm;
        }

        
    }
}
