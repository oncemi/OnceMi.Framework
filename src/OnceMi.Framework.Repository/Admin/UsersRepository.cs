using Microsoft.Extensions.Logging;
using OnceMi.Framework.IRepository;
using OnceMi.IdentityServer4.User.Entities;
using System;

namespace OnceMi.Framework.Repository
{
    public class UsersRepository : BaseUnitOfWorkRepository<Users, long>, IUsersRepository
    {
        private readonly ILogger<UsersRepository> _logger;
        private readonly BaseUnitOfWorkManager _uow;
        private readonly IFreeSql _db;

        public UsersRepository(BaseUnitOfWorkManager uow
            , ILogger<UsersRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _db = base.Orm;
        }
    }
}
