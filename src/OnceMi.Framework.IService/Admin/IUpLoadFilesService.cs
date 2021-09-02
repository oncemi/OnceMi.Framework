using Microsoft.AspNetCore.Http;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface IUpLoadFilesService : IBaseService<UpLoadFiles, long>
    {
        Task<UploadFileInfo> Upload(IFormFile file
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null);

        Task<List<UploadFileInfo>> Upload(IFormFileCollection files
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null);

        Task<List<UploadFileInfo>> Upload(List<IFormFile> files
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null);

        Task<UploadFileInfo> Upload(Stream stream
            , string oldName
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null);

        Task<UploadFileInfo> UploadToLocal(IFormFile file
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null);

        Task<List<UploadFileInfo>> UploadToLocal(IFormFileCollection files
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null);

        Task<List<UploadFileInfo>> UploadToLocal(List<IFormFile> files
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null);

        Task<UploadFileInfo> UploadToLocal(Stream stream
            , string oldName
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null);

        Task<UploadFileInfo> UploadToOSS(IFormFile file
            , string bucketName
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null);

        Task<List<UploadFileInfo>> UploadToOSS(IFormFileCollection files
            , string bucketName
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null);

        Task<List<UploadFileInfo>> UploadToOSS(List<IFormFile> files
            , string bucketName
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null);

        Task<UploadFileInfo> UploadToOSS(string uploadFilePath
            , string bucketName
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null);

        Task<UploadFileInfo> UploadToOSS(Stream stream
            , string bucketName
            , string oldName
            , string path
            , long owner
            , FileAccessMode accessMode = FileAccessMode.Private
            , DateTime? expiredTime = null);

        Task DeleteFile(List<UploadFileInfo> files);

        Task DeleteFile(string key);

        Task DeleteFile(long fileId);

        Task DeleteFile(List<long> fileIds);

        Task DeleteFile(UploadFileInfo file);

        UploadFileInfo DecodeFileKey(string key);

        /// <summary>
        /// 获取OSS文件下载链接
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        Task<string> GetPresignedObjectUrl(string path, string bucketName, int expiredSecond);

        /// <summary>
        /// 查询访问权限Select
        /// </summary>
        /// <returns></returns>
        List<ISelectResponse<string>> QueryAccessModes();

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IPageResponse<FileItemResponse>> Query(IPageRequest request);
    }
}
