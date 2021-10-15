using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.IService.Article;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Extension.Authorizations;

namespace OnceMi.Framework.Api.Controllers.v1.ar
{
    /// <summary>
    /// 文章管理
    /// </summary>
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ArticleController : ControllerBase
    {
        private readonly ILogger<ArticleController> _logger;
        private readonly IArticleService _service;
        private readonly IArticleTagService _articleTagService;

        public ArticleController(ILogger<ArticleController> logger
            , IArticleService service
            , IArticleTagService articleTagService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _articleTagService = articleTagService ?? throw new ArgumentNullException(nameof(articleTagService));
        }

        /// <summary>
        /// 查询所有标签
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        [SkipAuthorization]
        public async Task<List<ISelectResponse<string>>> AllTags()
        {
            List<ArticleTagResponse> responses = await _articleTagService.QueryAllTags();
            return responses.Select(p => new ISelectResponse<string>()
            {
                Value = p.Tag,
                Name = p.Tag
            }).ToList();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IPageResponse<ArticleResponse>> Get([FromQuery] QueryArticlePageRequest request)
        {
            return await _service.Query(request);
        }

        /// <summary>
        /// 根据ID查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ArticleResponse> Get(long id)
        {
            return await _service.Query(id);
        }

        /// <summary>
        /// 新增文章
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ArticleResponse> Post(CreateOrUpdateArticleRequest request)
        {
            if (request.Categories == null || request.Categories.Count == 0)
            {
                throw new BusException(-1, "文章分类不能为空");
            }
            return await _service.Insert(request);
        }

        /// <summary>
        /// 修改文章
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task Put(CreateOrUpdateArticleRequest request)
        {
            if (request.Id == null || request.Id == 0)
            {
                throw new BusException(-1, "更新文章时文章Id不能为空");
            }
            if (request.Categories == null || request.Categories.Count == 0)
            {
                throw new BusException(-1, "文章分类不能为空");
            }
            await _service.Update(request);
        }

        /// <summary>
        /// 根据Id删除
        /// </summary>
        [HttpDelete]
        public async Task Delete(List<long> ids)
        {
            await _service.Delete(ids);
        }
    }
}
