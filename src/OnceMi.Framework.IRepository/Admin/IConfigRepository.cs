﻿using FreeSql;
using OnceMi.Framework.Entity.Admin;

namespace OnceMi.Framework.IRepository
{
    public interface IConfigRepository : IBaseRepository<Configs, long>, IRepositoryDependency
    {

    }
}