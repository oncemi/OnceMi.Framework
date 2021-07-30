using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using System;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Authorizations
{
    public class GlobalPermissionRequirement :  IAuthorizationRequirement
    {
        public ActionDescriptor ActionDescriptor { get; set; }
    }
}
