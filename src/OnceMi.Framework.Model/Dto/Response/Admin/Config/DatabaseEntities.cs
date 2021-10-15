using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Entity.Article;
using System.Collections.Generic;

namespace OnceMi.Framework.Model.Dto
{
    public class DatabaseEntities
    {
        public List<Apis> Apis { get; set; }

        public List<Jobs> Jobs { get; set; }

        public List<JobGroups> JobGroups { get; set; }

        public List<Menus> Menus { get; set; }

        public List<Views> Views { get; set; }

        public List<Users> Users { get; set; }

        public List<Organizes> Organizes { get; set; }

        public List<UserOrganize> UserOrganize { get; set; }

        public List<Roles> Roles { get; set; }

        public List<UserRole> UserRole { get; set; }

        public List<RolePermissions> RolePermissions { get; set; }

        public List<ArticleCategories> ArticleCategories { get; set; }
    }
}
