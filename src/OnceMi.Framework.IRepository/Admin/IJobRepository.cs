using FreeSql;
using OnceMi.Framework.Entity.Admin;

namespace OnceMi.Framework.IRepository
{
    public interface IJobRepository : IBaseRepository<Jobs, long>, IRepositoryDependency
    {

    }
}
