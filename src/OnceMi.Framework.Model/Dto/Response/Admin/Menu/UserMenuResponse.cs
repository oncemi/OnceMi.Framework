using OnceMi.Framework.Model.Dto.Response.Admin.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class UserMenuResponse : IResponse
    {
        /// <summary>
        /// 路由名称
        /// </summary>
        public string Router { get; set; }

        /// <summary>
        /// 菜单路径
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Path { get; set; } = null;

        /// <summary>
        /// 名称
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Name { get; set; } = null;

        /// <summary>
        /// 图标
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Icon { get; set; } = null;

        /// <summary>
        /// 是否隐藏菜单项，true 隐藏，false 不隐藏，会注入到路由元数据meta中。
        /// </summary>
        public bool Invisible { get; set; } = false;

        /// <summary>
        /// 链接
        /// </summary>
        public string Link { get; set; } = null;

        /// <summary>
        /// 参数
        /// </summary>
        public object Query { get; set; } = null;

        /// <summary>
        /// 菜单权限
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MenuRoleAuthority Authority { get; set; } = null;

        /// <summary>
        /// 页面信息
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MenuPageMetaResponse Page { get; set; } = null;

        /// <summary>
        /// 子菜单
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<UserMenuResponse> Children { get; set; } = null;
    }

    public class MenuPageMetaResponse
    {
        /// <summary>
        /// 页面标题
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Title { get; set; } = null;

        /// <summary>
        /// 页面面包屑
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> Breadcrumb { get; set; } = null;
    }
}
