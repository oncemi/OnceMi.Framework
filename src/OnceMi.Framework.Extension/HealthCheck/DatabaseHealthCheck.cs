using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.HealthCheck
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly ILogger<DatabaseHealthCheck> _logger;
        private readonly IdleBus<IFreeSql> _ib;

        public DatabaseHealthCheck(ILogger<DatabaseHealthCheck> logger
            , IdleBus<IFreeSql> ib)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<DatabaseHealthCheck>));
            _ib = ib ?? throw new ArgumentNullException(nameof(IdleBus<IFreeSql>));
        }

        public string Name => this.GetType().Name;

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var dbs = _ib.GetAll();
                if (dbs == null || dbs.Count == 0)
                {
                    throw new Exception("Can not get db from IdleBus");
                }
                List<string> failedDb = new List<string>();
                foreach (var item in dbs)
                {
                    if (!item.Ado.ExecuteConnectTest())
                    {
                        failedDb.Add(item.Ado.ConnectionString);
                    }
                }
                if (failedDb.Count == dbs.Count)
                {
                    throw new Exception("All databases are down.");
                }
                if (failedDb.Count == 0)
                {
                    return await Task.FromResult(new HealthCheckResult(HealthStatus.Healthy, "Success"));
                }
                else
                {
                    string failedDbStr = string.Join('|', failedDb);
                    return await Task.FromResult(new HealthCheckResult(
                        status: HealthStatus.Degraded
                        , description: $"There are problems with these database connections.({failedDbStr})"));
                }
            }
            catch (Exception ex)
            {
                return await Task.FromResult(new HealthCheckResult(HealthStatus.Unhealthy, ex.Message));
            }
        }
    }
}
