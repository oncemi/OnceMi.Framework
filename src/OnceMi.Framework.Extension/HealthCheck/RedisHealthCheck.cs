using FreeRedis;
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
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly ILogger<RedisHealthCheck> _logger;
        private readonly RedisClient _redisClient;

        public RedisHealthCheck(ILogger<RedisHealthCheck> logger
            , RedisClient redisClient)
        {
            _logger = _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<RedisHealthCheck>));
            _redisClient = redisClient ?? throw new ArgumentNullException(nameof(RedisClient));
        }

        public string Name => this.GetType().Name;

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                string key = Guid.NewGuid().ToString();
                _redisClient.Set(key, $"RedisHealthCheck|{DateTime.Now}", TimeSpan.FromSeconds(3));
                long count = _redisClient.Del(key);
                if (count == 1)
                {
                    return await Task.FromResult(new HealthCheckResult(HealthStatus.Healthy, "Success"));
                }
                else
                {
                    return await Task.FromResult(new HealthCheckResult(HealthStatus.Degraded, "Too long execution time."));
                }
            }
            catch (Exception ex)
            {
                return await Task.FromResult(new HealthCheckResult(HealthStatus.Unhealthy, ex.Message));
            }
        }
    }
}
