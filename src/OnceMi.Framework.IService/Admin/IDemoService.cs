using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Dto;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface IDemoService : IBaseService<Config, long>
    {
        Task<List<Config>> GetAllConfigs();
    }
}
