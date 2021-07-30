using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.IRepository.Base
{
    public class InitializeDatabase
    {
        private readonly IFreeSql _fsql = null;
        private readonly ILogger<InitializeDatabase> _logger;

        public InitializeDatabase(IFreeSql fsql
            , ILoggerFactory logger)
        {
            _fsql = fsql ?? throw new ArgumentNullException(nameof(IFreeSql));
            _logger = logger == null ? throw new ArgumentNullException(nameof(ILogger<InitializeDatabase>)) : logger.CreateLogger<InitializeDatabase>();
        }

        public Task Begin()
        {

            return Task.CompletedTask;
        }
    }
}
