using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Model;
using OnceMi.Framework.Util.Json;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Helpers
{
    /// <summary>
    /// 一些重写规则
    /// </summary>
    public static class RewriteHelper
    {
        public static IActionResult RewriteInvalidModelStateResponse(ActionContext context)
        {
            //获取验证失败的模型字段 
            var errorList = context.ModelState.Select(e
                => $"{e.Key}{(string.IsNullOrEmpty(e.Key) ? "" : ":")}{e.Value.Errors?.FirstOrDefault()?.ErrorMessage}")
            .ToList();
            //设置返回内容
            var result = new ResultObject<object>()
            {
                Code = (int)HttpStatusCode.BadRequest,
                Message = string.Join('|', errorList),
                Data = context.ModelState,
            };
            var objectResult = new BadRequestObjectResult(result)
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
            };
            return objectResult;
        }

        public static async Task GlobalExceptionHandler(HttpContext context, ILoggerFactory loggerFactory, JsonNamingPolicy jsonNamingPolicy)
        {
            var ex = context.Features.Get<IExceptionHandlerFeature>();
            if (ex != null)
            {
                //write log
                ILogger logger = loggerFactory.CreateLogger($"[{context.Request.Method}]{context.Request.Path}");
                logger.LogError(ex.Error, ex.Error.Message);
                //set status code
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                //serialize
                var errObj = JsonUtil.SerializeToString(new ResultObject<object>(context.Response.StatusCode, ex.Error.Message), new JsonSerializerOptions()
                {
                    //小驼峰
                    PropertyNamingPolicy = jsonNamingPolicy
                });
                await context.Response.WriteAsync(errObj).ConfigureAwait(false);
            }
        }
    }
}
