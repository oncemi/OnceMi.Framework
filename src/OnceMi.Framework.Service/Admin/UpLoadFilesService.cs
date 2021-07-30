using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class UpLoadFilesService : BaseService<UpLoadFiles, long>, IUpLoadFilesService
    {
        private readonly IUpLoadFilesRepository _repository;
        private readonly ILogger<UpLoadFilesService> _logger;

        public UpLoadFilesService(IUpLoadFilesRepository repository
            , ILogger<UpLoadFilesService> logger) : base(repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(IUpLoadFilesRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<UpLoadFilesService>));
        }

    }
}
