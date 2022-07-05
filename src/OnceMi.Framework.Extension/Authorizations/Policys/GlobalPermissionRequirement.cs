using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace OnceMi.Framework.Extension.Authorizations
{
    public class GlobalPermissionRequirement : IAuthorizationRequirement
    {
        public ActionDescriptor ActionDescriptor { get; set; }
    }
}
