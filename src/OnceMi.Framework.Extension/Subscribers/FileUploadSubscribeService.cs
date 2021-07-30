using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.MQ;
using OnceMi.Framework.Model.Dto;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Subscribers
{
    public class FileUploadSubscribeService : IQueneSubscribe<UploadFileRequest>
    {
        private readonly ILogger<FileUploadSubscribeService> _logger;

        public FileUploadSubscribeService(ILogger<FileUploadSubscribeService> logger
            , IMessageQueneService bus) : base(bus, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<FileUploadSubscribeService>));
        }

        public override Task Subscribe(UploadFileRequest model, CancellationToken cancellationToken = default)
        {
            //throw new NotImplementedException();

            _logger.LogInformation($"消息队列收到了消息。文件名为：{model.FileName}，SubId：{SubId}，提供器：{MqService.Options.ProviderType}");

            Thread.Sleep(5000);

            _logger.LogInformation($"消费完成，线程ID：{Thread.GetCurrentProcessorId()}");

            return Task.CompletedTask;
        }
    }
}
