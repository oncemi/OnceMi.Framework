using FreeRedis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Enums;
using OnceMi.Framework.Model.Exception;
using System;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnceMi.Framework.Api.Controllers.v1.Admin
{
    /// <summary>
    /// 文件管理
    /// </summary>
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "SysAdmin")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly RedisClient _redisClient;
        private readonly IUpLoadFilesService _upLoadFilesService;

        public FileController(ILogger<FileController> logger
            , IIdGeneratorService idGenerator
            , RedisClient redisClient
            , IUpLoadFilesService upLoadFilesService)
        {
            _logger = logger;
            _idGenerator = idGenerator;
            _redisClient = redisClient;
            _upLoadFilesService = upLoadFilesService;
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <returns></returns>
        // GET: api/<UploadController>
        [HttpGet]
        [Route("[action]")]
        public async Task<string> Download()
        {
            string set = _idGenerator.NewId().ToString();

            _redisClient.Set("USER_JWT_STATUS:820122670_jwt_status", set, TimeSpan.FromSeconds(110));

            await _upLoadFilesService.Where(p => true).ToListAsync();

            return await Task.FromResult(_redisClient.Get("key"));
        }

        // GET api/<UploadController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            throw new BusException(10010, "获取数据失败！");
        }

        // POST api/<UploadController>
        [HttpPost]
        [Route("[action]")]
        public void Upload([FromBody] string value)
        {
            throw new Exception("未知的错误！", new Exception("这个是错误栈！"));
        }

        // DELETE api/<UploadController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {

        }
    }
}
