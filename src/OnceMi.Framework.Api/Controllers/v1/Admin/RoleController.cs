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
    /// 角色管理
    /// </summary>
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly ILogger<RoleController> _logger;
        private readonly IRolesService _service;
        private readonly IMapper _mapper;

        public RoleController(ILogger<RoleController> logger
            , IRolesService rolesService
            , IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = rolesService ?? throw new ArgumentNullException(nameof(rolesService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// 获取下一排序顺序
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(nameof(GetNextSortValue))]
        public async Task<int> GetNextSortValue([FromQuery] long? parentId)
        {
            return await _service.QueryNextSortValue(parentId);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IPageResponse<RoleItemResponse>> Get([FromQuery] IPageRequest request)
        {
            return await _service.Query(request);
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
            }, true);
            if (data != null && data.PageData != null && data.PageData.Any())
            {
                return _mapper.Map<List<ICascaderResponse>>(data.PageData);
            }
            return new List<ICascaderResponse>();
        }

        /// <summary>
        /// 根据ID查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<RoleItemResponse> Get(long id)
        {
            return await _service.Query(id);
        }

        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<RoleItemResponse> Post(CreateRoleRequest request)
        {
            return await _service.Insert(request);
        }

        /// <summary>
        /// 修改角色
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task Put(UpdateRoleRequest request)
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
