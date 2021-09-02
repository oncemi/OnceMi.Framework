using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Extension.Authorizations;
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
    /// 组织机构管理
    /// </summary>
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class OrganizeController : ControllerBase
    {
        private readonly ILogger<OrganizeController> _logger;
        private readonly IOrganizesService _service;
        private readonly IUsersService _usersService;
        private readonly IMapper _mapper;

        public OrganizeController(ILogger<OrganizeController> logger
            , IOrganizesService rolesService
            , IUsersService usersService
            , IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = rolesService ?? throw new ArgumentNullException(nameof(rolesService));
            _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// 获取组织类型
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        [NoAuthorize]
        public List<ISelectResponse<int>> OrganizeTypeSelectList()
        {
            return _service.QueryOrganizeTypes();
        }

        /// <summary>
        /// 查询组织列表（选择框）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        [NoAuthorize]
        public async Task<List<ISelectResponse<long>>> UserSelectList(string query)
        {
            return await _usersService.GetUserSelectList(query);
        }

        /// <summary>
        /// 查询级联选择器数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<List<ICascaderResponse>> CascaderList()
        {
            var data = await _service.Query(new OrganizePageRequest()
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
        /// 查询用户管理页面树
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IEnumerable<OrganizeItemResponse>> OrganizeTreeList()
        {
            var data = await _service.Query(new OrganizePageRequest()
            {
                Page = 1,
                Size = 999999,
                OrderBy = new string[] { "id,asc" },
            }, true);
            if (data != null && data.PageData != null && data.PageData.Any())
            {
                return data.PageData;
            }
            return new List<OrganizeItemResponse>();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IPageResponse<OrganizeItemResponse>> Get([FromQuery] OrganizePageRequest request)
        {
            return await _service.Query(request);
        }

        /// <summary>
        /// 根据ID查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<OrganizeItemResponse> Get(long id)
        {
            return await _service.Query(id);
        }

        /// <summary>
        /// 新增组织
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<OrganizeItemResponse> Post(CreateOrganizeRequest request)
        {
            return await _service.Insert(request);
        }

        /// <summary>
        /// 修改组织
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task Put(UpdateOrganizeRequest request)
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
