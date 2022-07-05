using AutoMapper;
using FreeRedis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class DemoService : BaseService<Entity.Admin.Config, long>, IDemoService
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

        public async Task<List<Entity.Admin.Config>> GetAllConfigs()
        {
            return await _repository.GetAllConfigs();
        }

        #endregion
    }
}
