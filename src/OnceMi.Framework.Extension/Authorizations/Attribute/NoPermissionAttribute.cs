using Microsoft.AspNetCore.Authorization;

namespace OnceMi.Framework.Extension.Authorizations
{
    /// <summary>
    /// 用于保护API资源，被保护的接口在用户登录后即可访问
    /// </summary>
    public class NoPermissionAttribute : AuthorizeAttribute
    {

    }
}
