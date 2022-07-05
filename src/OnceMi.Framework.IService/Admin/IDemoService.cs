using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface IDemoService : IBaseService<Entity.Admin.Config, long>
    {
        Task<List<Entity.Admin.Config>> GetAllConfigs();
    }
}
