using FreeSql;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.Util.Extensions;
using System;

namespace OnceMi.Framework.Repository
{
    public class ApiRepository : BaseUnitOfWorkRepository<Api, long>, IApiRepository
    {
        private readonly ILogger<ApiRepository> _logger;
        private readonly IFreeSql _db;

        public ApiRepository(BaseUnitOfWorkManager uow
            , ILogger<ApiRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = base.Orm;
        }
    }
}
