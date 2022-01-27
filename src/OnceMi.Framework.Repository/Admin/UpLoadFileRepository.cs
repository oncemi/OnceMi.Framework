using FreeSql;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.Util.Extensions;
using System;

namespace OnceMi.Framework.Repository
{
    public class UpLoadFileRepository : BaseUnitOfWorkRepository<UpLoadFile, long>, IUpLoadFileRepository
    {
        private readonly ILogger<UpLoadFileRepository> _logger;
        private readonly IFreeSql _db;

        public UpLoadFileRepository(BaseUnitOfWorkManager uow
            , ILogger<UpLoadFileRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = base.Orm;
        }
    }
}
