using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class UserRolePermissionResponse : IResponse
    {
        public long Id { get; set; }

        public List<string> Operation { get; set; } = new List<string>();
    }
}
