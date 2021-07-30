using OnceMi.IdentityServer4.User.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class OrganizePageRequest : IPageRequest
    {
        public OrganizeType? OrganizeType { get; set; }
    }
}
