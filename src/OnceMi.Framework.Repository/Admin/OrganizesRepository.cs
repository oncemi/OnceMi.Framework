using FreeSql;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.Util.Extensions;
using OnceMi.IdentityServer4.User.Entities;
using System;

namespace OnceMi.Framework.Repository
{
    public class OrganizesRepository : BaseUnitOfWorkRepository<Organizes, long>, IOrganizesRepository
    {
        private readonly ILogger<OrganizesRepository> _logger;
        private readonly BaseUnitOfWorkManager _uow;
        private readonly IFreeSql _db;

        public OrganizesRepository(BaseUnitOfWorkManager uow
            , ILogger<OrganizesRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _db = base.Orm;
        }


    }
}
