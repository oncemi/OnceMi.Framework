using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OnceMi.Framework.Extension.Authorizations;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnceMi.Framework.Api.Controllers.v1.Admin
{
    /// <summary>
    /// 接口管理
    /// </summary>
    [ApiController]
    [ApiVersion(Model.Enums.ApiVersions.V1)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;
        private readonly IApiService _service;
        private readonly IMapper _mapper;

        public ApiController(ILogger<ApiController> logger
            , IApiService service
            , IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// 获取Api版本
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        [SkipAuthorization]
        public List<ISelectResponse<string>> ApiVersions()
        {
            return _service.QueryApiVersions();
        }

        /// <summary>
        /// 查询级联选择器
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<List<ICascaderResponse>> CascaderList()
        {
            var data = await _service.Query(new QueryApiPageRequest()
            {
                Page = 1,
                Size = int.MaxValue,
                OrderBy = new string[] { "id,asc" },
            }, true);
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
        [Route("[action]")]
        public async Task<IPageResponse<ApiItemResponse>> PageList([FromQuery] QueryApiPageRequest request)
        {
            return await _service.Query(request);
        }

        /// <summary>
        /// 查询接口详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ApiItemResponse> Get(long id)
        {
            var result = await _service.Query(id);
            return result;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiItemResponse> Post(CreateApiRequest request)
        {
            if (!request.Version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                throw new BusException(ResultCode.API_VERSION_NAME_MUST_STARTWITH_V, "Api版本必须使用‘v’开头。");
            }
            if (!double.TryParse(request.Version.Replace("v", ""), out double _))
            {
                throw new BusException(ResultCode.API_VERSION_MUST_NUMBER, "Api版本号必须为数字。");
            }
            if (!string.IsNullOrEmpty(request.Method))
            {
                if (!Enum.TryParse(request.Method, true, out OperationType method))
                {
                    throw new BusException(ResultCode.API_NOT_SUPPORT_REQUEST_TYPE, $"不允许的操作类型：{request.Method}。");
                }
                request.Method = method.ToString();
            }
            if (request.Path == "/")
            {
                throw new BusException(ResultCode.API_REQUEST_PATH_CANNOT_ROOT, $"请求路径不能为根目录。");
            }
            if (string.IsNullOrEmpty(request.Method) && request.ParentId != null)
            {
                throw new BusException(ResultCode.API_NOT_ROOT_REQUEST_METHOD_CANNOT_NULL, "当Api不为个根节点（控制器）时，请求方式不能为空");
            }
            return await _service.Insert(request);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task Put(UpdateApiRequest request)
        {
            if (!request.Version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                throw new BusException(ResultCode.API_VERSION_NAME_MUST_STARTWITH_V, "Api版本必须使用‘v’开头。");
            }
            if (!double.TryParse(request.Version.Replace("v", ""), out double _))
            {
                throw new BusException(ResultCode.API_VERSION_MUST_NUMBER, "Api版本号必须为数字。");
            }
            if (!string.IsNullOrEmpty(request.Method))
            {
                if (!Enum.TryParse(request.Method, true, out OperationType method))
                {
                    throw new BusException(ResultCode.API_NOT_SUPPORT_REQUEST_TYPE, $"不允许的操作类型：{request.Method}。");
                }
                request.Method = method.ToString();
            }
            if (request.Path == "/")
            {
                throw new BusException(ResultCode.API_REQUEST_PATH_CANNOT_ROOT, $"请求路径不能为根目录。");
            }
            if (string.IsNullOrEmpty(request.Method) && request.ParentId != null)
            {
                throw new BusException(ResultCode.API_NOT_ROOT_REQUEST_METHOD_CANNOT_NULL, "当Api不为个根节点（控制器）时，请求方式不能为空");
            }

            await _service.Update(request);
        }

        /// <summary>
        /// 同步
        /// </summary>
        [HttpPost]
        [Route("[action]")]
        public async Task Sync()
        {
            await _service.Sync();
        }

        /// <summary>
        /// 删除
        /// </summary>
        [HttpDelete]
        public async Task Delete(List<long> ids)
        {
            await _service.Delete(ids);
        }
    }
}
