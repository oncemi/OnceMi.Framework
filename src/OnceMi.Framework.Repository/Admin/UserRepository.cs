using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using System;

namespace OnceMi.Framework.Repository
{
    public class UserRepository : BaseUnitOfWorkRepository<UserInfo, long>, IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        private readonly IFreeSql _db;

        public UserRepository(BaseUnitOfWorkManager uow
            , ILogger<UserRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = base.Orm;
        }
    }
}
