using Hardware.Info;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Entity.Article;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Util.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class ConfigService : BaseService<Configs, long>, IConfigService
    {
        private readonly IConfigRepository _repository;
        private readonly ILogger<ConfigService> _logger;

        public ConfigService(IConfigRepository repository
            , ILogger<ConfigService> logger) : base(repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SystemHardwareInfo> SystemHardwareInfo()
        {
            return await Task.Run(() =>
            {
                SystemHardwareInfo hardwareInfo = new SystemHardwareInfo();

                IHardwareInfo hardwareInfoHandler = new HardwareInfo();
                hardwareInfoHandler.RefreshMemoryStatus();
                hardwareInfoHandler.RefreshCPUList(includePercentProcessorTime: false);

                hardwareInfo.TotalPhysicalMemory = hardwareInfoHandler.MemoryStatus.TotalPhysical / 1024 / 1024;
                hardwareInfo.AvailablePhysicalMemory = hardwareInfoHandler.MemoryStatus.AvailablePhysical / 1024 / 1024;
                //cpu
                for (int i = 0; i < hardwareInfoHandler.CpuList.Count; i++)
                {
                    var cpuInfo = new SystemCpuHardwareInfo()
                    {
                        Num = i + 1,
                        Name = hardwareInfoHandler.CpuList[i].Name,
                        MaxClockSpeed = hardwareInfoHandler.CpuList[i].MaxClockSpeed / 1000.0,
                        NumberOfCores = hardwareInfoHandler.CpuList[i].NumberOfCores,
                    };
                    if (cpuInfo.NumberOfCores == 0)
                    {
                        cpuInfo.NumberOfCores = (uint)Environment.ProcessorCount;
                    }
                    hardwareInfo.CpuInfos.Add(cpuInfo);
                }
                return hardwareInfo;
            });
        }

        /// <summary>
        /// 导出维护数据
        /// 导出的数据为转义之后的json字符串，可在网页中进行去除转义和直接复制到导入程序中
        /// </summary>
        /// <returns></returns>
        public async Task<Stream> ExportTestData()
        {
            DatabaseEntities data = new DatabaseEntities
            {
                Apis = await _repository.Orm.Select<Apis>().ToListAsync(),
                Views = await _repository.Orm.Select<Views>().ToListAsync(),
                Menus = await _repository.Orm.Select<Menus>().ToListAsync(),
                RolePermissions = await _repository.Orm.Select<RolePermissions>().ToListAsync(),
                Users = await _repository.Orm.Select<Users>().ToListAsync(),
                Organizes = await _repository.Orm.Select<Organizes>().ToListAsync(),
                UserOrganize = await _repository.Orm.Select<UserOrganize>().ToListAsync(),
                Roles = await _repository.Orm.Select<Roles>().ToListAsync(),
                UserRole = await _repository.Orm.Select<UserRole>().ToListAsync(),
                Jobs = await _repository.Orm.Select<Jobs>().ToListAsync(),
                JobGroups = await _repository.Orm.Select<JobGroups>().ToListAsync(),
                ArticleCategories = await _repository.Orm.Select<ArticleCategories>().ToListAsync(),
            };
            string source = JsonUtil.SerializeToString(data);
            if (source == null)
            {
                throw new Exception("Convert data to json failed.");
            }
            object jsonObj = new
            {
                data = source,
            };
            string json = JsonUtil.SerializeToString(jsonObj)?[8..^1];
            if (string.IsNullOrEmpty(json))
            {
                throw new Exception("Serialize data to string failed.");
            }
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            MemoryStream ms = new MemoryStream(bytes);
            return ms;
        }
    }
}
