using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.AspNetCore.OSS;
using OnceMi.Framework.Config;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.Enum;
using OnceMi.Framework.Util.Extensions;
using OnceMi.Framework.Util.Http;
using OnceMi.Framework.Util.Json;
using OnceMi.Framework.Util.Reflection;
using OnceMi.Framework.Util.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class UpLoadFileService : BaseService<UpLoadFile, long>, IUpLoadFileService
    {
        private readonly IUpLoadFileRepository _repository;
        private readonly ILogger<UpLoadFileService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IOSSService _ossService;
        private readonly IHttpContextAccessor _accessor;
        private readonly ConfigManager _config;
        private readonly IMapper _mapper;

        public UpLoadFileService(IUpLoadFileRepository repository
            , ILogger<UpLoadFileService> logger
            , IIdGeneratorService idGenerator
            , IOSSService ossService
            , IHttpContextAccessor accessor
            , ConfigManager config
            , IMapper mapper) : base(repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(IUpLoadFileRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<UpLoadFileService>));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _ossService = ossService ?? throw new ArgumentNullException(nameof(ossService));
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public List<ISelectResponse<string>> QueryAccessModes()
        {
            List<EnumModel> enumModels = EnumUtil.EnumToList<FileAccessMode>();
            if (enumModels == null || enumModels.Count == 0)
            {
                return new List<ISelectResponse<string>>();
            }
            List<ISelectResponse<string>> result = enumModels
                .OrderBy(p => p.Value)
                .Select(p => new ISelectResponse<string>()
                {
                    Name = p.Description,
                    Value = p.Name,
                })
                .ToList();
            return result;
        }

        public async Task<IPageResponse<FileItemResponse>> Query(IPageRequest request)
        {
            IPageResponse<FileItemResponse> response = new IPageResponse<FileItemResponse>();
            Expression<Func<UpLoadFile, bool>> exp = p => !p.IsDeleted;
            if (!string.IsNullOrEmpty(request.Search))
            {
                exp = exp.And(p => p.Name.Contains(request.Search));
            }

            //get count
            long count = await _repository.Where(exp).CountAsync();
            List<UpLoadFile> allFiles = await _repository.Select
                .Page(request.Page, request.Size)
                .OrderBy(request.OrderByModels)
                .Include(p => p.Owner)
                .Where(exp)
                .NoTracking()
                .ToListAsync();
            if (allFiles == null || allFiles.Count == 0)
            {
                return new IPageResponse<FileItemResponse>()
                {
                    Page = request.Page,
                    Size = 0,
                    Count = count,
                    PageData = new List<FileItemResponse>(),
                };
            }
            foreach (var item in allFiles)
            {
                item.Url = BuildFileUrl(item);
            }
            return new IPageResponse<FileItemResponse>()
            {
                Page = request.Page,
                Size = allFiles.Count,
                Count = count,
                PageData = _mapper.Map<List<FileItemResponse>>(allFiles),
            };
        }

        #region 根据配置文件自动选中位置上传
        public async Task<UploadFileInfo> Upload(IFormFile file
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null)
        {
            if (_config.FileUpload.IsUploadToOSS)
            {
                return await UploadToOSS(file, _config.FileUpload.BucketName, GetOSSUploadPath(), owner, accessMode, expiredTime);
            }
            else
            {
                return await UploadToLocal(file, GetLocalUploadPath(), owner, accessMode, expiredTime);
            }
        }

        public async Task<List<UploadFileInfo>> Upload(IFormFileCollection files
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null)
        {
            if (_config.FileUpload.IsUploadToOSS)
            {
                return await UploadToOSS(files, _config.FileUpload.BucketName, GetOSSUploadPath(), owner, accessMode, expiredTime);
            }
            else
            {
                return await UploadToLocal(files, GetLocalUploadPath(), owner, accessMode, expiredTime);
            }
        }

        public async Task<List<UploadFileInfo>> Upload(List<IFormFile> files
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null)
        {
            if (_config.FileUpload.IsUploadToOSS)
            {
                return await UploadToOSS(files, _config.FileUpload.BucketName, GetOSSUploadPath(), owner, accessMode, expiredTime);
            }
            else
            {
                return await UploadToLocal(files, GetLocalUploadPath(), owner, accessMode, expiredTime);
            }
        }

        public async Task<UploadFileInfo> Upload(Stream stream
            , string oldName
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null)
        {
            if (_config.FileUpload.IsUploadToOSS)
            {
                return await UploadToOSS(stream, _config.FileUpload.BucketName, oldName, GetOSSUploadPath(), owner, accessMode, expiredTime);
            }
            else
            {
                return await UploadToLocal(stream, oldName, GetLocalUploadPath(), owner, accessMode, expiredTime);
            }
        }

        #endregion

        #region UploadToLocal

        public async Task<UploadFileInfo> UploadToLocal(IFormFile file
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null)
        {
            if (file == null)
            {
                throw new BusException(ResultCode.FILE_UPLOAD_IS_NULL, "上传文件为空");
            }
            List<IFormFile> upFiles = new List<IFormFile>()
            {
                file
            };
            var result = await UploadToLocal(upFiles, path, owner, accessMode, expiredTime);
            if (result != null && result.Count > 0)
            {
                return result[0];
            }
            throw new BusException(ResultCode.FILE_UPLOAD_FAILED, "文件上传失败");
        }

        public async Task<List<UploadFileInfo>> UploadToLocal(IFormFileCollection files
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null)
        {
            if (files == null || files.Count == 0)
            {
                throw new ArgumentNullException(nameof(files));
            }
            List<IFormFile> upFiles = new List<IFormFile>();
            foreach (var item in files)
            {
                upFiles.Add(item);
            }
            return await UploadToLocal(upFiles, path, owner, accessMode, expiredTime);
        }

        public async Task<List<UploadFileInfo>> UploadToLocal(List<IFormFile> files
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null)
        {
            if (files == null || files.Count == 0)
            {
                throw new BusException(ResultCode.FILE_UPLOAD_IS_NULL, "上传文件为空");
            }
            List<UploadFileInfo> result = new List<UploadFileInfo>();
            try
            {
                foreach (var item in files)
                {
                    using (var stream = item.OpenReadStream())
                    {
                        var upload = await UploadToLocal(stream, item.FileName, path, owner, accessMode, expiredTime);
                        result.Add(upload);
                    }
                }
            }
            catch (Exception ex)
            {
                if (result.Count > 0)
                {
                    await DeleteFile(result);
                }
                _logger.LogError(ex, $"Upload file failed, error: {ex.Message}");
                throw;
            }
            return result;
        }

        public async Task<UploadFileInfo> UploadToLocal(Stream stream
            , string oldName
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null)
        {
            if (stream == null || stream.Length == 0)
            {
                throw new BusException(ResultCode.FILE_STREAM_IS_NULL, "文件流为空");
            }
            if (stream.Length == 0)
            {
                throw new BusException(ResultCode.FILE_STREAM_IS_NULL, "文件流为空");
            }
            if (!stream.CanRead)
            {
                throw new BusException(ResultCode.FILE_STREAM_CANNOT_READ, "无法读取文件流");
            }
            if (string.IsNullOrEmpty(oldName))
            {
                throw new BusException(ResultCode.FILE_OLDNAME_CANNOT_READ, "无法获取历史文件名称");
            }
            if (string.IsNullOrEmpty(path))
            {
                throw new BusException(ResultCode.FILE_UPLOAD_PATH_IS_NULL, "上传路径不能为空");
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                if (!Directory.Exists(path))
                {
                    throw new Exception("创建上传文件夹失败");
                }
            }
            string ext = Path.GetExtension(oldName);
            if (string.IsNullOrEmpty(ext))
            {
                throw new BusException(ResultCode.FILE_CONTENTTYPE_CANNOT_GET, $"获取文件类型失败，文件名称为：{oldName}");
            }
            string newName = $"{Guid.NewGuid()}{ext.ToLower()}";
            //获取绝对路径
            string filePath = Path.GetFullPath(Path.Combine(path, newName));
            stream.Position = 0;
            if (!await SaveAsAsync(stream, filePath))
            {
                throw new Exception($"保存文件【{oldName}】到【{filePath}】失败。");
            }
            //build upload result model
            UploadFileInfo upload = new UploadFileInfo()
            {
                Name = newName,
                OldName = oldName,
                Owner = owner,
                Path = filePath,
                Size = stream.Length / 1024,
                Url = null,
                StorageType = StorageType.Local,
                AccessMode = accessMode,
                ExpiredTime = expiredTime,
            };
            //save to db
            upload.Id = await SaveFileInfoToDatabase(upload);
            if (upload.Id == null)
            {
                //不直接抛异常，文件已经上传了
                await DeleteFileByPath(upload.Path);
            }
            //build url
            upload.Url = BuildFileUrl(upload);
            return upload;
        }

        #endregion

        #region UploadToOSS

        public async Task<UploadFileInfo> UploadToOSS(IFormFile file
            , string bucketName
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }
            List<IFormFile> upFiles = new List<IFormFile>() { file };
            var result = await UploadToOSS(upFiles, bucketName, path, owner, accessMode, expiredTime);
            if (result != null && result.Count > 0)
            {
                return result[0];
            }
            throw new BusException(ResultCode.FILE_UPLOAD_FAILED, "文件上传失败");
        }

        public async Task<List<UploadFileInfo>> UploadToOSS(IFormFileCollection files
            , string bucketName
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null)
        {
            if (files == null || files.Count == 0)
            {
                throw new ArgumentNullException(nameof(files));
            }
            List<IFormFile> upFiles = new List<IFormFile>();
            foreach (var item in files)
            {
                upFiles.Add(item);
            }
            return await UploadToOSS(upFiles, bucketName, path, owner, accessMode, expiredTime);
        }

        public async Task<List<UploadFileInfo>> UploadToOSS(List<IFormFile> files
            , string bucketName
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null)
        {
            if (files == null || files.Count == 0)
            {
                throw new BusException(ResultCode.FILE_UPLOAD_IS_NULL, "上传文件为空");
            }
            List<UploadFileInfo> result = new List<UploadFileInfo>();
            try
            {
                foreach (IFormFile item in files)
                {
                    using (var stream = item.OpenReadStream())
                    {
                        var upload = await UploadToOSS(stream, bucketName, item.FileName, path, owner, accessMode, expiredTime);
                        result.Add(upload);
                    }
                }
            }
            catch (Exception ex)
            {
                if (result.Count > 0)
                {
                    await DeleteFile(result);
                }
                _logger.LogError(ex, $"Upload file failed, error: {ex.Message}");
                throw;
            }
            return result;
        }

        public async Task<UploadFileInfo> UploadToOSS(string uploadFilePath
            , string bucketName
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null)
        {
            if (string.IsNullOrEmpty(uploadFilePath))
            {
                throw new BusException(ResultCode.FILE_UPLOAD_PATH_IS_NULL, "上传文件路径不能为空");
            }
            if (!File.Exists(uploadFilePath))
            {
                throw new BusException(ResultCode.FILE_UPLOAD_NOT_EXISTS, "上传文件不存在");
            }
            string oldName = Path.GetFileName(uploadFilePath);
            using (var stream = File.OpenRead(uploadFilePath))
            {
                var upload = await UploadToOSS(stream, bucketName, oldName, path, owner, accessMode, expiredTime);
                return upload;
            }
        }

        public async Task<UploadFileInfo> UploadToOSS(Stream stream
            , string bucketName
            , string oldName
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null)
        {
            if (stream == null || stream.Length == 0)
            {
                throw new BusException(ResultCode.FILE_STREAM_IS_NULL, "文件流为空");
            }
            if (stream.Length == 0)
            {
                throw new BusException(ResultCode.FILE_STREAM_IS_NULL, "文件流为空");
            }
            if (!stream.CanRead)
            {
                throw new BusException(ResultCode.FILE_STREAM_CANNOT_READ, "无法读取文件流");
            }
            if (string.IsNullOrEmpty(oldName))
            {
                throw new BusException(ResultCode.FILE_OLDNAME_CANNOT_READ, "无法获取历史文件名称");
            }
            string ext = Path.GetExtension(oldName);
            if (string.IsNullOrEmpty(ext))
            {
                throw new BusException(ResultCode.FILE_CONTENTTYPE_CANNOT_GET, $"获取文件类型失败，文件名称为：{oldName}");
            }
            string newName = $"{Path.GetFileNameWithoutExtension(oldName)}_{owner}_{_idGenerator.NewId()}{ext}";
            if (string.IsNullOrEmpty(path))
            {
                throw new BusException(ResultCode.FILE_UPLOAD_PATH_IS_NULL, "上传路径不能为空");
            }
            if (path.StartsWith("/"))
            {
                path = path.TrimStart('/');
            }
            if (!path.EndsWith("/"))
            {
                path += "/";
            }
            string newPath = path + newName;
            stream.Position = 0;
            //upload
            bool putResult = await _ossService.PutObjectAsync(bucketName, newPath, stream);
            if (!putResult)
            {
                throw new BusException(ResultCode.FILE_UPLOAD_TO_OSS_FAILED, $"上传文件【{newPath}】到对象储存失败");
            }
            stream.Position = 0;
            if (accessMode == FileAccessMode.PublicRead
                || accessMode == FileAccessMode.PublicReadAndWrite
                || accessMode == FileAccessMode.PublicWrite)
            {
                await _ossService.SetObjectAclAsync(bucketName, newPath, AccessMode.PublicRead);
            }
            UploadFileInfo upload = new UploadFileInfo()
            {
                Name = newName,
                OldName = oldName,
                Owner = owner,
                Path = newPath,
                Size = stream.Length / 1024,
                Url = null,
                StorageType = StorageType.OSS,
                BucketName = bucketName,
                AccessMode = accessMode,
                ExpiredTime = expiredTime,
            };
            upload.Id = await SaveFileInfoToDatabase(upload);
            if (upload.Id == null)
            {
                //删除已经上传的文件
                await _ossService.RemoveObjectAsync(bucketName, newPath);
                throw new Exception($"保存文件【{oldName}】到数据库失败。");
            }
            //build url
            upload.Url = BuildFileUrl(upload);
            return upload;
        }
        #endregion

        #region 删除文件

        public async Task DeleteFile(List<UploadFileInfo> files)
        {
            if (files == null || files.Count == 0)
            {
                return;
            }
            foreach (var item in files)
            {
                await DeleteFile(item);
            }
        }

        public async Task DeleteFile(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            UploadFileInfo fileInfo = DecodeFileKey(key);
            if (fileInfo == null)
            {
                throw new Exception("Decode file key to 'UploadFileInfo' object failed.");
            }
            if (fileInfo.Id != null && fileInfo.Id > 0)
            {
                await DeleteFile(fileInfo.Id.Value);
            }
            else
            {
                await DeleteFile(fileInfo);
            }
        }

        public async Task DeleteFile(long fileId)
        {
            var file = await _repository.Where(p => p.Id == fileId).FirstAsync();
            if (file == null)
            {
                return;
            }
            UploadFileInfo fileInfo = new UploadFileInfo()
            {
                Id = file.Id,
                Name = file.Name,
                OldName = file.OldName,
                Path = file.Path,
                Size = file.Size,
                Url = file.Url,
                Owner = file.CreatedUserId,
                StorageType = file.StorageType,
                AccessMode = file.AccessMode,
            };
            await DeleteFile(fileInfo);
        }

        public async Task DeleteFile(List<long> fileIds)
        {
            if (fileIds == null || fileIds.Count == 0)
            {
                return;
            }
            var files = await _repository.Where(p => fileIds.Contains(p.Id)).ToListAsync();
            if (files == null || files.Count == 0)
            {
                return;
            }
            foreach (var file in files)
            {
                UploadFileInfo fileInfo = new UploadFileInfo()
                {
                    Id = file.Id,
                    Name = file.Name,
                    OldName = file.OldName,
                    Path = file.Path,
                    Size = file.Size,
                    Url = file.Url,
                    Owner = file.CreatedUserId,
                    StorageType = file.StorageType,
                    BucketName = file.BucketName,
                    AccessMode = file.AccessMode,
                };
                await DeleteFile(fileInfo);
            }
        }

        public async Task DeleteFile(UploadFileInfo file)
        {
            if (file == null)
            {
                return;
            }
            switch (file.StorageType)
            {
                case StorageType.Local:
                    {
                        if (!string.IsNullOrEmpty(file.Path) && File.Exists(file.Path))
                        {
                            File.Delete(file.Path);
                        }
                    }
                    break;
                case StorageType.OSS:
                    {
                        if (!string.IsNullOrEmpty(file.Path) && !string.IsNullOrEmpty(file.BucketName))
                        {
                            await _ossService.RemoveObjectAsync(file.BucketName, file.Path);
                        }
                    }
                    break;
            }
            if (file.Id != null)
            {
                await _repository.DeleteAsync(file.Id.Value);
            }
        }

        private async Task DeleteFileByPath(string path, string bucketName = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                if (!File.Exists(path))
                {
                    return;
                }
                File.Delete(path);
            }
            else
            {
                await _ossService.RemoveObjectAsync(bucketName, path);
            }
        }

        #endregion

        public UploadFileInfo DecodeFileKey(string key)
        {
            string json = AES.AESDecrypt(key, _config.AppSettings.AESSecretKey, _config.AppSettings.AESVector);
            if (string.IsNullOrEmpty(json))
            {
                throw new BusException(ResultCode.FILE_KEY_DEC_FAILED, "文件密钥验证失败");
            }
            UploadFileInfo fileInfo = JsonUtil.DeserializeStringToObject<UploadFileInfo>(json);
            if (fileInfo == null)
            {
                throw new BusException(ResultCode.FILE_KEY_FORMAT_FAILED, "文件密钥验证失败");
            }
            return fileInfo;
        }

        public async Task<string> GetPresignedObjectUrl(string path, string bucketName, int expiredSecond)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Oss object path can not null.");
            }
            if (string.IsNullOrEmpty(bucketName))
            {
                bucketName = _config.FileUpload.BucketName;
            }
            if (expiredSecond < 600)
            {
                throw new Exception("Expired second can not less than 600 seconds.");
            }
            if (expiredSecond > 604800)  //7天
            {
                throw new Exception("Expired second can not moren than 604800 seconds.");
            }
            return await _ossService.PresignedGetObjectAsync(bucketName, path, expiredSecond);
        }

        #region Private

        private async Task<bool> SaveAsAsync(
            IFormFile formFile,
            string filename,
            CancellationToken cancellationToken = default)
        {
            using (var fileStream = new FileStream(filename, FileMode.Create))
            {
                using (var inputStream = formFile.OpenReadStream())
                {
                    await inputStream.CopyToAsync(fileStream, 80 * 1024, cancellationToken);
                }
            }
            if (File.Exists(filename))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> SaveAsAsync(
            Stream stream,
            string filename,
            CancellationToken cancellationToken = default)
        {
            using (var fileStream = new FileStream(filename, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream, 80 * 1024, cancellationToken);
                stream.Position = 0;
            }
            if (File.Exists(filename))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<long?> SaveFileInfoToDatabase(UploadFileInfo upload)
        {
            try
            {
                UpLoadFile saveInfo = await _repository.InsertAsync(new UpLoadFile()
                {
                    Id = _idGenerator.NewId(),
                    Name = upload.Name,
                    OldName = upload.OldName,
                    Path = upload.Path,
                    Content = null,
                    Size = upload.Size,
                    StorageType = upload.StorageType,
                    BucketName = upload.BucketName,
                    AccessMode = upload.AccessMode,
                    CreatedUserId = upload.Owner,
                    CreatedTime = DateTime.Now,
                });
                return saveInfo?.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Save upload file info to db failed, error: {ex.Message}");
                return null;
            }
        }

        private string GetLocalUploadPath()
        {
            //上传路径为空，使用系统默认路径
            if (string.IsNullOrEmpty(_config.FileUpload.FileUploadPath))
            {
                throw new Exception("文件上传路径未配置，请联系管理员。");
            }
            string path = _config.FileUpload.FileUploadPath;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = path.ConvertUnixPathToWindowsPath();
            }
            else
            {
                path = path.ConvertWindowsPathToUnixPath();
            }
            path = Path.Combine(Path.GetFullPath(path), DateTime.Now.ToString("yyyy-MM"));

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                if (!Directory.Exists(path))
                {
                    throw new Exception("创建上传文件夹失败");
                }
            }
            return path;
        }

        private string GetOSSUploadPath()
        {
            if (string.IsNullOrEmpty(_config.FileUpload.FileUploadPath))
            {
                throw new Exception("文件上传路径未配置，请联系管理员。");
            }
            string path = _config.FileUpload.FileUploadPath;
            if (!path.StartsWith('/'))
            {
                throw new Exception("对象存储文件上传路径仅支持绝对路径，如/data/upload。");
            }
            path += (path.EndsWith('/') ? DateTime.Now.ToString("yyyy-MM") : $"/{DateTime.Now.ToString("yyyy-MM")}") + "/";

            if (path.StartsWith("/"))
            {
                path = path.TrimStart('/');
            }
            if (!path.EndsWith("/"))
            {
                path += "/";
            }
            return path;
        }

        private void Director(string dir, List<string> list)
        {
            DirectoryInfo d = new DirectoryInfo(dir);
            FileInfo[] files = d.GetFiles();
            DirectoryInfo[] directs = d.GetDirectories();
            foreach (FileInfo f in files)
            {
                list.Add(Path.GetFullPath(f.FullName));
            }
            foreach (DirectoryInfo dd in directs)
            {
                Director(dd.FullName, list);
            }
        }

        private string BuildFileUrl(UpLoadFile item)
        {
            UploadFileInfo fileInfo = new UploadFileInfo()
            {
                Id = item.Id,
                Name = item.Name,
                OldName = item.OldName,
                Path = item.Path,
                Size = item.Size,
                Url = item.Url,
                Owner = item.CreatedUserId,
                StorageType = item.StorageType,
                AccessMode = item.AccessMode,
                BucketName = item.BucketName,
            };
            return BuildFileUrl(fileInfo);
        }

        private string BuildFileUrl(UploadFileInfo fileInfo)
        {
            UploadFileInfo parseInfo = TransExp<UploadFileInfo, UploadFileInfo>.Copy(fileInfo);
            parseInfo.Name = null;
            parseInfo.Url = null;
            parseInfo.Size = null;

            string json = JsonUtil.SerializeToString(parseInfo);
            string key = AES.AESEncrypt(json, _config.AppSettings.AESSecretKey, _config.AppSettings.AESVector);

            StringBuilder urlBuilder = new StringBuilder();
            urlBuilder.Append(_accessor.HttpContext.Request.Scheme);
            urlBuilder.Append("://");
            urlBuilder.Append(_accessor.HttpContext.Request.Host.ToString());
            urlBuilder.Append("/api/v1/file?key=");
            urlBuilder.Append(HttpUtil.UrlEncode(key));
            return urlBuilder.ToString();
        }

        #endregion
    }
}
