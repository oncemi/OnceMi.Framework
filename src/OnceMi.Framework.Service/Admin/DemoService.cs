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
    public class DemoService : BaseService<Configs, long>, IDemoService
    {
        private readonly IDemoRepository _repository;
        private readonly ILogger<DemoService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;
        private readonly RedisClient _redisClient;

        public DemoService(IDemoRepository repository
            , ILogger<DemoService> logger
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

        #region 分库查询

        public async Task<List<Configs>> GetAllConfigs()
        {
            return await _repository.GetAllConfigs();
        }

        #endregion
    }
}
