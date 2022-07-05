using System.Collections.Generic;

namespace OnceMi.Framework.Model.Dto
{
    public class PermissionViewModel
    {
        public List<RoleItemResponse> Roles { get; set; } = new List<RoleItemResponse>();

        public List<MenuItemResponse> Menus { get; set; } = new List<MenuItemResponse>();
    }
}
