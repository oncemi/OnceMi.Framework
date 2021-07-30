using FreeSql;
using OnceMi.IdentityServer4.User.Entities;

namespace OnceMi.Framework.IRepository
{
    public interface IUsersRepository : IBaseRepository<Users, long>, IFrameRepository
    {

    }
}
