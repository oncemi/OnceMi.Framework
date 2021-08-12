using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Util.Json;
using OnceMi.IdentityServer4.User.Entities;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class ConfigsService : BaseService<Configs, long>, IConfigsService
    {
        private readonly IConfigsRepository _repository;
        private readonly ILogger<ConfigsService> _logger;

        public ConfigsService(IConfigsRepository repository
            , ILogger<ConfigsService> logger) : base(repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 导出维护数据
        /// </summary>
        /// <returns></returns>
        public async Task<Stream> ExportMaintainData()
        {
            DatabaseEntities data = new DatabaseEntities
            {
                Apis = await _repository.Orm.Select<Apis>().ToListAsync(),
                JobGroups = await _repository.Orm.Select<JobGroups>().ToListAsync(),
                Menus = await _repository.Orm.Select<Menus>().ToListAsync(),
                RolePermissions = await _repository.Orm.Select<RolePermissions>().ToListAsync(),
                Views = await _repository.Orm.Select<Views>().ToListAsync(),
                Users = await _repository.Orm.Select<Users>().ToListAsync(),
                Organizes = await _repository.Orm.Select<Organizes>().ToListAsync(),
                UserOrganize = await _repository.Orm.Select<UserOrganize>().ToListAsync(),
                Roles = await _repository.Orm.Select<Roles>().ToListAsync(),
                UserRole = await _repository.Orm.Select<UserRole>().ToListAsync()
            };
            string json = JsonUtil.SerializeToString(data);
            if(json == null)
            {
                throw new Exception("Convert data to json failed.");
            }
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            MemoryStream ms = new MemoryStream(bytes);
            return ms;
        }
    }
}
