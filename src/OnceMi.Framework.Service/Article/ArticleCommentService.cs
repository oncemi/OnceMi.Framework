using AutoMapper;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Entity.Article;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Article;
using System;

namespace OnceMi.Framework.Service.Article
{
    public class ArticleCommentService : BaseService<ArticleComments, long>, IArticleCommentService
    {
        private readonly IArticleCommentRepository _repository;
        private readonly ILogger<ArticleCommentService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IMapper _mapper;

        public ArticleCommentService(IArticleCommentRepository repository
            , ILogger<ArticleCommentService> logger
            , IIdGeneratorService idGenerator
            , IMapper mapper) : base(repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }


    }
}
