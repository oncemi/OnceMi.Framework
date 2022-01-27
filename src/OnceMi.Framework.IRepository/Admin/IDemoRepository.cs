using FreeSql;
using OnceMi.Framework.Entity.Admin;

namespace OnceMi.Framework.IRepository
{
    public interface IDemoRepository : IBaseRepository<Config, long>, IRepositoryDependency
    {
        Task<List<Config>> GetAllConfigs();
    }
}
