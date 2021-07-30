using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnceMi.Framework.Api.Controllers.v1.Admin
{
    /// <summary>
    /// 字典管理
    /// </summary>
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class DictionaryController : ControllerBase
    {
        private readonly ILogger<DictionaryController> _logger;
        private readonly IDictionariesService _service;
        private readonly IMapper _mapper;

        public DictionaryController(ILogger<DictionaryController> logger
            , IDictionariesService service
            , IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// 查询级联选择器数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(nameof(CascaderList))]
        public async Task<List<ICascaderResponse>> CascaderList()
        {
            var data = await _service.Query(new IPageRequest()
            {
                Page = 1,
                Size = 999999,
                OrderBy = new string[] { "id,asc" },
            });
            if (data != null && data.PageData != null && data.PageData.Any())
            {
                return _mapper.Map<List<ICascaderResponse>>(data.PageData);
            }
            return new List<ICascaderResponse>();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IPageResponse<DictionaryItemResponse>> Get([FromQuery] IPageRequest request)
        {
            return await _service.Query(request);
        }

        /// <summary>
        /// 根据ID查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<DictionaryItemResponse> Get(long id)
        {
            return await _service.Query(id);
        }

        /// <summary>
        /// 新增字典
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<DictionaryItemResponse> Post(CreateDictionaryRequest request)
        {
            return await _service.Insert(request);
        }

        /// <summary>
        /// 修改字典
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task Put(UpdateDictionaryRequest request)
        {
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
