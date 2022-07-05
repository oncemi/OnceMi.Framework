using System.Collections.Generic;

namespace OnceMi.Framework.Model.Dto
{
    public class UserRolePermissionResponse : IResponse
    {
        public long Id { get; set; }

        public List<string> Operation { get; set; } = new List<string>();
    }
}
