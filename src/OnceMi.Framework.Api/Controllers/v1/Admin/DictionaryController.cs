using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Extension.Authorizations;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Enums;
using OnceMi.Framework.Model.Exceptions;
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
        private readonly IDictionaryService _service;
        private readonly IMapper _mapper;

        public DictionaryController(ILogger<DictionaryController> logger
            , IDictionaryService service
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
        [Route("[action]")]
        public async Task<List<ICascaderResponse>> CascaderList()
        {
            var data = await _service.Query(new IPageRequest()
            {
                Page = 1,
                Size = int.MaxValue,
                OrderBy = new string[] { $"{nameof(DictionaryItemResponse.Sort)},asc" },
            });
            if (data != null && data.PageData != null && data.PageData.Any())
            {
                return _mapper.Map<List<ICascaderResponse>>(data.PageData);
            }
            return new List<ICascaderResponse>();
        }

        /// <summary>
        /// 查询字典树 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IEnumerable<DictionaryItemResponse>> DictionaryTreeList()
        {
            var data = await _service.Query(new IPageRequest()
            {
                Page = 1,
                Size = int.MaxValue,
                OrderBy = new string[] { $"{nameof(DictionaryItemResponse.Sort)},asc" },
            });
            if (data != null && data.PageData != null && data.PageData.Any())
            {
                return data.PageData;
            }
            return new List<DictionaryItemResponse>();
        }

        /// <summary>
        /// 获取下一排序顺序
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        [SkipAuthorization]
        public async ValueTask<int> NextSortValue([FromQuery] long? parentId)
        {
            return await _service.QueryNextSortValue(parentId);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IPageResponse<DictionaryItemResponse>> PageList([FromQuery] IPageRequest request)
        {
            return await _service.Query(request);
        }

        /// <summary>
        /// 查询字典详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<DictionaryItemResponse> Get([FromQuery] DictionaryDetailRequest request)
        {
            if (string.IsNullOrEmpty(request.Code) && (request.Id == null || request.Id == 0))
            {
                throw new BusException(ResultCode.DIC_ID_AND_CODE_CANNOT_ALL_EMPTY, "查询条件Id和编码不能同时为空");
            }
            return await _service.Query(request);
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
