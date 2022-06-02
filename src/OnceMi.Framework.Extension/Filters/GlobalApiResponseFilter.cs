using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Model;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Util.Extensions;
using System;
using System.Linq;
using System.Net;

namespace OnceMi.Framework.Extension.Filters
{
    public class GlobalApiResponseFilter : ActionFilterAttribute
    {
        private readonly ILoggerFactory _loggerFactory;
        //忽略的url path
        private readonly string[] _noPackageWhitelists = new string[]
        {
            "/sys/health"
        };

        public GlobalApiResponseFilter(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(ILoggerFactory));
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                return;
            }
            switch (context.Result)
            {
                //case PartialViewResult:
                //case ViewResult:
                //case ViewComponentResult:
                //case AcceptedResult:
                //case AcceptedAtActionResult:
                //case ChallengeResult:
                //case CreatedResult:
                //case CreatedAtActionResult:
                //case FileResult:
                ////case FileStreamResult:
                ////case VirtualFileResult:
                ////case PhysicalFileResult:
                //case IKeepTempDataResult:
                ////case RedirectResult:
                ////case RedirectToActionResult:
                ////case RedirectToPageResult:
                //case ForbidResult:
                //case LocalRedirectResult:
                //case SignInResult:
                //case SignOutResult:
                default:
                    {
                        //不做任何处理
                        base.OnActionExecuted(context);
                    }
                    break;
                case EmptyResult emptyResult:
                    {
                        ContextResultRewrite(context, (int)HttpStatusCode.OK, null);
                    }
                    break;
                case StatusCodeResult statusCodeResult:
                    //case OkResult:
                    //case NotFoundResult:
                    //case NoContentResult:
                    //case ConflictResult:
                    {
                        ContextResultRewrite(context, statusCodeResult.StatusCode, null);
                    }
                    break;
                case JsonResult jsonResult:
                    {
                        ContextResultRewrite(context, jsonResult.StatusCode ?? (int)HttpStatusCode.OK, jsonResult.Value);
                    }
                    break;
                case ContentResult contentResult:
                    {
                        ContextResultRewrite(context, contentResult.StatusCode ?? (int)HttpStatusCode.OK, contentResult.Content);
                    }
                    break;
                case ObjectResult objectResult:
                    //case OkObjectResult:
                    //case BadRequestObjectResult:
                    //case NotFoundObjectResult:
                    {
                        ContextResultRewrite(context, objectResult.StatusCode ?? (int)HttpStatusCode.OK, objectResult.Value);
                    }
                    break;
            }
            base.OnActionExecuted(context);
        }

        private void ContextResultRewrite(ActionExecutedContext context, int statusCode, object value)
        {
            //跳过在白名单中的action
            if (_noPackageWhitelists.Any(p => p.Equals(context.HttpContext.Request.Path, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }
            //跳过包含NoPackageAttribute的Action
            var endpointMetadata = context.ActionDescriptor.EndpointMetadata;
            if (endpointMetadata != null && endpointMetadata.Count > 0)
            {
                if (endpointMetadata.Any(p => p is NoPackageAttribute))
                {
                    return;
                }
            }
            //检查是否已经封装
            string resultObjectName = typeof(ResultObject<object>).FullName?.Split('`')[0];
            string resultValueTypeName = value?.GetType().FullName;
            if (!string.IsNullOrEmpty(resultObjectName)
                && !string.IsNullOrEmpty(resultValueTypeName)
                && resultValueTypeName.StartsWith(resultObjectName))
            {
                return;
            }
            //rewrite statusCode
            if (Enum.IsDefined(typeof(HttpStatusCode), statusCode))
            {
                context.HttpContext.Response.StatusCode = statusCode;
            }
            else
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            //message
            string message = "Success";
            if (statusCode != (int)HttpStatusCode.OK)
            {
                if (value != null && value is string)
                {
                    message = value.ToString();
                    value = null;
                }
                else
                {
                    message = "Bad request";
                    if (Enum.IsDefined(typeof(HttpStatusCode), statusCode))
                    {
                        HttpStatusCode httpCode = (HttpStatusCode)statusCode;
                        message = httpCode.GetDescription() ?? httpCode.ToDescriptionOrString();
                    }
                    if (string.IsNullOrEmpty(message))
                    {
                        message = "Bad request";
                    }
                }
            }
            //reset 200 to 0
            if (statusCode == (int)HttpStatusCode.OK)
                statusCode = 0;
            //pakage result object
            context.Result = new ObjectResult(new ResultObject<object>()
            {
                Code = statusCode,
                Message = message,
                Data = value
            });
        }
    }
}
