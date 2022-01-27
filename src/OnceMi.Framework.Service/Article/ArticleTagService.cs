using AutoMapper;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Entity.Article;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Article;
using OnceMi.Framework.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Article
{
    public class ArticleTagService : BaseService<ArticleTag, long>, IArticleTagService
    {
        private readonly IArticleTagRepository _repository;
        private readonly ILogger<ArticleTagService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IMapper _mapper;

        public ArticleTagService(IArticleTagRepository repository
            , ILogger<ArticleTagService> logger
            , IIdGeneratorService idGenerator
            , IMapper mapper) : base(repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<ArticleTagResponse>> QueryAllTags()
        {
            List<ArticleTagResponse> responses = await _repository.Orm.Select<ArticleTag>()
                .GroupBy(p => new { p.Tag })
                .ToListAsync(p => new ArticleTagResponse()
                {
                    Tag = p.Key.Tag
                });
            return responses;
        }
    }
}
