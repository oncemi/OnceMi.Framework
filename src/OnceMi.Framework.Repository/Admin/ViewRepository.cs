using FreeSql;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.Util.Extensions;
using System;

namespace OnceMi.Framework.Repository
{
    public class ViewRepository : BaseUnitOfWorkRepository<Views, long>, IViewRepository
    {
        private readonly ILogger<ViewRepository> _logger;
        private readonly IFreeSql _db;

        public ViewRepository(BaseUnitOfWorkManager uow
            , ILogger<ViewRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = base.Orm;
        }
    }
}
