using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Dto;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface IConfigService : IBaseService<Configs, long>
    {
        Task<Stream> ExportTestData();

        /// <summary>
        /// 获取应用程序信息
        /// </summary>
        /// <returns></returns>
        Task<SystemHardwareInfo> HardwareInfo();
    }
}
