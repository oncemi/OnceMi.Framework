using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Entity.Article;
using System.Collections.Generic;

namespace OnceMi.Framework.Model.Dto
{
    public class DatabaseEntities
    {
        public List<Api> Apis { get; set; }

        public List<Job> Jobs { get; set; }

        public List<JobGroup> JobGroups { get; set; }

        public List<Menu> Menus { get; set; }

        public List<View> Views { get; set; }

        public List<UserInfo> Users { get; set; }

        public List<Organize> Organizes { get; set; }

        public List<UserOrganize> UserOrganize { get; set; }

        public List<Role> Roles { get; set; }

        public List<UserRole> UserRole { get; set; }

        public List<RolePermission> RolePermissions { get; set; }

        public List<ArticleCategory> ArticleCategories { get; set; }
    }
}
