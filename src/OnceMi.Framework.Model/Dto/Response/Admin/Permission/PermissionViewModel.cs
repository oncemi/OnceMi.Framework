using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class PermissionViewModel
    {
        public List<RoleItemResponse> Roles { get; set; } = new List<RoleItemResponse>();

        public List<MenuItemResponse> Menus { get; set; } = new List<MenuItemResponse>();
    }
}
