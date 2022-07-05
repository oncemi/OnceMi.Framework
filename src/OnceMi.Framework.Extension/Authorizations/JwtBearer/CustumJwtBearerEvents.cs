using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using OnceMi.Framework.Config;
using OnceMi.Framework.Model;
using OnceMi.Framework.Util.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Authorizations
{
    public static class CustumJwtBearerEvents
    {
        public static Task OnTokenValidated(TokenValidatedContext context)
        {
            if (context.Request.Path.Value.Contains("api/v1/file"))
            {
                context.Success();
            }

            return Task.CompletedTask;
        }

        public static async Task OnChallenge(JwtBearerChallengeContext context)
        {
            if (!string.IsNullOrEmpty(context.ErrorDescription))
                await WriteResponse(context.Response, StatusCodes.Status401Unauthorized, context.ErrorDescription);
            else
                await WriteResponse(context.Response, StatusCodes.Status401Unauthorized, "Unauthorized");
            context.HandleResponse();
        }

        public static async Task OnForbidden(ForbiddenContext context)
        {
            await WriteResponse(context.Response, StatusCodes.Status403Forbidden, "Forbidden");
        }

        public static Task OnAuthenticationFailed(AuthenticationFailedContext context)
        {
            // 如果过期，则把<是否过期>添加到，返回头信息中
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }

        private static async Task WriteResponse(HttpResponse response, int statusCode, string message)
        {
            response.ContentType = "application/json";
            response.StatusCode = statusCode;

            await response.WriteAsync(JsonUtil.SerializeToString(new ResultObject<object>()
            {
                Code = statusCode,
                Message = message,
            }, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = GlobalConfigConstant.DefaultJsonNamingPolicy
            }));
        }
    }
}
