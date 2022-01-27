using FreeSql;
using OnceMi.Framework.Entity.Admin;

namespace OnceMi.Framework.IRepository
{
    public interface IApiRepository : IBaseRepository<Api, long>, IRepositoryDependency
    {

    }
}
