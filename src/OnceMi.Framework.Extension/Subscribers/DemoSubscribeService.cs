using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.MQ;
using OnceMi.Framework.Model.Dto;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Subscribers
{
    public class DemoSubscribeService : IQueneSubscribe<SubDemoModel>
    {
        private readonly ILogger<DemoSubscribeService> _logger;

        public DemoSubscribeService(ILogger<DemoSubscribeService> logger
            , IMessageQueneService bus
            , IServiceProvider serviceProvider) : base(bus, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<DemoSubscribeService>));
        }

        public override Task Subscribe(SubDemoModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"收到了消息，发送时间：{model.Time.ToString("yyyy-MM-dd HH:mm:ss.fff")}，延时：{model.Span}，应该接收时间：{model.Time.AddSeconds(model.Span).ToString("yyyy-MM-dd HH:mm:ss.fff")}，内容：{model.Title}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sub demo failed, {ex.Message}", ex);
            }
            return Task.CompletedTask;
        }
    }
}
