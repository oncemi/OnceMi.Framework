﻿using FreeSql;
using OnceMi.Framework.Entity.Admin;

namespace OnceMi.Framework.IRepository
{
    public interface IPermissionRepository : IBaseRepository<RolePermission, long>, IRepositoryDependency
    {

    }
}
