using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using System;

namespace OnceMi.Framework.Repository
{
    public class MenuRepository : BaseUnitOfWorkRepository<Menu, long>, IMenuRepository
    {
        private readonly ILogger<MenuRepository> _logger;
        private readonly IFreeSql _db;

        public MenuRepository(BaseUnitOfWorkManager uow
            , ILogger<MenuRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = base.Orm;
        }
    }
}
