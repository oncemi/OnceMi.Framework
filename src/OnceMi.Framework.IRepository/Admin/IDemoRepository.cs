using FreeSql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.Framework.IRepository
{
    public interface IDemoRepository : IBaseRepository<Entity.Admin.Config, long>, IRepositoryDependency
    {
        Task<List<Entity.Admin.Config>> GetAllConfigs();
    }
}
