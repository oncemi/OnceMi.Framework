using FreeSql;
using OnceMi.Framework.Entity.Admin;

namespace OnceMi.Framework.IRepository
{
    public interface IDemoRepository : IBaseRepository<Configs, long>, IRepositoryDependency
    {
        Task<List<Configs>> GetAllConfigs();
    }
}
