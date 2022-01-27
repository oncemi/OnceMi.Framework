using FreeSql;
using OnceMi.Framework.Entity.Admin;

namespace OnceMi.Framework.IRepository
{
    public interface IUserRepository : IBaseRepository<UserInfo, long>, IRepositoryDependency
    {

    }
}
