using AutoMapper;
using FreeRedis;
using Hardware.Info;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Entity.Article;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Dto.Response.Admin.Config;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.Cache;
using OnceMi.Framework.Util.Json;
using OnceMi.Framework.Util.User;
using System.Text;

namespace OnceMi.Framework.Service.Admin
{
    public class ConfigService : BaseService<Configs, long>, IConfigService
    {
        private readonly IConfigRepository _repository;
        private readonly ILogger<ConfigService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;
        private readonly RedisClient _redisClient;

        public ConfigService(IConfigRepository repository
            , ILogger<ConfigService> logger
            , IIdGeneratorService idGenerator
            , IHttpContextAccessor accessor
            , IMapper mapper
            , RedisClient redisClient) : base(repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _redisClient = redisClient ?? throw new ArgumentNullException(nameof(redisClient));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _accessor = accessor;
        }

        #region 硬件信息

        public Task<SystemHardwareInfo> HardwareInfo()
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
            return Task.FromResult(hardwareInfo);
        }

        #endregion

        #region 维护相关

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

        #endregion

        #region 配置项管理

        /// <summary>
        /// 通过key查询配置项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key</param>
        /// <returns></returns>
        /// <exception cref="BusException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<ConfigModel<T>> QueryConfig<T>(string key) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new BusException(ResultCode.CONFIG_KEY_CANNOT_NULL, "获取配置项失败，参数KEY不能为空");
            }
            return await _redisClient.GetOrCreateAsync<ConfigModel<T>>(CacheConstant.GetConfigKey(key), async () =>
            {
                Configs config = await _repository.Where(p => p.Key == key).FirstAsync();
                if (config == null)
                {
                    throw new BusException(ResultCode.CONFIG_CANNOT_FING_KEY, $"获取配置项失败，未找到配置项：{key}");
                }
                ConfigModel<T> result = _mapper.Map<ConfigModel<T>>(config);
                if (result == null)
                {
                    throw new Exception($"AutoMapper映射对象{nameof(Configs)}至{nameof(ConfigModel<T>)}时失败");
                }
                if (string.IsNullOrEmpty(config.Content))
                {
                    result.Data = default;
                }
                result.Data = JsonUtil.DeserializeStringToObject<T>(config.Content);
                return result;
            });
        }

        /// <summary>
        /// 添加或者更新配置项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="BusException"></exception>
        public async Task AddOrUpdateQueryConfig<T>(ConfigModel<T> data) where T : class, new()
        {
            if (data == null)
            {
                throw new BusException(ResultCode.CONFIG_KEY_CANNOT_NULL, "设置配置项失败，参数不能为空");
            }
            if (string.IsNullOrWhiteSpace(data.Key))
            {
                throw new BusException(ResultCode.CONFIG_KEY_CANNOT_NULL, "设置配置项失败，参数KEY不能为空");
            }
            string content = "";
            if (data.Data != null)
            {
                content = JsonUtil.SerializeToString(data.Data);
            }
            Configs config = await _repository.Where(p => p.Key == data.Key).FirstAsync();
            if (config == null)
            {
                config = new Configs()
                {
                    Id = _idGenerator.NewId(),
                    Content = content,
                    Description = data.Description,
                    CreatedTime = data.CreatedTime,
                    CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id
                };
                await _repository.InsertAsync(config);
            }
            else
            {
                config.Content = content;
                config.UpdatedTime = DateTime.Now;
                config.UpdatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
                await _repository.UpdateAsync(config);
                //清空缓存，如果存在的话
                if (_redisClient.Exists(CacheConstant.GetConfigKey(data.Key)))
                {
                    _redisClient.Del(CacheConstant.GetConfigKey(data.Key));
                }
            }
        }

        /// <summary>
        /// 删除配置项
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task DeleteConfig(List<string> keys)
        {
            if (keys == null || keys.Count == 0)
            {
                return;
            }
            await _repository.Where(p => keys.Contains(p.Key))
                .ToDelete()
                .ExecuteAffrowsAsync();
        }

        #endregion
    }
}
