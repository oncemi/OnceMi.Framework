using AutoMapper;
using FreeRedis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using System;

namespace OnceMi.Framework.Service.Admin
{
    public class JobHistoriesService : BaseService<JobHistories, long>, IJobHistoriesService
    {
        private readonly IJobHistoriesRepository _repository;
        private readonly ILogger<JobHistoriesService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IMapper _mapper;
        private readonly RedisClient _redis;

        public JobHistoriesService(IJobHistoriesRepository repository
            , ILogger<JobHistoriesService> logger
            , IIdGeneratorService idGenerator
            , IMapper mapper
            , RedisClient redis) : base(repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        }


    }
}
