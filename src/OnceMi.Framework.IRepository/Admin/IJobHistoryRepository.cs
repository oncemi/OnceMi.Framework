using FreeSql;
using OnceMi.Framework.Entity.Admin;

namespace OnceMi.Framework.IRepository
{
    public interface IJobHistoryRepository : IBaseRepository<JobHistory, long>, IRepositoryDependency
    {

    }
}
