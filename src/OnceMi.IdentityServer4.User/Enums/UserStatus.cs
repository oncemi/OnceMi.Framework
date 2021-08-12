
using System.ComponentModel;

namespace OnceMi.IdentityServer4.User
{
    public enum UserStatus
    {
        [Description("启用")]
        Enable = 1,

        [Description("禁用")]
        Disable = 2,
    }
}
