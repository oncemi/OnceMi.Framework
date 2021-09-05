using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Config;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Extension.Authorizations;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Enums;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.User;
using System;
using System.Collections.Generic;
using System.IO;
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
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private readonly IUpLoadFilesService _service;
        private readonly ConfigManager _config;

        public FileController(ILogger<FileController> logger
            , IUpLoadFilesService service
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
        [NoAuthorize]
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
        public async Task<IPageResponse<FileItemResponse>> QueryByPage([FromQuery] IPageRequest request)
        {
            return await _service.Query(request);
        }

        /// <summary>
        /// 下载文件（需要认证）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="responseType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Download(string key, FileResponseType responseType = FileResponseType.Binary)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new BusException(-1, "请求文件key不能为空");
            }
            //解码
            UploadFileInfo fileInfo = _service.DecodeFileKey(key);
            //判断访问权限，暂不支持，没找到覆盖重新授权策略的方式
            if (fileInfo.AccessMode == FileAccessMode.Private)
            {
                long? userId = HttpContext.User?.GetSubject().id;
                if (userId == null)
                {
                    throw new BusException(-1, "对不起，您没有改文件的访问权限");
                }
                if (fileInfo.Owner != userId)
                {
                    throw new BusException(-1, "对不起，您没有改文件的访问权限");
                }
            }
            return await FileDownload(fileInfo, responseType);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="responseType"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get(string key, FileResponseType responseType = FileResponseType.Binary)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new BusException(-1, "请求文件key不能为空");
            }
            //解码
            UploadFileInfo fileInfo = _service.DecodeFileKey(key);
            //判断访问权限，暂不支持，没找到覆盖重新授权策略的方式
            if (fileInfo.AccessMode != FileAccessMode.PublicRead
                && fileInfo.AccessMode != FileAccessMode.PublicReadAndWrite)
            {
                return RedirectToAction("Download", new { key = key });
            }
            return await FileDownload(fileInfo, responseType);
        }

        /// <summary>
        /// 文件上传，文件上传限制为：2147483648 Byte(2G)
        /// </summary>
        /// <param name="request">请求参数</param>
        [HttpPost]
        [RequestSizeLimit(2147483648)]
        public async Task<List<UploadFileInfo>> Post([FromForm] FileUploadRequest request)
        {
            if (request.AccessMode == 0)
            {
                request.AccessMode = FileAccessMode.Private;
            }
            IFormFileCollection files = HttpContext.Request.Form.Files;
            if (files == null || files.Count == 0)
            {
                throw new BusException(-1, "上传的文件为空");
            }
            long? userId = HttpContext.User?.GetSubject().id;
            if (userId == null)
            {
                throw new BusException(-1, "获取登录用户信息失败");
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

        private async Task<IActionResult> FileDownload(UploadFileInfo file, FileResponseType responseType)
        {
            if (string.IsNullOrEmpty(file.Path))
            {
                throw new BusException(-1, "获取文件参数失败，找不到指定的文件路径！");
            }
            if (file.ExpiredTime != null && (file.ExpiredTime.Value - DateTime.Now).TotalSeconds <= 0)
            {
                throw new BusException(-1, "该文件已到期！");
            }
            switch (file.StorageType)
            {
                case StorageType.Local:
                    {
                        if (!System.IO.File.Exists(file.Path))
                        {
                            throw new BusException(-1, "获取文件参数失败，找不到指定的文件存储位置！");
                        }
                        if (responseType == FileResponseType.Blob)
                        {
                            using (FileStream stream = System.IO.File.OpenRead(file.Path))
                            {
                                byte[] data = new byte[stream.Length];
                                await stream.ReadAsync(data);

                                return File(data, "application/octet-stream", file.OldName);
                            }
                        }
                        else
                        {
                            if (!new FileExtensionContentTypeProvider().TryGetContentType(file.Path, out string contentType))
                            {
                                contentType = "application/octet-stream";
                            }
                            return File(System.IO.File.OpenRead(file.Path), contentType, file.OldName);
                        }
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
                            throw new BusException(-1, "获取文件参数失败，找不到指定的文件存储位置！");
                        }
                        string url = await _service.GetPresignedObjectUrl(file.Path, file.BucketName, expiredSeconds);
                        return Redirect(url);
                    }
                default:
                    throw new BusException(-1, "未知的文件存储类型！");
            }
        }

        #endregion

    }
}
