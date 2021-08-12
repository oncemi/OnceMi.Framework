using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Enums;
using System;
using System.Threading.Tasks;

namespace OnceMi.Framework.Api.Controllers.v2
{
    /// <summary>
    /// 维护
    /// </summary>
    [ApiController]
    [ApiVersion(ApiVersions.V2)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class MaintainController : ControllerBase
    {
        private readonly IConfigsService _service;

        public MaintainController(IConfigsService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// 导出基础数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Export()
        {
            var stream = await _service.ExportMaintainData();
            return File(stream, "application/json", "数据导出.json");
        }

    }
}
