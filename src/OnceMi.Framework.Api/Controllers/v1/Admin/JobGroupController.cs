using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Enums;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnceMi.Framework.Api.Controllers.v1.Admin
{
    /// <summary>
    /// 作业分组管理
    /// </summary>
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class JobGroupController : ControllerBase
    {
        private readonly ILogger<JobGroupController> _logger;
        private readonly IJobGroupsService _service;
        private readonly IMapper _mapper;

        public JobGroupController(ILogger<JobGroupController> logger
            , IJobGroupsService service
            , IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IPageResponse<JobGroupItemResponse>> Get([FromQuery] IPageRequest request)
        {
            return await _service.Query(request);
        }

        /// <summary>
        /// 查询分组信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<JobGroupItemResponse> Get(long id)
        {
            return await _service.Query(id);
        }

        /// <summary>
        /// 查询分组列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<List<ISelectResponse<long>>> SelectList(long id)
        {
            var data = await _service.Query(new IPageRequest()
            {
                Page = 1,
                Size = 999999,
                OrderBy = new string[] { "id,asc" },
            });
            if (data != null && data.PageData != null && data.PageData.Any())
            {
                return data.PageData.Select(p => new ISelectResponse<long>()
                {
                    Name = p.Name,
                    Value = p.Id
                })
                    .ToList();
            }
            return new List<ISelectResponse<long>>();
        }

        /// <summary>
        /// 新增分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JobGroupItemResponse> Post(CreateJobGroupRequest request)
        {
            return await _service.Insert(request);
        }

        /// <summary>
        /// 修改分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task Put(UpdateJobGroupRequest request)
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
