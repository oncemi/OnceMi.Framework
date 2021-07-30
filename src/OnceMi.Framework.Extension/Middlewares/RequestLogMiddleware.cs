using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Middlewares
{
    public class RequestLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly ConfigManager _config;

        public RequestLogMiddleware(RequestDelegate next
            , ILogger<RequestLogMiddleware> logger
            , ConfigManager config)
        {
            _next = next;
            _logger = logger;
            _config = config;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestLogModel = new RequestLogModel
            {
                Url = context.Request.Path.ToString(),
                Headers = context.Request.Headers,
                Method = context.Request.Method
            };
            //获取context.Request.Body内容
            string[] requestBodyMethod = new string[] { "post", "delete", "put" };
            if (requestBodyMethod.Contains(context.Request.Method.ToLower()))
            {
                context.Request.EnableBuffering(); //启用倒带功能，就可以让 Request.Body 可以再次读取
                if(context.Request.Body.Length > 0)
                {
                    byte[] buffer = new byte[context.Request.ContentLength.Value];
                    context.Request.Body.Read(buffer, 0, buffer.Length);
                    context.Request.Body.Position = 0;
                    requestLogModel.RequestBody = Encoding.UTF8.GetString(buffer);
                }
            }
            else if (context.Request.Method.ToLower().Equals("get"))
            {
                requestLogModel.RequestBody = context.Request.QueryString.Value;
            }
            Stopwatch watch = Stopwatch.StartNew();
            //获取Response.Body内容
            var originalBodyStream = context.Response.Body;
            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;
                await _next(context);

                watch.Stop();

                requestLogModel.ResponseBody = await FormatResponse(context.Response);
                requestLogModel.Elapsed = watch.ElapsedMilliseconds;
                requestLogModel.StatusCode = context.Response.StatusCode;

                await responseBody.CopyToAsync(originalBodyStream);
            }
            _logger.LogInformation($"RequestLog: {requestLogModel}");
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return text;
        }
    }

    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            ConfigManager config = builder.ApplicationServices.GetRequiredService<ConfigManager>();
            if (config == null)
                throw new Exception("Can not get ConfigManager from service collection.");
            if (config.AppSettings.IsEnableRequestLog)
            {
                builder.UseMiddleware<RequestLogMiddleware>();
            }
            return builder;
        }
    }
}
