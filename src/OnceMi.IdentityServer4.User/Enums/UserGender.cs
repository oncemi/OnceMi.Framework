
using System.ComponentModel;

namespace OnceMi.IdentityServer4.User
{
    /// <summary>
    /// 用户性别
    /// </summary>
    public enum UserGender
    {
        [Description("未知")]
        Unknow = 0,

        [Description("男")]
        Male = 1,

        [Description("女")]
        Female = 2,

        [Description("其他")]
        All = 4,
    }
}
