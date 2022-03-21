using Microsoft.AspNetCore.Mvc;
using OnceMi.Framework.Model;
using System.Net;

namespace OnceMi.Framework.Extension.Helpers
{
    internal static class FilterHelper
    {
        public static IActionResult BuildResult(HttpStatusCode code, string message = "")
        {
            return new JsonResult(new ResultObject<object>()
            {
                Code = (int)code,
                Message = string.IsNullOrEmpty(message) ? code.ToString() : message,
                Data = null
            })
            {
                StatusCode = (int)code,
            };
        }
    }
}
