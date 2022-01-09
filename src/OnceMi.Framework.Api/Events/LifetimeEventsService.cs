using NLog;
using OnceMi.Framework.Extension.Job;

namespace OnceMi.Framework.Api.Middlewares
{
    public class LifetimeEventsService : IHostedService
    {
        private readonly ILogger<LifetimeEventsService> _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IServiceProvider _serviceProvider;

        public LifetimeEventsService(ILogger<LifetimeEventsService> logger
            , IHostApplicationLifetime appLifetime
            , IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(async () =>
            {
                await OnStarted();
            });
            _appLifetime.ApplicationStopping.Register(async () =>
            {
                await OnStopping();
            });
            _appLifetime.ApplicationStopped.Register(async () =>
            {
                await OnStopped();
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task OnStarted()
        {
            //加载jobs
            using (var scope = _serviceProvider.CreateScope())
            {
                IJobSchedulerService jobsService = scope.ServiceProvider.GetRequiredService<IJobSchedulerService>();
                await jobsService.Init();
            }
        }

        private Task OnStopping()
        {
            return Task.CompletedTask;
        }

        private Task OnStopped()
        {
            _logger.LogInformation("The log manager is shutting down...");
            //nlog 保证程序关闭后日志正常写入
            LogManager.Shutdown();

            return Task.CompletedTask;
        }
    }
}
