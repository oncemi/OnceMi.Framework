using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Model;
using OnceMi.Framework.Model.Exception;
using System;
using System.Net;

namespace OnceMi.Framework.Extension.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILoggerFactory _loggerFactory;

        public GlobalExceptionFilter(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(ILoggerFactory));
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception == null || context.ExceptionHandled)
                return;
            //create log
            ILogger logger = _loggerFactory.CreateLogger($"[{context.HttpContext.Request.Method}] {context.HttpContext.Request.Path}");
            //process
            switch (context.Exception)
            {
                case BusException ex:
                    {
                        context.Result = new ObjectResult(new ResultObject<object>((int)ex.Code, ex.Message));
                        logger.LogWarning(ex, ex.Message);
                    }
                    break;
                default:
                    {
                        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        context.Result = new ObjectResult(new ResultObject<object>(context.HttpContext.Response.StatusCode, context.Exception.Message));
                        logger.LogError(context.Exception, context.Exception.Message);
                    }
                    break;
            }
            context.ExceptionHandled = true;
        }
    }
}
