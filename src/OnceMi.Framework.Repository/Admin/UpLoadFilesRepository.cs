using FreeSql;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.Util.Extensions;
using System;

namespace OnceMi.Framework.Repository
{
    public class UpLoadFilesRepository : BaseUnitOfWorkRepository<UpLoadFiles, long>, IUpLoadFilesRepository
    {
        private readonly ILogger<UpLoadFilesRepository> _logger;
        private readonly BaseUnitOfWorkManager _uow;
        private readonly IFreeSql _db;

        public UpLoadFilesRepository(BaseUnitOfWorkManager uow
            , ILogger<UpLoadFilesRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _db = base.Orm;
        }


    }
}
