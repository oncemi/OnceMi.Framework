using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using OnceMi.Framework.Config;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Extension.Authorizations;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Enums;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.Http;
using OnceMi.Framework.Util.User;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnceMi.Framework.Api.Controllers.v1.Admin
{
    /// <summary>
    /// 文件管理
    /// </summary>
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private readonly IUpLoadFileService _service;
        private readonly ConfigManager _config;

        public FileController(ILogger<FileController> logger
            , IUpLoadFileService service
            , ConfigManager config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// 获取访问权限Select
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        [SkipAuthorization]
        public List<ISelectResponse<string>> AccessModeSelectList()
        {
            return _service.QueryAccessModes();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IPageResponse<FileItemResponse>> PageList([FromQuery] IPageRequest request)
        {
            return await _service.Query(request);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <remarks>
        /// 这种携带token的方式其实是非常不安全的，截获token之后就可以使用改token下载其它文件或通过解析jwttoken知道谁在下载文件 <br />
        /// 其它解决方案 <br />
        /// 1、针对改文件生成一个临时的url，提供用户下载 <br />
        /// 2、使用blob，但是问题是OSS中的文件无法通过这种方式下载 <br />
        /// 3、下载文件之前请求一次获取一个临时的token，然后通过临时token来下载文件，缺点就是实在太复杂 <br />
        /// </remarks>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get(string key, string token)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new BusException(ResultCode.FILE_REQUEST_KEY_CANNOT_NULL, "请求文件key不能为空");
            }
            //解码
            UploadFileInfo fileInfo = _service.DecodeFileKey(key);
            //判断访问权限，暂不支持，没找到覆盖重新授权策略的方式
            switch (fileInfo.AccessMode)
            {
                case FileAccessMode.PublicRead:
                    {
                        //公共读
                        //任何人都可以查看
                    }
                    break;
                case FileAccessMode.Private:
                    {
                        //私有文件
                        //仅自己可以查询或下载
                        if (!TryParseToken(token, out JwtSecurityToken securityToken))
                        {
                            throw new BusException(ResultCode.FILE_TOKEN_INVALID, "请求token验证失败");
                        }
                        if (securityToken.ValidTo < DateTime.UtcNow)
                        {
                            throw new BusException(ResultCode.FILE_TOKEN_EXPIRED, "请求token已过期");
                        }
                        long? userId = securityToken.Claims?.GetSubject().id;
                        if (userId == null)
                        {
                            throw new BusException(ResultCode.FILE_NO_DOWNLOAD_PERMISSION, "对不起，您没有该文件的访问权限");
                        }
                        if (fileInfo.Owner != userId)
                        {
                            throw new BusException(ResultCode.FILE_NO_DOWNLOAD_PERMISSION, "对不起，您没有该文件的访问权限");
                        }
                    }
                    break;
                case FileAccessMode.Inside:
                    {
                        //内部文件
                        //目前是登录用户即可查看或下载
                        if (!TryParseToken(token, out JwtSecurityToken securityToken))
                        {
                            throw new BusException(ResultCode.FILE_TOKEN_INVALID, "请求token验证失败");
                        }
                        if (securityToken.ValidTo < DateTime.UtcNow)
                        {
                            throw new BusException(ResultCode.FILE_TOKEN_EXPIRED, "请求token已过期");
                        }
                    }
                    break;
                default:
                    throw new BusException(ResultCode.FILE_PERMISSION_UNSUPPORT, "不支持的文件访问权限");
            }
            return await FileDownload(fileInfo);
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="request">请求参数</param>
        [HttpPost]
        [RequestSizeLimit(GlobalConfigConstant.FileUploadSizeLimit * 1024 * 1024)]
        public async Task<List<UploadFileInfo>> Post([FromForm] FileUploadRequest request)
        {
            if (request.AccessMode == 0)
            {
                request.AccessMode = FileAccessMode.Private;
            }
            IFormFileCollection files = HttpContext.Request.Form.Files;
            if (files == null || files.Count == 0)
            {
                throw new BusException(ResultCode.FILE_UPLOAD_IS_NULL, "上传的文件不能为空");
            }
            long? userId = HttpContext.User?.GetSubject().id;
            if (userId == null)
            {
                throw new BusException(ResultCode.FILE_GET_USERINFO_FAILED, "获取登录用户信息失败");
            }
            return await _service.Upload(files, userId.Value, request.AccessMode, request.ExpiredTime);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="ids"></param>
        [HttpDelete]
        public async Task Delete([FromBody] List<long> ids)
        {
            await _service.DeleteFile(ids);
        }

        #region private

        private async Task<IActionResult> FileDownload(UploadFileInfo file)
        {
            if (string.IsNullOrEmpty(file.Path))
            {
                throw new BusException(ResultCode.FILE_CANNOT_FIND_FILE_PATH, "获取文件参数失败，找不到指定的文件路径！");
            }
            if (file.ExpiredTime != null && (file.ExpiredTime.Value - DateTime.Now).TotalSeconds <= 0)
            {
                throw new BusException(ResultCode.FILE_EXPIRED, "该文件已过期！");
            }
            switch (file.StorageType)
            {
                case StorageType.Local:
                    {
                        if (!System.IO.File.Exists(file.Path))
                        {
                            throw new BusException(ResultCode.FILE_CANNOT_FIND_LOCALTION, "获取文件参数失败，找不到指定的文件存储位置！");
                        }
                        if (!new FileExtensionContentTypeProvider().TryGetContentType(file.Path, out string contentType))
                        {
                            contentType = "application/octet-stream";
                        }
                        return File(System.IO.File.OpenRead(file.Path), contentType, file.OldName);
                    }
                case StorageType.OSS:
                    {
                        int expiredSeconds = 48 * 3600;
                        if (file.ExpiredTime != null && (file.ExpiredTime.Value - DateTime.Now).TotalSeconds > 0)
                        {
                            expiredSeconds = (int)(file.ExpiredTime.Value - DateTime.Now).TotalSeconds;
                            if (expiredSeconds > 7 * 24 * 3600)
                            {
                                expiredSeconds = 7 * 24 * 3600;
                            }
                        }
                        if (string.IsNullOrEmpty(file.BucketName))
                        {
                            throw new BusException(ResultCode.FILE_CANNOT_FIND_LOCALTION, "获取文件参数失败，找不到指定的文件存储位置！");
                        }
                        string url = await _service.GetPresignedObjectUrl(file.Path, file.BucketName, expiredSeconds);
                        return Redirect(url);
                    }
                default:
                    throw new BusException(ResultCode.FILE_UNKNOW_STORAGE_TYPE, "未知的文件存储类型！");
            }
        }

        private bool TryParseToken(string token, out JwtSecurityToken jwtSecurityToken)
        {
            jwtSecurityToken = null;
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("Token can not null");
                }
                var handler = new JwtSecurityTokenHandler();
                var tokenObj = handler.ReadJwtToken(HttpUtil.UrlDecode(token));
                if (tokenObj == null)
                {
                    throw new Exception("Read token failed.");
                }
                jwtSecurityToken = tokenObj;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Try parse file download token failed, token is {token}, error is {ex.Message}");
                return false;
            }
        }

        #endregion

    }
}
